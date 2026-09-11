using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zxw.Framework.AI.Models;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Persistence for a credential that cannot come from configuration — namely a key issued by
    /// the PKCE flow. A host that already has a secret store (keychain, vault, its own settings
    /// service) implements this and nothing else changes.
    /// </summary>
    public interface IOrcaCredentialPersistence
    {
        OrcaPersistedCredential Read();

        void Write(OrcaPersistedCredential credential);

        void Delete();
    }

    /// <summary>The on-disk shape. Contains the secret, so it is never logged or surfaced.</summary>
    public sealed class OrcaPersistedCredential
    {
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("generation")]
        public string Generation { get; set; }

        [JsonPropertyName("needs_reauth")]
        public bool NeedsReauth { get; set; }

        [JsonPropertyName("obtained_at_utc")]
        public DateTimeOffset ObtainedAtUtc { get; set; }
    }

    /// <summary>
    /// Default persistence: a per-user JSON file beside the application's own user-scoped data,
    /// which is where .NET desktop and CLI tooling already keeps per-user settings. The file is
    /// created with owner-only permissions where the platform supports it.
    /// </summary>
    public sealed class OrcaUserProfileCredentialPersistence : IOrcaCredentialPersistence
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly string _filePath;

        public OrcaUserProfileCredentialPersistence(string filePath = null)
        {
            _filePath = filePath ?? DefaultFilePath();
        }

        public string FilePath => _filePath;

        public static string DefaultFilePath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(root))
                root = Path.Combine(Path.GetTempPath(), "zxw-framework");

            return Path.Combine(root, "Zxw.Framework.AI", "orcarouter.credentials.json");
        }

        public OrcaPersistedCredential Read()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return null;

                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<OrcaPersistedCredential>(json, JsonOptions);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (JsonException)
            {
                // A corrupted store is treated as absent rather than fatal. The credential is
                // classified for reauthentication by the caller.
                return null;
            }
        }

        public void Write(OrcaPersistedCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_filePath, JsonSerializer.Serialize(credential, JsonOptions));
            TryRestrictToOwner(_filePath);
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch (IOException)
            {
                // Deleting an already-gone file is success.
            }
            catch (UnauthorizedAccessException)
            {
                // Surfaced through status rather than thrown from a Clear() call.
            }
        }

        private static void TryRestrictToOwner(string path)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return;

            try
            {
                const UnixFileMode ownerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                File.SetUnixFileMode(path, ownerOnly);
            }
            catch (Exception)
            {
                // Best effort: an unsupported filesystem must not break credential storage.
            }
        }
    }

    /// <summary>
    /// The project's credential store. Resolution order, highest first:
    /// <list type="number">
    /// <item>an explicitly configured <see cref="OrcaRouterOptions.ApiKey"/>;</item>
    /// <item>the <c>ORCAROUTER_API_KEY</c> environment variable;</item>
    /// <item>a persisted key from a previous PKCE authorization.</item>
    /// </list>
    /// This is how the host already passes secrets today; nothing new is introduced for the
    /// API-key path, and the PKCE path reuses the same resolution.
    /// </summary>
    public sealed class OrcaCredentialStore : IOrcaCredentialStore
    {
        private readonly OrcaRouterOptions _options;
        private readonly IOrcaCredentialPersistence _persistence;
        private readonly object _gate = new object();

        private OrcaCredential _active;
        private bool _needsReauth;

        public OrcaCredentialStore(
            OrcaRouterOptions options,
            IOrcaCredentialPersistence persistence = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _persistence = persistence;
        }

        public OrcaCredential Load()
        {
            lock (_gate)
            {
                if (_active != null && _active.HasKey)
                    return _active;

                _active = ResolveFromLayers();
                return _active;
            }
        }

        public OrcaCredentialStatus GetStatus()
        {
            var credential = Load();
            if (credential == null || !credential.HasKey)
            {
                return new OrcaCredentialStatus
                {
                    State = OrcaCredentialState.Missing,
                    Source = OrcaCredentialSource.None,
                    Message =
                        "No OrcaRouter credential is configured. Paste an sk-orca-… API key, set " +
                        $"{OrcaRouterOptions.ApiKeyEnvironmentVariable}, or use Connect with OrcaRouter."
                };
            }

            if (_needsReauth)
            {
                return new OrcaCredentialStatus
                {
                    State = OrcaCredentialState.NeedsReauth,
                    Source = credential.Source,
                    MaskedKey = OrcaSecretRedaction.Mask(credential.ApiKey),
                    AccountId = credential.AccountId,
                    Scope = credential.Scope,
                    Message =
                        "OrcaRouter rejected this credential (401). It must be replaced: reconnect " +
                        "the account or paste a new API key. OrcaRouter keys are durable and are not refreshed."
                };
            }

            return new OrcaCredentialStatus
            {
                State = OrcaCredentialState.Active,
                Source = credential.Source,
                MaskedKey = OrcaSecretRedaction.Mask(credential.ApiKey),
                AccountId = credential.AccountId,
                Scope = credential.Scope,
                Message = credential.Source == OrcaCredentialSource.Pkce
                    ? "Connected with OrcaRouter. The key is reused until it is revoked."
                    : "Using a configured OrcaRouter API key."
            };
        }

        /// <summary>Store a newly obtained credential and clear any reauthentication flag.</summary>
        public void Save(OrcaCredential credential)
        {
            if (credential == null || !credential.HasKey)
                throw new ArgumentException("A credential with a key is required.", nameof(credential));

            lock (_gate)
            {
                if (_persistence != null)
                {
                    _persistence.Write(new OrcaPersistedCredential
                    {
                        ApiKey = credential.ApiKey,
                        Source = credential.Source.ToString(),
                        AccountId = credential.AccountId,
                        Scope = credential.Scope,
                        Generation = credential.Generation,
                        NeedsReauth = false,
                        ObtainedAtUtc = DateTimeOffset.UtcNow
                    });
                }

                _active = credential;
                _needsReauth = false;
            }
        }

        /// <summary>
        /// Clear the stored credential. A value supplied by the environment or by host
        /// configuration is not ours to remove, so it stays resolvable and the status says so.
        /// </summary>
        public void Clear()
        {
            lock (_gate)
            {
                _persistence?.Delete();
                _active = null;
                _needsReauth = false;

                // Re-resolve: if configuration or the environment still supplies a key, that is
                // the credential now in effect and the caller is told rather than left guessing.
                _active = ResolveFromLayers();
            }
        }

        /// <summary>
        /// Terminal reauthentication. Only the exact credential generation that the relay rejected
        /// is marked; a late failure raised by a request carrying an older generation cannot
        /// poison a credential that has since been replaced.
        /// </summary>
        public bool MarkNeedsReauth(string rejectedGeneration)
        {
            lock (_gate)
            {
                var current = _active ?? ResolveFromLayers();
                if (current == null || !current.HasKey)
                    return false;

                if (!string.IsNullOrEmpty(rejectedGeneration) &&
                    !string.Equals(current.Generation, rejectedGeneration, StringComparison.Ordinal))
                {
                    // Stale failure: it belongs to a credential that is no longer in use.
                    return false;
                }

                _needsReauth = true;
                return true;
            }
        }

        public bool NeedsReauth
        {
            get
            {
                lock (_gate)
                {
                    return _needsReauth;
                }
            }
        }

        private OrcaCredential ResolveFromLayers()
        {
            // 1 + 2: the host's existing secret mechanisms. ResolveApiKey() already checks the
            // configured value first, then the environment variable.
            var configuredKey = _options.ResolveApiKey();
            if (!string.IsNullOrWhiteSpace(configuredKey))
            {
                return new OrcaCredential(
                    configuredKey,
                    OrcaCredentialSource.ApiKey,
                    generation: StableGeneration("api-key", configuredKey));
            }

            // 3: a key from a previous PKCE authorization.
            var persisted = _persistence?.Read();
            if (persisted != null && !string.IsNullOrWhiteSpace(persisted.ApiKey))
            {
                _needsReauth = persisted.NeedsReauth;
                var source = string.Equals(persisted.Source, nameof(OrcaCredentialSource.Pkce), StringComparison.Ordinal)
                    ? OrcaCredentialSource.Pkce
                    : OrcaCredentialSource.ApiKey;

                return new OrcaCredential(
                    persisted.ApiKey,
                    source,
                    persisted.AccountId,
                    persisted.Scope,
                    string.IsNullOrWhiteSpace(persisted.Generation)
                        ? StableGeneration("persisted", persisted.ApiKey)
                        : persisted.Generation);
            }

            return null;
        }

        /// <summary>
        /// A generation for credentials that have no exchange to mint one. Derived from the
        /// credential value, so it changes exactly when the credential changes.
        /// </summary>
        private static string StableGeneration(string kind, string secret)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(kind + ":" + secret));
                var builder = new System.Text.StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    builder.Append(b.ToString("x2"));
                return builder.ToString(0, 32);
            }
        }
    }

    /// <summary>The store contract, so hosts and tests can substitute their own.</summary>
    public interface IOrcaCredentialStore
    {
        OrcaCredential Load();

        OrcaCredentialStatus GetStatus();

        void Save(OrcaCredential credential);

        void Clear();

        /// <summary>
        /// Mark the credential which made a rejected request as needing reauthentication. Returns
        /// false when <paramref name="rejectedGeneration"/> is not the generation currently in use,
        /// so a late failure from a replaced credential cannot poison the current one.
        /// </summary>
        bool MarkNeedsReauth(string rejectedGeneration);

        bool NeedsReauth { get; }
    }
}

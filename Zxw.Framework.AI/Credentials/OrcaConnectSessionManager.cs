using System;
using System.Collections.Generic;
using System.Threading;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Owns the server-side "a sign-in is already in progress" lock for the OrcaRouter PKCE flow.
    /// <para>
    /// Every terminal path must release it: success, denial, exchange error, timeout, an explicit
    /// cancel, switching authentication method, closing the dialog, and <c>pagehide</c>. Attempts
    /// are generation-tagged so a late response from an abandoned attempt can never mutate the
    /// state of a newer one.
    /// </para>
    /// </summary>
    public sealed class OrcaConnectSessionManager
    {
        /// <summary>How long an unused authorization attempt stays alive before it self-releases.</summary>
        public static readonly TimeSpan AttemptLifetime = TimeSpan.FromMinutes(10);

        private static long _generationCounter;

        private readonly object _gate = new object();
        private readonly Dictionary<string, PendingAttempt> _pending =
            new Dictionary<string, PendingAttempt>(StringComparer.Ordinal);

        /// <summary>Begin an attempt, releasing any attempt that is still pending.</summary>
        public PendingAttempt Begin(OrcaPkceConnectSession session, string provider)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            lock (_gate)
            {
                Expire();
                // Starting a new sign-in, or switching provider, abandons the previous attempt
                // rather than leaving its lock held.
                _pending.Clear();

                var attempt = new PendingAttempt
                {
                    AttemptId = session.AttemptId,
                    Generation = Interlocked.Increment(ref _generationCounter),
                    Provider = provider ?? OrcaPkceCredentialProvider.ProviderId,
                    AuthorizeUrl = session.AuthorizeUrl,
                    Instructions = session.Instructions,
                    Session = session,
                    StartedUtc = DateTimeOffset.UtcNow
                };

                _pending[attempt.AttemptId] = attempt;
                return attempt;
            }
        }

        /// <summary>
        /// Look up an attempt and confirm it still belongs to the current generation. A stale
        /// attempt id from a superseded sign-in is refused.
        /// </summary>
        public bool TryGetCurrent(string attemptId, out PendingAttempt attempt)
        {
            lock (_gate)
            {
                Expire();
                attempt = null;
                if (string.IsNullOrEmpty(attemptId))
                    return false;

                if (!_pending.TryGetValue(attemptId, out var found))
                    return false;

                attempt = found;
                return true;
            }
        }

        /// <summary>Release the lock, whether the attempt finished or was abandoned.</summary>
        public void Release(string attemptId)
        {
            lock (_gate)
            {
                if (string.IsNullOrEmpty(attemptId))
                    return;

                _pending.Remove(attemptId);
            }
        }

        public bool IsBusy
        {
            get
            {
                lock (_gate)
                {
                    Expire();
                    return _pending.Count > 0;
                }
            }
        }

        /// <summary>Release attempts that nobody completed, so a lock cannot outlive its window.</summary>
        private void Expire()
        {
            var now = DateTimeOffset.UtcNow;
            var expired = new List<string>();
            foreach (var pair in _pending)
            {
                if (now - pair.Value.StartedUtc > AttemptLifetime)
                    expired.Add(pair.Key);
            }

            foreach (var key in expired)
                _pending.Remove(key);
        }

        public sealed class PendingAttempt
        {
            public string AttemptId { get; set; }

            /// <summary>Monotonic id for this sign-in. A response tagged with an older one is ignored.</summary>
            public long Generation { get; set; }

            public string Provider { get; set; }

            public string AuthorizeUrl { get; set; }

            public string Instructions { get; set; }

            public DateTimeOffset StartedUtc { get; set; }

            public OrcaPkceConnectSession Session { get; set; }
        }
    }
}

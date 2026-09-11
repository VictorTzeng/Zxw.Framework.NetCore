using System;
using Zxw.Framework.AI.Options;

namespace Zxw.Framework.UnitTest
{
    /// <summary>
    /// Test isolation for the ambient credential.
    /// <para>
    /// <see cref="OrcaRouterOptions.ResolveApiKey"/> honours the <c>ORCAROUTER_API_KEY</c>
    /// environment variable, which is correct product behaviour but means a developer machine with
    /// a key exported would silently satisfy tests that assert "no credential is configured".
    /// Tests suspend it and restore whatever was there.
    /// </para>
    /// The value itself is never read, asserted on, logged, or stored anywhere but in memory.
    /// </summary>
    internal static class OrcaTestEnvironment
    {
        /// <summary>Remove the ambient key, returning the prior value for restoration.</summary>
        public static string SuspendAmbientApiKey()
        {
            var existing = Environment.GetEnvironmentVariable(OrcaRouterOptions.ApiKeyEnvironmentVariable);
            Environment.SetEnvironmentVariable(OrcaRouterOptions.ApiKeyEnvironmentVariable, null);
            return existing;
        }

        /// <summary>Put the ambient key back exactly as it was found.</summary>
        public static void RestoreAmbientApiKey(string value)
        {
            if (value != null)
                Environment.SetEnvironmentVariable(OrcaRouterOptions.ApiKeyEnvironmentVariable, value);
        }
    }
}

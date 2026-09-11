using System;
using System.Security.Cryptography;
using System.Text;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// PKCE (RFC 7636) primitives. SHA-256 and base64url both come from the BCL; no dependency is
    /// added for either.
    /// </summary>
    public static class OrcaPkce
    {
        /// <summary>Entropy of the verifier, in bytes. 32 bytes → 43 base64url characters.</summary>
        public const int VerifierBytes = 32;

        /// <summary>Entropy of the CSRF <c>state</c> value, in bytes.</summary>
        public const int StateBytes = 16;

        /// <summary>Only S256 is ever sent. See <see cref="OrcaAuthEndpoints.BuildAuthorizeUrl"/>.</summary>
        public const string ChallengeMethod = "S256";

        /// <summary>
        /// A fresh verifier from the cryptographic RNG. Generated per authorization attempt and
        /// never reused, derived from a timestamp, or derived from anything guessable.
        /// </summary>
        public static string CreateVerifier() => Base64UrlEncode(RandomBytes(VerifierBytes));

        /// <summary>A fresh opaque <c>state</c> value from the cryptographic RNG.</summary>
        public static string CreateState() => Base64UrlEncode(RandomBytes(StateBytes));

        /// <summary>
        /// <c>base64url(sha256(verifier))</c> with the padding stripped, per RFC 7636 §4.2.
        /// </summary>
        public static string CreateChallenge(string verifier)
        {
            if (string.IsNullOrEmpty(verifier))
                throw new ArgumentException("PKCE verifier must not be empty.", nameof(verifier));

            using (var sha256 = SHA256.Create())
            {
                return Base64UrlEncode(sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
            }
        }

        /// <summary>
        /// Constant-time comparison of the returned <c>state</c> against the one that was sent.
        /// This is the only thing standing between the callback and a code somebody else's page
        /// dropped on it, so it must not short-circuit on the first differing byte.
        /// </summary>
        public static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null)
                return false;

            var a = Encoding.UTF8.GetBytes(left);
            var b = Encoding.UTF8.GetBytes(right);
            if (a.Length != b.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        public static string Base64UrlEncode(byte[] raw) =>
            Convert.ToBase64String(raw).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] RandomBytes(int count)
        {
            var buffer = new byte[count];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            return buffer;
        }
    }
}

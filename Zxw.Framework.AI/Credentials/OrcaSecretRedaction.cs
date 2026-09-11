using System;
using System.Collections.Generic;
using System.Text;

namespace Zxw.Framework.AI.Credentials
{
    /// <summary>
    /// Masking and scrubbing helpers so credential material cannot reach a log, an error message,
    /// a telemetry event, or a UI payload.
    /// </summary>
    public static class OrcaSecretRedaction
    {
        public const string Placeholder = "***";

        /// <summary>
        /// Short display form of a key: keeps the scheme prefix and the last four characters so a
        /// user can tell two keys apart, and nothing else.
        /// </summary>
        public static string Mask(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                return string.Empty;

            var value = secret.Trim();
            var prefix = "sk-orca-";
            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                var tail = value.Length > prefix.Length + 4 ? value.Substring(value.Length - 4) : string.Empty;
                return prefix + "…" + tail;
            }

            return value.Length <= 4 ? Placeholder : "…" + value.Substring(value.Length - 4);
        }

        /// <summary>
        /// Remove every known secret from a free-text message before it is surfaced or logged.
        /// Used on transport error bodies, which are the easiest place for a credential to leak.
        /// </summary>
        public static string Scrub(string message, params string[] secrets)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            var result = message;
            foreach (var secret in secrets ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(secret) || secret.Length < 6)
                    continue;
                result = result.Replace(secret, Placeholder);
            }

            // Also strip anything that merely looks like an OrcaRouter key, in case a body echoed
            // a value we were not handed.
            result = ScrubKeyShapedTokens(result);
            return result;
        }

        private static string ScrubKeyShapedTokens(string message)
        {
            const string marker = "sk-orca-";
            var index = message.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                return message;

            var builder = new StringBuilder(message.Length);
            var cursor = 0;
            while (index >= 0)
            {
                builder.Append(message, cursor, index - cursor);
                builder.Append(marker).Append(Placeholder);
                cursor = index + marker.Length;
                while (cursor < message.Length && IsKeyChar(message[cursor]))
                    cursor++;
                index = message.IndexOf(marker, cursor, StringComparison.Ordinal);
            }

            builder.Append(message, cursor, message.Length - cursor);
            return builder.ToString();
        }

        private static bool IsKeyChar(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
            c == '-' || c == '_';

        /// <summary>
        /// Minimal structural check only. The <c>sk-orca-</c> prefix is not proof the credential is
        /// valid; the first real request establishes that.
        /// </summary>
        public static bool LooksLikeOrcaKey(string secret) =>
            !string.IsNullOrWhiteSpace(secret) &&
            secret.Trim().StartsWith("sk-orca-", StringComparison.Ordinal) &&
            secret.Trim().Length > "sk-orca-".Length + 3;
    }
}

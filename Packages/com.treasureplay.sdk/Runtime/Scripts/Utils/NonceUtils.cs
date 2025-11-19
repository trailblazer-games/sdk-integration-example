using System;
using System.Linq;
using System.Security.Cryptography;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Utility for generating nonces for Play Integrity
    /// </summary>
    public static class NonceUtils
    {
        /// <summary>
        /// Generate a Base64 web-safe (URL-safe), no-wrap, no-padding nonce for Google Play Integrity API (16-500 bytes before Base64).
        /// </summary>
        public static string GenerateIntegrityNonceBase64(int byteLength = 32)
        {
            if (byteLength < 16) byteLength = 16;
            if (byteLength > 500) byteLength = 500;
            
            var bytes = new byte[byteLength];
            RandomNumberGenerator.Fill(bytes);
            
            // Start with standard Base64, then convert to web-safe and strip padding '=' per RFC 4648 §5
            var b64 = Convert.ToBase64String(bytes);
            var webSafe = b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return webSafe;
        }
    }
}

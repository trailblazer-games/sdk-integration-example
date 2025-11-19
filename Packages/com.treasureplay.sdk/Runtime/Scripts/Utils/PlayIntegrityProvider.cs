using System;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Provides Play Integrity token for SDK initialization
    /// </summary>
    public static class PlayIntegrityProvider
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string HelperClass = "com.treasureplay.sdk.UnityPlayIntegrityHelper";
#endif

        /// <summary>
        /// Fetches a Google Play Integrity token to be attached to the init request.
        /// Returns null if the token cannot be obtained.
        /// </summary>
        public static string FetchIntegrityTokenForInit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null)
                {
                    TreasurePlayLogger.Warning("[TreasurePlaySDK] Unable to fetch integrity token: currentActivity is null");
                    return null;
                }

                // Generate a Base64 nonce as required by Play Integrity
                var nonce = NonceUtils.GenerateIntegrityNonceBase64();

                using var helper = new AndroidJavaClass(HelperClass);
                var token = helper.CallStatic<string>("fetchIntegrityToken", activity, nonce);

                if (string.IsNullOrEmpty(token))
                {
                    TreasurePlayLogger.Warning("[TreasurePlaySDK] Play Integrity token fetch returned null or empty");
                }
                else
                {
                    TreasurePlayLogger.Log("[TreasurePlaySDK] Play Integrity token retrieved successfully");
                }
                
                return token;
            }
            catch (Exception ex)
            {
                TreasurePlayLogger.Warning($"[TreasurePlaySDK] Failed to fetch Play Integrity token: {ex.Message}");
                return null;
            }
#else
            return null;
#endif
        }
    }
}

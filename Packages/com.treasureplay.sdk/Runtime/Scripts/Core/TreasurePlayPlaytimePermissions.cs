using System;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Handles playtime tracking permissions for Android usage stats.
    /// Required for playtime quest tracking which monitors time spent in designated games.
    /// </summary>
    public sealed class TreasurePlayPlaytimePermissions
    {
        /// <summary>
        /// Event fired when the user has granted or denied playtime tracking permission
        /// </summary>
        public event Action<bool> OnPermissionResult;

        /// <summary>
        /// Checks if the app currently has playtime tracking permission (Usage Stats Access)
        /// </summary>
        /// <returns>True if permission is granted, false otherwise</returns>
        public bool HasPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var helperClass = new AndroidJavaClass("com.treasureplay.sdk.PlaytimePermissionHelper"))
                {
                    bool hasPermission = helperClass.CallStatic<bool>("hasUsageStatsPermission");
                    TreasurePlayLogger.Log($"[TreasurePlaySDK] Current permission status: {hasPermission}");
                    return hasPermission;
                }
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Error checking permission");
                TreasurePlayLogger.Exception(e);
                return false;
            }
#elif UNITY_IOS
            // iOS doesn't require special permissions for playtime tracking
            // Screen time tracking can be done through app state monitoring
            TreasurePlayLogger.Log("[TreasurePlaySDK] iOS doesn't require usage stats permission");
            return true;
#else
            TreasurePlayLogger.Warning("[TreasurePlaySDK] Not supported on this platform");
            return false;
#endif
        }

        /// <summary>
        /// Requests playtime tracking permission from the user.
        /// Opens the system settings screen where the user can grant Usage Stats Access.
        /// </summary>
        public void RequestPermission()
        {
            TreasurePlayLogger.Log("[TreasurePlaySDK] Requesting playtime tracking permission");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var helperClass = new AndroidJavaClass("com.treasureplay.sdk.PlaytimePermissionHelper"))
                {
                    // Set up the callback listener
                    var listener = new PlaytimePermissionListener(this);
                    helperClass.CallStatic("requestUsageStatsPermission", listener);
                    
                    TreasurePlayLogger.Log("[TreasurePlaySDK] Opened Usage Stats settings");
                }
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Error requesting permission");
                TreasurePlayLogger.Exception(e);
                OnPermissionResult?.Invoke(false);
            }
#elif UNITY_IOS
            // iOS doesn't require explicit permission
            TreasurePlayLogger.Log("[TreasurePlaySDK] iOS doesn't require permission request");
            OnPermissionResult?.Invoke(true);
#else
            TreasurePlayLogger.Warning("[TreasurePlaySDK] Permission request not supported on this platform");
            OnPermissionResult?.Invoke(false);
#endif
        }

        /// <summary>
        /// Internal method called by native code when permission status changes
        /// </summary>
        internal void NotifyPermissionResult(bool granted)
        {
            TreasurePlayLogger.Log($"[TreasurePlaySDK] Permission result: {granted}");
            OnPermissionResult?.Invoke(granted);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Android Java proxy listener for permission changes
        /// </summary>
        private class PlaytimePermissionListener : AndroidJavaProxy
        {
            private readonly TreasurePlayPlaytimePermissions _owner;

            public PlaytimePermissionListener(TreasurePlayPlaytimePermissions owner)
                : base("com.treasureplay.sdk.PlaytimePermissionCallback")
            {
                _owner = owner;
            }

            public void onPermissionResult(bool granted)
            {
                UnityEngine.Debug.Log($"[PlaytimePermissionListener] Native callback received: {granted}");
                
                // Use AndroidJavaObject to marshal to Unity main thread safely
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        _owner.NotifyPermissionResult(granted);
                    }));
                }
            }
        }
#endif
    }
}

using System;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// SDK Playtime Tracking
    /// </summary>
    public sealed class TreasurePlaySDKPlaytimeTracking
    {
        private readonly TreasurePlayUserIdentity _identity;
        private readonly TreasurePlaySettings _settings;
        private readonly TreasurePlayPlaytimePermissions _permissions;
        private readonly TreasurePlaySession _session;
        
        private bool _isTracking = false;

        public bool IsTracking => _isTracking;

        public TreasurePlaySDKPlaytimeTracking(
            TreasurePlayUserIdentity identity,
            TreasurePlaySettings settings,
            TreasurePlayPlaytimePermissions permissions,
            TreasurePlaySession session)
        {
            _identity = identity;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Starts tracking ALL installed apps
        /// </summary>
        /// <param name="monitorIntervalSeconds">How often to check usage (default: 180s = 3 minutes)</param>
        public void StartTracking(int monitorIntervalSeconds = 180)
        {
            if (_isTracking)
            {
                TreasurePlayLogger.Log("[TreasurePlaySDK] Already tracking");
                return;
            }

            if (!_permissions.HasPermission())
            {
                TreasurePlayLogger.Warning("[TreasurePlaySDK] Cannot start - missing usage stats permission");
                return;
            }

            if (!_session.IsValid)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Cannot start - no valid session. Call InitializeWithBackendAsync first");
                return;
            }

            TreasurePlayLogger.Log("[TreasurePlaySDK] Starting - will track ALL apps");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var helper = new AndroidJavaClass("com.treasureplay.sdk.SDKPlaytimeServiceHelper"))
                {
                    string apiUrl = _settings.ApiBaseUri?.ToString();
                    
                    if (string.IsNullOrEmpty(apiUrl))
                    {
                        TreasurePlayLogger.Error("[TreasurePlaySDK] API URL not configured");
                        return;
                    }

                    // Remove trailing slashes
                    apiUrl = apiUrl.TrimEnd('/');

                    TreasurePlayLogger.Log($"[TreasurePlaySDK] Config: tp_uid={_session.TpUid}, apiUrl={apiUrl}, interval={monitorIntervalSeconds}s");

                    helper.CallStatic(
                        "startPlaytimeTrackingService",
                        currentActivity,
                        _session.TpUid,                // tp_uid
                        _session.SessionToken,         // session_token
                        apiUrl,                        // apiUrl
                        monitorIntervalSeconds
                    );

                    _isTracking = true;
                    TreasurePlayLogger.Log("[TreasurePlaySDK] Service started - tracking ALL apps");
                }
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Failed to start");
                TreasurePlayLogger.Exception(e);
            }
#elif UNITY_IOS
            TreasurePlayLogger.Log("[TreasurePlaySDK] iOS not yet implemented");
#else
            TreasurePlayLogger.Warning("[TreasurePlaySDK] Not supported on this platform");
#endif
        }

        /// <summary>
        /// Stops tracking
        /// </summary>
        public void StopTracking()
        {
            if (!_isTracking)
            {
                return;
            }

            TreasurePlayLogger.Log("[TreasurePlaySDK] Stopping");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var helper = new AndroidJavaClass("com.treasureplay.sdk.SDKPlaytimeServiceHelper"))
                {
                    helper.CallStatic("stopPlaytimeTrackingService", currentActivity);
                    _isTracking = false;
                    TreasurePlayLogger.Log("[TreasurePlaySDK] Stopped");
                }
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Error stopping");
                TreasurePlayLogger.Exception(e);
            }
#endif
        }

        /// <summary>
        /// Forces an immediate flush of usage data
        /// </summary>
        public void FlushNow()
        {
            if (!_isTracking)
            {
                TreasurePlayLogger.Warning("[TreasurePlaySDK] Cannot flush - not tracking");
                return;
            }

            TreasurePlayLogger.Log("[TreasurePlaySDK] Flushing");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var helper = new AndroidJavaClass("com.treasureplay.sdk.SDKPlaytimeServiceHelper"))
                {
                    var service = helper.CallStatic<AndroidJavaObject>("getServiceInstance");
                    if (service != null)
                    {
                        service.Call("flushImmediately");
                    }
                }
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Error flushing");
                TreasurePlayLogger.Exception(e);
            }
#endif
        }
    }
}

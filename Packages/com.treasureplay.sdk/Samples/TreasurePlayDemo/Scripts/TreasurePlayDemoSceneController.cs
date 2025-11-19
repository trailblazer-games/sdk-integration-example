using System.Collections.Generic;
using TreasurePlay.SDK.Core;
using UnityEngine;

namespace TreasurePlay.SDK.Demo
{
    /// <summary>
    /// Demo example TreasurePlaySDK
    /// </summary>
    public sealed class TreasurePlayDemoSceneController : MonoBehaviour
    {
        [SerializeField]
        private TreasurePlaySettings _settingsOverride;

        [SerializeField]
        private string _demoCuid = "demo-cuid-001";

        [SerializeField]
        private string _demoFbAi = "demo-fbai-001";

        [Header("Playtime Permission Demo")]
        [SerializeField]
        private bool _checkPlaytimePermissionOnStart = false;

        private void Start()
        {
            // Initialize SDK with user identity
            var identity = new TreasurePlayUserIdentity.Builder()
                .WithDisplayName("Demo Player")
                .WithCuid(_demoCuid)
                .WithFbAi(_demoFbAi)
                .Build();
            
            TreasurePlaySDK.Instance.Initialize(identity, _settingsOverride, enableBackendSession: true);
            
            Debug.Log("[TreasurePlayDemo] SDK initialized - ready to open WebView and check rewards");

            // Optionally check playtime permission on start
            if (_checkPlaytimePermissionOnStart)
            {
                // Wait for backend initialization before starting playtime tracking
                StartCoroutine(WaitForInitAndCheckPermissions());
            }
        }

        private System.Collections.IEnumerator WaitForInitAndCheckPermissions()
        {
            // Wait until session is valid (backend init complete)
            while (!TreasurePlaySDK.Instance.Session.IsValid)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[TreasurePlayDemo] Backend initialized - checking playtime permissions");
            CheckPlaytimePermission();
        }

        /// <summary>
        /// Example method showing how to check and request playtime permissions
        /// </summary>
        public void CheckPlaytimePermission()
        {
            var permissions = TreasurePlaySDK.Instance.PlaytimePermissions;
            
            bool hasPermission = permissions.HasPermission();
            Debug.Log($"[TreasurePlayDemo] Playtime permission status: {hasPermission}");

            if (!hasPermission)
            {
                Debug.Log("[TreasurePlayDemo] Requesting playtime permission...");
                
                // Subscribe to permission result
                permissions.OnPermissionResult += OnPlaytimePermissionResult;
                
                // Request permission
                permissions.RequestPermission();
            }
            else
            {
                StartSDKPlaytimeTrackingDemo();
            }
        }

        private void OnPlaytimePermissionResult(bool granted)
        {
            Debug.Log($"[TreasurePlayDemo] Playtime permission result: {granted}");
            
            // Unsubscribe
            TreasurePlaySDK.Instance.PlaytimePermissions.OnPermissionResult -= OnPlaytimePermissionResult;

            if (granted)
            {
                Debug.Log("[TreasurePlayDemo] Playtime tracking enabled - ready to track quest progress!");
                
                // Always start tracking when permission is granted (if session is valid)
                if (TreasurePlaySDK.Instance.Session.IsValid)
                {
                    Debug.Log("[TreasurePlayDemo] Session valid - starting playtime tracking immediately");
                    StartSDKPlaytimeTrackingDemo();
                }
                else
                {
                    Debug.Log("[TreasurePlayDemo] Waiting for session to be valid...");
                    StartCoroutine(WaitForSessionAndStartTracking());
                }
            }
            else
            {
                Debug.Log("[TreasurePlayDemo] Playtime tracking denied - playtime quests won't work");
            }
        }
        
        private System.Collections.IEnumerator WaitForSessionAndStartTracking()
        {
            // Wait until session is valid
            while (!TreasurePlaySDK.Instance.Session.IsValid)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[TreasurePlayDemo] Session now valid - starting playtime tracking");
            StartSDKPlaytimeTrackingDemo();
        }

        /// <summary>
        /// Start SDK Playtime Tracking - Tracks ALL apps automatically
        /// </summary>
        public void StartSDKPlaytimeTrackingDemo()
        {
            Debug.Log("[TreasurePlayDemo] Starting SDK playtime tracking - ALL apps");
            
            // This service tracks ALL installed apps automatically
            TreasurePlaySDK.Instance.PlaytimeTracking.StartTracking(monitorIntervalSeconds: 30);
            
            Debug.Log("[TreasurePlayDemo] SDK playtime tracking started");
        }

        /// <summary>
        /// Stop SDK Playtime Tracking
        /// </summary>
        public void StopSDKPlaytimeTrackingDemo()
        {
            TreasurePlaySDK.Instance.PlaytimeTracking.StopTracking();
            Debug.Log("[TreasurePlayDemo] SDK playtime tracking stopped");
        }

        /// <summary>
        /// Flush SDK Playtime Data
        /// </summary>
        public void FlushSDKPlaytimeDataDemo()
        {
            TreasurePlaySDK.Instance.PlaytimeTracking.FlushNow();
            Debug.Log("[TreasurePlayDemo] SDK playtime data flushed");
        }
        
        private void OnApplicationQuit()
        {
            // Clean shutdown
            if (TreasurePlaySDK.Instance.PlaytimeTracking != null && 
                TreasurePlaySDK.Instance.PlaytimeTracking.IsTracking)
            {
                TreasurePlaySDK.Instance.PlaytimeTracking.FlushNow();
                TreasurePlaySDK.Instance.PlaytimeTracking.StopTracking();
            }
        }
    }
}

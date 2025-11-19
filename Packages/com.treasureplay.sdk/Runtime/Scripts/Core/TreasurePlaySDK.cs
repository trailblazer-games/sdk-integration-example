using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TreasurePlay.SDK.Networking;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    public sealed class TreasurePlaySDK : MonoBehaviour
    {
        private static TreasurePlaySDK _instance;

        private TreasurePlaySettings _settings;
        private TreasurePlayUserIdentity _identity;
        private ITreasurePlayApiClient _apiClient;
        private TreasurePlaySession _session;
        private TreasurePlayPlaytimePermissions _playtimePermissions;
        private TreasurePlaySDKPlaytimeTracking _sdkPlaytimeTracking;
        private TreasurePlayRewardManager _rewardManager;

        public static TreasurePlaySDK Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var existing = FindObjectOfType<TreasurePlaySDK>();
                if (existing != null)
                {
                    _instance = existing;
                    return _instance;
                }

                var go = new GameObject("TreasurePlaySDK");
                _instance = go.AddComponent<TreasurePlaySDK>();
                return _instance;
            }
        }

        public TreasurePlaySettings Settings => _settings;
        public TreasurePlaySession Session => _session;
        public TreasurePlayPlaytimePermissions PlaytimePermissions => _playtimePermissions;
        public TreasurePlaySDKPlaytimeTracking SDKPlaytimeTracking => _sdkPlaytimeTracking;
        public TreasurePlaySDKPlaytimeTracking PlaytimeTracking => _sdkPlaytimeTracking;
        public TreasurePlayRewardManager RewardManager => _rewardManager;
        public bool IsInitialized { get; private set; }
        public event Action<bool> WebViewVisibilityChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initializes the SDK.
        /// </summary>
        /// <param name="identity">User identity</param>
        /// <param name="overrideSettings">Settings override</param>
        /// <param name="enableBackendSession">Enable backend session for rewards and playtime tracking</param>
        public void Initialize(TreasurePlayUserIdentity identity, TreasurePlaySettings overrideSettings = null, bool enableBackendSession = false)
        {
            if (IsInitialized)
            {
                TreasurePlayLogger.SdkLog("TreasurePlaySDK is already initialized.");
                return;
            }

            if (identity.IsValid == false)
            {
                throw new ArgumentException("TreasurePlaySDK requires a valid user identity.");
            }

            _identity = identity;
            _settings = overrideSettings != null ? overrideSettings : TreasurePlaySettings.LoadFromResources();
            if (_settings == null)
            {
                throw new InvalidOperationException("TreasurePlaySettings asset could not be loaded.");
            }

            TreasurePlayLogger.Configure(_settings.MinimumLogType);

            if (enableBackendSession)
            {
                // Initialize full SDK with backend session, rewards, and playtime tracking
                _apiClient = new TreasurePlayApiClient(_settings, _identity, TreasurePlaySdkVersion.Get());
                _session = new TreasurePlaySession();
                _session.LoadFromStorage();
                _playtimePermissions = new TreasurePlayPlaytimePermissions();
                _sdkPlaytimeTracking = new TreasurePlaySDKPlaytimeTracking(_identity, _settings, _playtimePermissions, _session);
                _rewardManager = new TreasurePlayRewardManager(_apiClient, _session, _settings);

                IsInitialized = true;
                TreasurePlayLogger.SdkLog($"Treasure Play SDK initialized with backend session for {_identity}");
                
                _ = InitializeWithBackendAsync();
            }
            else
            {
                IsInitialized = true;
                TreasurePlayLogger.SdkLog($"Treasure Play SDK initialized (WebView-only) for {_identity}");
            }
        }


        public async Task<bool> InitializeWithBackendAsync(TreasurePlayInitRequest initRequest = null)
        {
            if (!IsInitialized)
            {
                TreasurePlayLogger.SessionError("SDK must be initialized before calling InitializeWithBackendAsync");
                return false;
            }

            var request = initRequest ?? TreasurePlayInitRequest.CreateBuilder()
                .WithCuid(_identity.Cuid)
                .WithFbAi(_identity.FbAi)
                .WithEmail(_identity.Email)
                .WithDisplayName(_identity.DisplayName)
                .WithLocale(_identity.Locale)
                .WithCountryCode(_identity.CountryCode)
                .WithApiKey(_settings.ApiKey)
                .Build();

            try
            {
                // Fetch Play Integrity token for Android
                Dictionary<string, string> headers = null;
#if UNITY_ANDROID && !UNITY_EDITOR
                var playIntegrityToken = PlayIntegrityProvider.FetchIntegrityTokenForInit();
                if (!string.IsNullOrEmpty(playIntegrityToken))
                {
                    headers = new Dictionary<string, string>
                    {
                        { "X-Play-Integrity-Token", playIntegrityToken }
                    };
                    TreasurePlayLogger.Log("[TreasurePlaySDK] Play Integrity token added to /init request");
                }
#endif

                var response = await _apiClient.InitializeAsync(request, headers, CancellationToken.None);
                if (response != null && response.IsValid)
                {
                    _session.SetSessionData(response.SessionToken, response.TpUid, response.WebViewUrl);

                    TreasurePlayLogger.SessionLog("Backend initialization successful");
                    TreasurePlayLogger.SessionLog($"Session Token: {response.SessionToken}");
                    return true;
                }
                else
                {
                    TreasurePlayLogger.SessionError("Backend initialization failed - invalid response");
                    return false;
                }
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return false;
            }
        }

        public void ShowQuestWebView(bool forceRefresh = false)
        {
            if (!IsInitialized)
            {
                TreasurePlayLogger.WebViewError("SDK must be initialized before showing webview");
                return;
            }
            
            var url = BuildWebViewUrl();
            
            TreasurePlayLogger.WebViewLog("========================================");
            TreasurePlayLogger.WebViewLog("Opening Quest WebView");
            TreasurePlayLogger.WebViewLog($"Complete URL: {url}");
            TreasurePlayLogger.WebViewLog($"Force Refresh: {forceRefresh}");
            TreasurePlayLogger.WebViewLog("========================================");
            
            TreasurePlayWebViewManager.ShowWebView(url, forceRefresh);
            WebViewVisibilityChanged?.Invoke(true);
        }

        private string BuildWebViewUrl()
        {
            var baseUrl = _settings.WebViewUrl;
            
            TreasurePlayLogger.WebViewLog($"Base URL: {baseUrl}");
            
            var parameters = new Dictionary<string, string>
            {
                { "advertisingId", _identity.FbAi },
                { "cuid", _identity.Cuid },
                { "appKey", _settings.ApiKey },
                { "deviceType", GetDeviceType() }
            };
            
            var queryParams = new List<string>();
            foreach (var kvp in parameters)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    var encoded = $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}";
                    queryParams.Add(encoded);
                    TreasurePlayLogger.WebViewLog($"Parameter: {kvp.Key} = {kvp.Value}");
                }
            }

            var queryString = string.Join("&", queryParams);
            var separator = baseUrl.Contains("?") ? "&" : "?";
            
            return $"{baseUrl}{separator}{queryString}";
        }

        private string GetDeviceType()
        {
            #if UNITY_ANDROID
                return "android";
            #elif UNITY_IOS
                return "ios";
            #else
                return "web";
            #endif
        }

        public void HideQuestWebView()
        {
            TreasurePlayWebViewManager.HideWebView();
            WebViewVisibilityChanged?.Invoke(false);
        }

        public bool IsWebViewVisible => TreasurePlayWebViewManager.IsWebViewVisible;

        /// <summary>
        /// Checks the available reward coins for the user.
        /// Requires enableBackendSession to be true during initialization.
        /// </summary>
        /// <returns>Amount of coins available, or -1 if request failed</returns>
        public async Task<int> CheckRewardsAsync()
        {
            if (_rewardManager == null)
            {
                TreasurePlayLogger.NetworkError("RewardManager is not available. Initialize SDK with enableBackendSession: true");
                return -1;
            }

            return await _rewardManager.CheckRewardsAsync();
        }

        /// <summary>
        /// Redeems all available coins. The SKU is determined by the app source ID in the session token.
        /// Requires enableBackendSession to be true during initialization.
        /// </summary>
        /// <param name="message">Optional message to include with the redemption</param>
        /// <returns>Updated balance after redemption, or -1 if redemption failed</returns>
        public async Task<int> RedeemAsync(string message = "")
        {
            if (_rewardManager == null)
            {
                TreasurePlayLogger.NetworkError("RewardManager is not available. Initialize SDK with enableBackendSession: true");
                return -1;
            }

            return await _rewardManager.RedeemAsync(message);
        }

        private void ExecuteAsync(Func<Task> routine)
        {
            async void Wrapper()
            {
                try
                {
                    await routine().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    TreasurePlayLogger.Exception(exception);
                }
            }

            Wrapper();
        }
    }
}

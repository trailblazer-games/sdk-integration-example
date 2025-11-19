using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Manages native webview integration for displaying quest content
    /// </summary>
    public sealed class TreasurePlayWebViewManager
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass _unityPlayer;
        private static AndroidJavaObject _currentActivity;
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _TreasurePlay_ShowWebView(string url, bool forceRefresh);
        
        [DllImport("__Internal")]
        private static extern void _TreasurePlay_HideWebView();
        
        [DllImport("__Internal")]
        private static extern bool _TreasurePlay_IsWebViewVisible();
#endif

        public static bool IsWebViewVisible { get; private set; }

        public static void ShowWebView(string url, bool forceRefresh = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                TreasurePlayLogger.Error("[TreasurePlaySDK] Cannot show webview with empty URL");
                return;
            }

            TreasurePlayLogger.Log($"[TreasurePlaySDK] Showing webview with URL: {url}");

#if UNITY_EDITOR
            // In editor, open URL in default browser
            Application.OpenURL(url);
            IsWebViewVisible = true;
#elif UNITY_ANDROID
            ShowWebViewAndroid(url, forceRefresh);
#elif UNITY_IOS
            ShowWebViewIOS(url, forceRefresh);
#else
            TreasurePlayLogger.Warning("[TreasurePlaySDK] WebView not supported on this platform");
#endif
        }

        public static void HideWebView()
        {
            TreasurePlayLogger.Log("[TreasurePlaySDK] Hiding webview");

#if UNITY_EDITOR
            // In editor, we can't hide the browser window
            IsWebViewVisible = false;
#elif UNITY_ANDROID
            HideWebViewAndroid();
#elif UNITY_IOS
            HideWebViewIOS();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void ShowWebViewAndroid(string url, bool forceRefresh)
        {
            try
            {
                if (_unityPlayer == null)
                {
                    _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                }

                _currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        // Create intent for WebView activity
                        var intent = new AndroidJavaObject("android.content.Intent", _currentActivity, new AndroidJavaClass("com.treasureplay.sdk.TreasurePlayWebViewActivity"));
                        intent.Call<AndroidJavaObject>("putExtra", "url", url);
                        intent.Call<AndroidJavaObject>("putExtra", "forceRefresh", forceRefresh);
                        _currentActivity.Call("startActivity", intent);
                        IsWebViewVisible = true;
                    }
                    catch (Exception e)
                    {
                        TreasurePlayLogger.Error($"[TreasurePlaySDK] Android WebView error: {e.Message}");
                    }
                }));
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error($"[TreasurePlaySDK] Failed to show Android WebView: {e.Message}");
            }
        }

        private static void HideWebViewAndroid()
        {
            try
            {
                // For Android, we can't programmatically close the activity from Unity
                // The user will need to use the close button in the WebView activity
                IsWebViewVisible = false;
                TreasurePlayLogger.Log("[TreasurePlaySDK] Android WebView hide requested - user must close manually");
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error($"[TreasurePlaySDK] Failed to hide Android WebView: {e.Message}");
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private static void ShowWebViewIOS(string url, bool forceRefresh)
        {
            try
            {
                _TreasurePlay_ShowWebView(url, forceRefresh);
                IsWebViewVisible = true;
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error($"[TreasurePlaySDK] Failed to show iOS WebView: {e.Message}");
            }
        }

        private static void HideWebViewIOS()
        {
            try
            {
                _TreasurePlay_HideWebView();
                IsWebViewVisible = false;
            }
            catch (Exception e)
            {
                TreasurePlayLogger.Error($"[TreasurePlaySDK] Failed to hide iOS WebView: {e.Message}");
            }
        }
#endif
    }
}

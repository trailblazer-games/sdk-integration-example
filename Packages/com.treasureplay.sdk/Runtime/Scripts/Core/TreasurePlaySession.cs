using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Manages session data for TreasurePlay SDK
    /// </summary>
    public sealed class TreasurePlaySession
    {
        private const string SessionTokenKey = "TreasurePlay_SessionToken";
        private const string TpUidKey = "TreasurePlay_TpUid";
        private const string WebViewUrlKey = "TreasurePlay_WebViewUrl";

        public string SessionToken { get; private set; }
        public string TpUid { get; private set; }
        public string TreasurePlayWebViewUrl { get; private set; }
        public bool IsValid => !string.IsNullOrEmpty(SessionToken) && !string.IsNullOrEmpty(TpUid);

        public void SetSessionData(string sessionToken, string tpUid, string webViewUrl)
        {
            SessionToken = sessionToken;
            TpUid = tpUid;
            TreasurePlayWebViewUrl = webViewUrl;

            // Persist to PlayerPrefs for session restoration
            PlayerPrefs.SetString(SessionTokenKey, sessionToken);
            PlayerPrefs.SetString(TpUidKey, tpUid);
            PlayerPrefs.SetString(WebViewUrlKey, webViewUrl);
            PlayerPrefs.Save();

            TreasurePlayLogger.SessionLog($"Session data saved - TP UID: {tpUid}");
        }

        public void LoadFromStorage()
        {
            var token = PlayerPrefs.GetString(SessionTokenKey, string.Empty);
            var tpUid = PlayerPrefs.GetString(TpUidKey, string.Empty);
            var webViewUrl = PlayerPrefs.GetString(WebViewUrlKey, string.Empty);

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(tpUid))
            {
                SessionToken = token;
                TpUid = tpUid;
                TreasurePlayWebViewUrl = webViewUrl;
                TreasurePlayLogger.SessionLog($"Session restored from storage - TP UID: {tpUid}");
                return;
            }

            // Clear invalid session data
            ClearSessionData();
        }

        public void ClearSessionData()
        {
            SessionToken = string.Empty;
            TpUid = string.Empty;
            TreasurePlayWebViewUrl = string.Empty;

            PlayerPrefs.DeleteKey(SessionTokenKey);
            PlayerPrefs.DeleteKey(TpUidKey);
            PlayerPrefs.DeleteKey(WebViewUrlKey);
            PlayerPrefs.Save();

            TreasurePlayLogger.SessionLog("Session data cleared");
        }
    }
}

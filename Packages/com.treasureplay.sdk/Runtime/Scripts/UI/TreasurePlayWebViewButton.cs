using TreasurePlay.SDK.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TreasurePlay.SDK.UI
{
    /// <summary>
    /// Simple button component that opens the quest webview.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class TreasurePlayWebViewButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            if (TreasurePlaySDK.Instance == null || !TreasurePlaySDK.Instance.IsInitialized)
            {
                TreasurePlayLogger.Error("SDK not initialized. Cannot open quest webview.");
                return;
            }
            
            TreasurePlaySDK.Instance.ShowQuestWebView();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }
    }
}

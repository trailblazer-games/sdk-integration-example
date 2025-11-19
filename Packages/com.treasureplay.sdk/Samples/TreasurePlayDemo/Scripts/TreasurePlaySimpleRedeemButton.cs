using UnityEngine;
using UnityEngine.UI;
using TreasurePlay.SDK.Core;

namespace TreasurePlay.SDK.Demo
{
    /// <summary>
    /// Simple button to redeem all available coins
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TreasurePlaySimpleRedeemButton : MonoBehaviour
    {
        [Header("Redemption Settings")]
        [SerializeField]
        [Tooltip("Optional message to include with the redemption")]
        private string message = "Redemption from Unity";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private async void OnButtonClicked()
        {
            // Disable button while redeeming
            _button.interactable = false;
            
            Debug.Log($"[SimpleRedeem] Redeeming all available coins...");
            
            // Call the SDK to redeem (empties the account)
            int newBalance = await TreasurePlaySDK.Instance.RedeemAsync(message);
            
            if (newBalance >= 0)
            {
                Debug.Log($"[SimpleRedeem] Success! New balance: {newBalance}");
            }
            else
            {
                Debug.LogError($"[SimpleRedeem] Failed to redeem");
            }
            
            // Re-enable button
            _button.interactable = true;
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}

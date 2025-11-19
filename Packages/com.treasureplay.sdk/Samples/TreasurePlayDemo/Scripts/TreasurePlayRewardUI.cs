using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TreasurePlay.SDK.Core;

namespace TreasurePlay.SDK.Demo
{
    /// <summary>
    /// Simple UI for testing TreasurePlay Rewards
    /// </summary>
    public class TreasurePlayRewardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private TextMeshProUGUI balanceText;
        
        [SerializeField]
        private Button checkRewardsButton;
        
        [SerializeField]
        private TreasurePlaySettings _settingsOverride;

        private void Start()
        {
            if (checkRewardsButton != null)
            {
                checkRewardsButton.onClick.AddListener(OnCheckRewardsClicked);
            }
            
            UpdateBalanceText("Not checked yet");
        }

        private async void OnCheckRewardsClicked()
        {
            try
            {
                if (checkRewardsButton != null)
                {
                    checkRewardsButton.interactable = false;
                }
            
                UpdateBalanceText("Checking...");
                Debug.Log("[RewardUI] Checking rewards...");
            
                var balance = await TreasurePlaySDK.Instance.CheckRewardsAsync();
            
                if (balance >= 0)
                {
                    UpdateBalanceText($"{_settingsOverride.CoinId}: {balance}");
                    Debug.Log($"[RewardUI] Balance: {balance}");
                }
                else
                {
                    UpdateBalanceText("Check failed");
                    Debug.LogError("[RewardUI] Failed to check rewards");
                }
            
                if (checkRewardsButton != null)
                {
                    checkRewardsButton.interactable = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RewardUI] Exception while checking rewards: {e}");
                UpdateBalanceText("Error occurred");
            }
        }

        private void UpdateBalanceText(string text)
        {
            if (balanceText != null)
            {
                balanceText.text = text;
            }
        }
    }
}

using System.Threading.Tasks;
using TreasurePlay.SDK.Core;
using UnityEngine;

namespace TreasurePlay.SDK.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Reward/Inventory system.
    /// This shows how to check rewards and redeem them.
    /// </summary>
    public class TreasurePlayRewardExample : MonoBehaviour
    {
        [Header("Redemption Settings")]
        [SerializeField]
        [Tooltip("Optional message to include with the redemption")]
        private string redemptionMessage = "Redeemed from game";

        private async void Start()
        {
            // Wait a bit for SDK initialization
            await System.Threading.Tasks.Task.Delay(2000);
            
            // Check for available rewards
            await CheckRewards();
        }

        /// <summary>
        /// Checks for available reward coins.
        /// </summary>
        public async Task CheckRewards()
        {
            Debug.Log("[RewardExample] Checking for available rewards...");
            
            // Get available coins
            int coinsAvailable = await TreasurePlaySDK.Instance.CheckRewardsAsync();
            
            if (coinsAvailable < 0)
            {
                Debug.LogWarning("[RewardExample] Failed to check rewards");
                return;
            }
            
            Debug.Log($"[RewardExample] Available coins: {coinsAvailable}");
            
            if (coinsAvailable > 0)
            {
                // Show the available coins to the player
                ShowCoinsToPlayer(coinsAvailable);
            }
        }

        /// <summary>
        /// Redeems all available coins. The SKU is determined by the app source ID in the session token.
        /// This empties the entire account balance.
        /// Call this when the player wants to redeem their reward coins.
        /// </summary>
        public async Task RedeemCoins()
        {
            Debug.Log($"[RewardExample] Attempting to redeem all available coins");
            
            // Redeem all coins (empties the account)
            int updatedBalance = await TreasurePlaySDK.Instance.RedeemAsync(redemptionMessage);
            
            if (updatedBalance < 0)
            {
                Debug.LogError("[RewardExample] Redemption failed");
                ShowRedemptionError();
                return;
            }
            
            Debug.Log($"[RewardExample] Redemption successful!");
            Debug.Log($"[RewardExample] Updated balance: {updatedBalance}");
            
            // Update the player's UI with new balance
            ShowCoinsToPlayer(updatedBalance);
            
            // Give the player their in-game item/reward
            GivePlayerReward();
        }

        /// <summary>
        /// Shows the available coins to the player in your game UI.
        /// Replace this with your actual game UI logic.
        /// </summary>
        private void ShowCoinsToPlayer(int coins)
        {
            Debug.Log($"[RewardExample] Showing {coins} coins to player");
            // TODO: Update your game's UI to show available coins
            // Example: UIManager.Instance.UpdateCoinDisplay(coins);
        }

        /// <summary>
        /// Gives the player their in-game reward after successful redemption.
        /// Replace this with your actual game logic.
        /// </summary>
        private void GivePlayerReward()
        {
            Debug.Log("[RewardExample] Giving player their reward");
            // TODO: Give player their in-game item/currency
            // Example: PlayerInventory.Instance.AddItem("special_item");
        }

        /// <summary>
        /// Shows an error message to the player.
        /// Replace this with your actual game UI logic.
        /// </summary>
        private void ShowRedemptionError()
        {
            Debug.LogError("[RewardExample] Showing error to player");
            // TODO: Show error message in your game UI
            // Example: UIManager.Instance.ShowError("Not enough coins to redeem");
        }

        /// <summary>
        /// Example: Check for rewards when player returns to the game.
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                _ = CheckRewards();
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using TreasurePlay.SDK.Networking;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Manages reward/inventory operations for in-game items distribution.
    /// </summary>
    public sealed class TreasurePlayRewardManager
    {
        private readonly ITreasurePlayApiClient _apiClient;
        private readonly TreasurePlaySession _session;
        private readonly TreasurePlaySettings _settings;

        public TreasurePlayRewardManager(ITreasurePlayApiClient apiClient, TreasurePlaySession session, TreasurePlaySettings settings)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Checks the available reward coins for the user.
        /// </summary>
        /// <returns>Amount of coins available, or -1 if request failed</returns>
        public async Task<int> CheckRewardsAsync()
        {
            if (!_session.IsValid)
            {
                TreasurePlayLogger.NetworkError("Cannot check rewards - session is not valid. Call InitializeWithBackendAsync first.");
                return -1;
            }

            if (string.IsNullOrEmpty(_settings.CoinId))
            {
                TreasurePlayLogger.NetworkError("Cannot check rewards - CoinSkuId is not configured in TreasurePlaySettings.");
                return -1;
            }

            try
            {
                TreasurePlayLogger.NetworkLog("[RewardManager] Checking rewards from backend");
                
                var response = await _apiClient.GetInventoryAsync(_settings.CoinId, _session.SessionToken, CancellationToken.None);
                
                if (response != null && response.IsValid)
                {
                    int tokenAmount = response.GetTokenAmount();
                    TreasurePlayLogger.NetworkLog($"[RewardManager] Available tokens: {response.tokens} ({tokenAmount})");
                    TreasurePlayLogger.NetworkLog($"[RewardManager] Token type: {response.tokenType}");
                    return tokenAmount;
                }

                TreasurePlayLogger.NetworkError("[RewardManager] Failed to check rewards - invalid response");
                return -1;
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return -1;
            }
        }

        /// <summary>
        /// Redeems all available coins. The SKU is determined by the app source ID in the session token.
        /// </summary>
        /// <param name="message">Optional message to include with the redemption</param>
        /// <returns>Updated balance after redemption, or -1 if redemption failed</returns>
        public async Task<int> RedeemAsync(string message = "")
        {
            if (!_session.IsValid)
            {
                TreasurePlayLogger.NetworkError("[RewardManager] Cannot redeem - session is not valid");
                return -1;
            }

            try
            {
                TreasurePlayLogger.NetworkLog($"[RewardManager] Redeeming all available coins");
                if (!string.IsNullOrEmpty(message))
                {
                    TreasurePlayLogger.NetworkLog($"[RewardManager] Message: {message}");
                }
                
                var response = await _apiClient.RedeemAsync(message, _session.SessionToken, CancellationToken.None);
                
                if (response != null && response.IsValid)
                {
                    int updatedBalance = response.GetUpdatedBalance();
                    TreasurePlayLogger.NetworkLog($"[RewardManager] Redemption successful");
                    TreasurePlayLogger.NetworkLog($"[RewardManager] Updated balance: {response.updatedBalance} ({updatedBalance})");
                    if (!string.IsNullOrEmpty(response.message))
                    {
                        TreasurePlayLogger.NetworkLog($"[RewardManager] Message: {response.message}");
                    }
                    return updatedBalance;
                }
                else
                {
                    TreasurePlayLogger.NetworkError($"[RewardManager] Redemption failed: {response?.message ?? "Unknown error"}");
                    return -1;
                }
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return -1;
            }
        }
    }
}

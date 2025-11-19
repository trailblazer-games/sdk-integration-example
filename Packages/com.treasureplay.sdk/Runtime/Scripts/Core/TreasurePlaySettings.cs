using System;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    [CreateAssetMenu(fileName = "TreasurePlaySettings", menuName = "Treasure Play/Settings", order = 0)]
    public sealed class TreasurePlaySettings : ScriptableObject
    {
        private const string ResourcesPath = "TreasurePlaySettings";
        private const string ProductionApiUrl = "https://api.treasureplay.com";
        private const string TestnetApiUrl = "https://testnet.api.treasureplay.com";
        private const string ProductionInventoryUrl = "https://inventory.treasureplay.com";
        private const string TestnetInventoryUrl = "https://testnet.inventory.treasureplay.com";
        private const string WebViewUrlConst = "https://portal.treasureplay.com/quests";

        [Header("Configuration")]
        [SerializeField]
        private string apiKey = "";

        [SerializeField]
        private TreasurePlayEnvironment environment = TreasurePlayEnvironment.Testnet;

        [Header("Rewards")]
        [SerializeField]
        [Tooltip("The ID for the in-game coin/currency (e.g., TP_COINS)")]
        private string coinId = "";

        [Header("Logging")]
        [SerializeField]
        private LogType minimumLogType = LogType.Log;

        public string ApiKey => apiKey;

        public TreasurePlayEnvironment Environment => environment;

        public string CoinId => coinId;

        public Uri ApiBaseUri => TryCreateUri(environment == TreasurePlayEnvironment.Production ? ProductionApiUrl : TestnetApiUrl);

        public Uri InventoryBaseUri => TryCreateUri(environment == TreasurePlayEnvironment.Production ? ProductionInventoryUrl : TestnetInventoryUrl);

        public string WebViewUrl => WebViewUrlConst;

        public LogType MinimumLogType => minimumLogType;
        
        // Readonly properties for display in inspector
        public string ProductionUrl => ProductionApiUrl;
        public string TestnetUrl => TestnetApiUrl;
        public string ProductionInventoryUrlDisplay => ProductionInventoryUrl;
        public string TestnetInventoryUrlDisplay => TestnetInventoryUrl;

        public static TreasurePlaySettings LoadFromResources()
        {
            var settings = Resources.Load<TreasurePlaySettings>(ResourcesPath);
            if (settings == null)
            {
                TreasurePlayLogger.Warning(
                    $"TreasurePlaySettings.asset not found in Resources. Create one via Treasure Play ▸ Create Settings Asset.");
            }

            return settings;
        }

        private static Uri TryCreateUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return null;
            }

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            {
                TreasurePlayLogger.Warning($"Invalid URI configured: {uri}");
                return null;
            }

            return parsed;
        }
    }
}

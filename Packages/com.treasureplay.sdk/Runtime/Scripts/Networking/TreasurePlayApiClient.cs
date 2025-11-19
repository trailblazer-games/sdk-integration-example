using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TreasurePlay.SDK.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace TreasurePlay.SDK.Networking
{
    internal sealed class TreasurePlayApiClient : ITreasurePlayApiClient
    {
        private const string JsonContentType = "application/json";

        private readonly TreasurePlaySettings _settings;
        private readonly TreasurePlayUserIdentity _identity;
        private readonly string _sdkVersion;

        internal TreasurePlayApiClient(TreasurePlaySettings settings, TreasurePlayUserIdentity identity, string sdkVersion)
        {
            _settings = settings;
            _identity = identity;
            _sdkVersion = sdkVersion;
        }

        public async Task<TreasurePlayInitResponse> InitializeAsync(
            TreasurePlayInitRequest initRequest, 
            IDictionary<string, string> additionalHeaders, 
            CancellationToken cancellationToken)
        {
            if (_settings.ApiBaseUri == null)
            {
                TreasurePlayLogger.NetworkError("API base URI is not configured.");
                return null;
            }

            var path = "/init";
            var uri = new Uri(_settings.ApiBaseUri, path);
            using var request = UnityWebRequest.PostWwwForm(uri, "");
            
            // Create request DTO and serialize to JSON
            var requestDto = InitRequestDto.FromInitRequest(initRequest);
            var requestBody = JsonUtility.ToJson(requestDto, true);
            
            // Debug logging
            TreasurePlayLogger.NetworkLog($"POST {uri}");
            TreasurePlayLogger.NetworkLog($"Request Body:\n{requestBody}");
            
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            ApplyHeaders(request);
            
            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                    TreasurePlayLogger.NetworkLog($"Added header: {header.Key}");
                }
            }

            try
            {
                var response = await request.SendWebRequestAsync(cancellationToken);
                var payload = response.downloadHandler.text;
                
                if (response.result == UnityWebRequest.Result.Success)
                {
                    return ParseInitResponse(payload);
                }
                else
                {
                    TreasurePlayLogger.NetworkError($"Init request failed: {response.error}");
                    return null;
                }
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return null;
            }
        }
        
        private void ApplyHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Authorization", $"Bearer {_settings.ApiKey}");
            request.SetRequestHeader("Accept", JsonContentType);
        }

        private static string Escape(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static TreasurePlayInitResponse ParseInitResponse(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    TreasurePlayLogger.NetworkError("Init response is empty");
                    return null;
                }

                // Deserialize the DTO
                var dto = JsonUtility.FromJson<InitResponseDto>(json);
                
                if (dto == null)
                {
                    TreasurePlayLogger.NetworkError("Failed to deserialize init response - dto is null");
                    return null;
                }

                if (dto.data == null)
                {
                    TreasurePlayLogger.NetworkError("Init response missing data object");
                    return null;
                }

                if (string.IsNullOrEmpty(dto.data.session_token))
                {
                    TreasurePlayLogger.NetworkError("Init response missing session_token");
                    return null;
                }

                // Convert DTO to business model
                return new TreasurePlayInitResponse(dto);
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.NetworkError($"Failed to parse init response: {exception.Message}");
                return null;
            }
        }

        public async Task<TreasurePlayInventoryResponse> GetInventoryAsync(string coinSkuId, string sessionToken, CancellationToken cancellationToken)
        {
            if (_settings.InventoryBaseUri == null)
            {
                TreasurePlayLogger.NetworkError("Inventory base URI is not configured.");
                return null;
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                TreasurePlayLogger.NetworkError("Session token is required for GetInventory");
                return null;
            }

            if (string.IsNullOrEmpty(coinSkuId))
            {
                TreasurePlayLogger.NetworkError("Coin SKU ID is required for GetInventory");
                return null;
            }

            var path = $"/token/{coinSkuId}";
            var uri = new Uri(_settings.InventoryBaseUri, path);
            using var request = UnityWebRequest.Get(uri);
            
            TreasurePlayLogger.NetworkLog($"GET {uri}");
            
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", sessionToken);

            try
            {
                var response = await request.SendWebRequestAsync(cancellationToken);
                var payload = response.downloadHandler.text;
                
                TreasurePlayLogger.NetworkLog($"GetInventory response: {payload}");
                
                if (response.result == UnityWebRequest.Result.Success)
                {
                    var inventoryResponse = JsonUtility.FromJson<TreasurePlayInventoryResponse>(payload);
                    return inventoryResponse;
                }
                else
                {
                    TreasurePlayLogger.NetworkError($"GetInventory request failed: {response.error}");
                    return null;
                }
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return null;
            }
        }

        public async Task<TreasurePlayRedeemResponse> RedeemAsync(string message, string sessionToken, CancellationToken cancellationToken)
        {
            if (_settings.InventoryBaseUri == null)
            {
                TreasurePlayLogger.NetworkError("Inventory base URI is not configured.");
                return null;
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                TreasurePlayLogger.NetworkError("Session token is required for Redeem");
                return null;
            }

            var path = "/giftcard/order/dynamic";
            var uri = new Uri(_settings.InventoryBaseUri, path);
            using var request = UnityWebRequest.PostWwwForm(uri, "");
            
            var redeemRequest = new TreasurePlayRedeemRequest(message ?? "");
            var requestBody = JsonUtility.ToJson(redeemRequest);
            
            TreasurePlayLogger.NetworkLog($"POST {uri}");
            TreasurePlayLogger.NetworkLog($"Request Body: {requestBody}");
            
            var bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", sessionToken);

            try
            {
                var response = await request.SendWebRequestAsync(cancellationToken);
                var payload = response.downloadHandler.text;
                
                TreasurePlayLogger.NetworkLog($"Redeem response: {payload}");
                
                if (response.result == UnityWebRequest.Result.Success)
                {
                    var redeemResponse = JsonUtility.FromJson<TreasurePlayRedeemResponse>(payload);
                    return redeemResponse;
                }
                else
                {
                    TreasurePlayLogger.NetworkError($"Redeem request failed: {response.error}");
                    return null;
                }
            }
            catch (Exception exception)
            {
                TreasurePlayLogger.Exception(exception);
                return null;
            }
        }

    }
}

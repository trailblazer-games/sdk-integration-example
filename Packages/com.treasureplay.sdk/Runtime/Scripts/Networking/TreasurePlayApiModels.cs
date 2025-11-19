using System;
using TreasurePlay.SDK.Core;

namespace TreasurePlay.SDK.Networking
{
    /// <summary>
    /// Request DTO for /init endpoint
    /// </summary>
    [Serializable]
    public sealed class InitRequestDto
    {
        public InitIdentitiesDto identities;
        public string api_key;

        public static InitRequestDto FromInitRequest(TreasurePlayInitRequest request)
        {
            return new InitRequestDto
            {
                identities = new InitIdentitiesDto
                {
                    cuid = request.Cuid,
                    fb_ai = request.FbAi,
                    email = request.Email,
                    display_name = request.DisplayName,
                    locale = request.Locale,
                    country_code = request.CountryCode,
                    game_id = request.GameId
                },
                api_key = request.ApiKey
            };
        }
    }

    [Serializable]
    public sealed class InitIdentitiesDto
    {
        public string cuid;
        public string fb_ai;
        public string email;
        public string display_name;
        public string locale;
        public string country_code;
        public string game_id;
    }
    
    /// <summary>
    /// Response DTO for /init endpoint
    /// </summary>
    [Serializable]
    public sealed class InitResponseDto
    {
        public InitDataDto data;
    }

    [Serializable]
    public sealed class InitDataDto
    {
        public string tp_uid;
        public string session_token;
    }
    
    /// <summary>
    /// TreasurePlayInitResponse
    /// </summary>
    public sealed class TreasurePlayInitResponse
    {
        public string SessionToken { get; }
        public string WebViewUrl { get; }
        public string TpUid { get; }

        public TreasurePlayInitResponse(InitResponseDto dto, string webViewUrl = null)
        {
            SessionToken = dto?.data?.session_token;
            WebViewUrl = webViewUrl;
            TpUid = dto?.data?.tp_uid;
        }

        public bool IsValid => !string.IsNullOrEmpty(SessionToken);
    }
}

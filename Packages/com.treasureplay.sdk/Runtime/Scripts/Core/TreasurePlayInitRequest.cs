using System;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Builder for creating init requests with flexible parameters
    /// </summary>
    public sealed class TreasurePlayInitRequest
    {
        public string Cuid { get; private set; }
        public string FbAi { get; private set; }
        public string Email { get; private set; }
        public string DisplayName { get; private set; }
        public string Locale { get; private set; }
        public string CountryCode { get; private set; }
        public string GameId { get; private set; }
        public string ApiKey { get; private set; }

        private TreasurePlayInitRequest() { }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private readonly TreasurePlayInitRequest _request = new TreasurePlayInitRequest();

            public Builder WithCuid(string cuid)
            {
                _request.Cuid = cuid;
                return this;
            }

            public Builder WithFbAi(string fbAi)
            {
                _request.FbAi = fbAi;
                return this;
            }

            public Builder WithEmail(string email)
            {
                _request.Email = email;
                return this;
            }

            public Builder WithDisplayName(string displayName)
            {
                _request.DisplayName = displayName;
                return this;
            }

            public Builder WithLocale(string locale)
            {
                _request.Locale = locale;
                return this;
            }

            public Builder WithCountryCode(string countryCode)
            {
                _request.CountryCode = countryCode;
                return this;
            }

            public Builder WithGameId(string gameId)
            {
                _request.GameId = gameId;
                return this;
            }

            public Builder WithApiKey(string apiKey)
            {
                _request.ApiKey = apiKey;
                return this;
            }

            public TreasurePlayInitRequest Build()
            {
                if (string.IsNullOrWhiteSpace(_request.Cuid) && string.IsNullOrWhiteSpace(_request.FbAi))
                {
                    throw new InvalidOperationException("Either Cuid or FbAi is required for TreasurePlayInitRequest.");
                }

                return _request;
            }
        }
    }
}

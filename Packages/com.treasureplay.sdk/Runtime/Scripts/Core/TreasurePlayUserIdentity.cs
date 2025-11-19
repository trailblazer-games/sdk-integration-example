using System;

namespace TreasurePlay.SDK.Core
{
    public readonly struct TreasurePlayUserIdentity
    {
        public string Email { get; }
        public string DisplayName { get; }
        public string Locale { get; }
        public string CountryCode { get; }
        public string Cuid { get; }
        public string FbAi { get; }

        private TreasurePlayUserIdentity(Builder builder)
        {
            Email = builder.Email;
            DisplayName = builder.DisplayName;
            Locale = builder.Locale;
            CountryCode = builder.CountryCode;
            Cuid = builder.Cuid;
            FbAi = builder.FbAi;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Cuid) && !string.IsNullOrWhiteSpace(FbAi);

        public override string ToString()
        {
            return $"Email={Email}, DisplayName={DisplayName}, Locale={Locale}, Country={CountryCode}, Cuid={Cuid}, FbAi={FbAi}";
        }

        public sealed class Builder
        {
            internal string Email { get; private set; }
            internal string DisplayName { get; private set; }
            internal string Locale { get; private set; }
            internal string CountryCode { get; private set; }
            internal string Cuid { get; private set; }
            internal string FbAi { get; private set; }

            public Builder WithEmail(string email)
            {
                Email = email;
                return this;
            }

            public Builder WithDisplayName(string displayName)
            {
                DisplayName = displayName;
                return this;
            }

            public Builder WithLocale(string locale)
            {
                Locale = locale;
                return this;
            }

            public Builder WithCountryCode(string countryCode)
            {
                CountryCode = countryCode;
                return this;
            }

            public Builder WithCuid(string cuid)
            {
                Cuid = cuid;
                return this;
            }

            public Builder WithFbAi(string fbAi)
            {
                FbAi = fbAi;
                return this;
            }

            public TreasurePlayUserIdentity Build()
            {
                if (string.IsNullOrWhiteSpace(Cuid))
                {
                    throw new InvalidOperationException("CUID is required for TreasurePlayUserIdentity.");
                }
                
                if (string.IsNullOrWhiteSpace(FbAi))
                {
                    throw new InvalidOperationException("Advertising ID (GAID/IDFA) is required for TreasurePlayUserIdentity.");
                }

                return new TreasurePlayUserIdentity(this);
            }
        }
    }
}

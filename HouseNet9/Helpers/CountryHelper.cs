namespace HouseNet9.Helpers
{
    public static class CountryHelper
    {
        private static readonly Dictionary<string, string> CountryCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Polska
            { "Polska", "PL" }, { "Poland", "PL" },

            // Ukraina
            { "Ukraina", "UA" }, { "Ukraine", "UA" },

            // Rumunia
            { "Rumunia", "RO" }, { "Romania", "RO" },

            // Węgry
            { "Węgry", "HU" }, { "Hungary", "HU" }, { "Magyarország", "HU" },

            // Słowenia
            { "Słowenia", "SI" }, { "Slovenia", "SI" },

            // Austria
            { "Austria", "AT" }, { "Österreich", "AT" },

            // Czechy
            { "Czechy", "CZ" }, { "Czech Republic", "CZ" }, { "Česká republika", "CZ" },

            // Chorwacja
            { "Chorwacja", "HR" }, { "Croatia", "HR" }, { "Hrvatska", "HR" },

            // Włochy
            { "Włochy", "IT" }, { "Italy", "IT" }, { "Italia", "IT" },

            // Słowacja
            { "Słowacja", "SK" }, { "Slovakia", "SK" }, { "Slovenská republika", "SK" },

            // Holandia
            { "Holandia", "NL" }, { "Netherlands", "NL" }, { "Nederland", "NL" },

            // Francja
            { "Francja", "FR" }, { "France", "FR" },

            // USA
            { "USA", "US" }, { "United States", "US" }, { "United States of America", "US" },

            // Bośnia i Hercegowina
            { "Bośnia i Hercegowina", "BA" }, { "Bosnia and Herzegovina", "BA" }, { "Bosna i Hercegovina", "BA" },

            // Szwajcaria
            { "Szwajcaria", "CH" }, { "Switzerland", "CH" }, { "Schweiz", "CH" },

            // Belgia
            { "Belgia", "BE" }, { "Belgium", "BE" }, { "België", "BE" },

            // Szwecja
            { "Szwecja", "SE" }, { "Sweden", "SE" }, { "Sverige", "SE" },

            // Hiszpania
            { "Hiszpania", "ES" }, { "Spain", "ES" }, { "España", "ES" },

            // Dania
            { "Dania", "DK" }, { "Denmark", "DK" }, { "Danmark", "DK" },

            // Norwegia
            { "Norwegia", "NO" }, { "Norway", "NO" }, { "Norge", "NO" }
        };

        public static string GetCountryCode(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return "UN"; // unknown

            if (CountryCodes.TryGetValue(country.Trim(), out var code))
                return code;

            return "UN"; // default unknown code
        }
    }
}

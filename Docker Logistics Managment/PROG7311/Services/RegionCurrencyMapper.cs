namespace PROG7311.Services;

/// <summary>
/// Maps client region labels (as stored on <see cref="Models.Client.Region"/>) to ISO 4217 currency codes.
/// Matching is case-insensitive; unknown regions default to ZAR (South African Rand).
/// </summary>
public static class RegionCurrencyMapper
{
    public static string GetCurrencyCodeForRegion(string? region)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            return "ZAR";
        }

        var key = region.Trim();

        return key.ToUpperInvariant() switch
        {
            "USA" or "US" or "UNITED STATES" => "USD",
            "CANADA" or "CA" => "CAD",
            "UK" or "GB" or "UNITED KINGDOM" or "GREAT BRITAIN" => "GBP",
            "EU" or "EUROPEAN UNION" or "EURO ZONE" or "EUROZONE" => "EUR",
            "JAPAN" or "JP" => "JPY",
            "CHINA" or "CN" or "PRC" or "PEOPLE'S REPUBLIC OF CHINA" => "CNY",
            "AUSTRALIA" or "AUSTRIALIA" or "AU" => "AUD",
            "SOUTH AFRICA" or "RSA" or "ZA" => "ZAR",
            _ => MatchPartial(key)
        };
    }

    private static string MatchPartial(string trimmedOriginal)
    {
        var upper = trimmedOriginal.ToUpperInvariant();
        if (upper.Contains("USA", StringComparison.Ordinal) || upper.Contains("UNITED STATES", StringComparison.Ordinal))
        {
            return "USD";
        }

        if (upper.Contains("UK", StringComparison.Ordinal) || upper.Contains("BRITAIN", StringComparison.Ordinal))
        {
            return "GBP";
        }

        if (upper.Contains("CANADA", StringComparison.Ordinal))
        {
            return "CAD";
        }

        if (upper.Contains("EUROPEAN UNION", StringComparison.Ordinal) || upper.Contains("EURO ZONE", StringComparison.Ordinal) || upper.Contains("EUROZONE", StringComparison.Ordinal) || upper == "EU")
        {
            return "EUR";
        }

        if (upper.Contains("JAPAN", StringComparison.Ordinal))
        {
            return "JPY";
        }

        if (upper.Contains("CHINA", StringComparison.Ordinal) || upper.Contains("PRC", StringComparison.Ordinal))
        {
            return "CNY";
        }

        if (upper.Contains("AUSTRALIA", StringComparison.Ordinal) || upper.Contains("AUSTRIALIA", StringComparison.Ordinal))
        {
            return "AUD";
        }

        if (upper.Contains("SOUTH AFRICA", StringComparison.Ordinal) || upper == "RSA" || upper == "ZA")
        {
            return "ZAR";
        }

        return "ZAR";
    }
}

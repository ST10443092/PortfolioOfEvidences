using System.Globalization;

namespace PROG7311.Services;

public static class CurrencyDisplay
{
    public static string Symbol(string isoCode)
    {
        return isoCode.Trim().ToUpperInvariant() switch
        {
            "ZAR" => "R",
            "USD" => "$",
            "GBP" => "£",
            "JPY" => "¥",
            _ => isoCode
        };
    }

    public static string FormatMoney(decimal amount, string isoCode)
    {
        var culture = CultureInfo.InvariantCulture;
        var symbol = Symbol(isoCode);
        var decimals = isoCode.Trim().ToUpperInvariant() == "JPY" ? 0 : 2;
        var fmt = decimals == 0 ? "N0" : "N2";
        return $"{symbol}{amount.ToString(fmt, culture)} {isoCode.Trim().ToUpperInvariant()}";
    }

    public static string FormatZar(decimal amountInZar)
    {
        return $"R {amountInZar.ToString("N2", CultureInfo.InvariantCulture)}";
    }
}

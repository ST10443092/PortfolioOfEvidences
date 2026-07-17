namespace PROG7311.Services;

public sealed record CurrencyConversionResult(
    decimal AmountInZar,
    decimal ExchangeRateZarPerUnit,
    DateOnly RateDate,
    string SourceCurrency);

public interface ICurrencyConversionService
{
    /// <summary>
    /// Converts an amount from <paramref name="fromCurrencyCode"/> (ISO 4217) to ZAR using a live rate.
    /// When <paramref name="fromCurrencyCode"/> is ZAR, returns the same amount with rate 1.
    /// </summary>
    Task<CurrencyConversionResult> ConvertToZarAsync(decimal amount, string fromCurrencyCode, CancellationToken cancellationToken = default);
}

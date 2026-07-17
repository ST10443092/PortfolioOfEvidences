using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PROG7311.Services;

public sealed class CurrencyConversionService : ICurrencyConversionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyConversionService> _logger;
    private readonly IConfiguration _configuration;

    public CurrencyConversionService(
        HttpClient httpClient,
        ILogger<CurrencyConversionService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CurrencyConversionResult> ConvertToZarAsync(
        decimal amount,
        string fromCurrencyCode,
        CancellationToken cancellationToken = default)
    {
        var code = fromCurrencyCode.Trim().ToUpperInvariant();

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(fromCurrencyCode));
        }

        if (code == "ZAR")
        {
            return new CurrencyConversionResult(
                amount,
                1m,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "ZAR");
        }

        var apiKey = _configuration["ExchangeRateApi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("ExchangeRate API key is missing.");
        }

        var url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{Uri.EscapeDataString(code)}";

        ExchangeRateApiLatestResponse? payload;
        try
        {
            payload = await _httpClient.GetFromJsonAsync<ExchangeRateApiLatestResponse>(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exchange rate request failed for {Currency} → ZAR.", code);
            throw new InvalidOperationException("Could not retrieve an exchange rate. Please try again.", ex);
        }

        if (payload == null)
        {
            throw new InvalidOperationException("No response was returned by the exchange rate service.");
        }

        if (!string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The exchange rate service returned an unsuccessful response.");
        }

        if (payload.ConversionRates == null || !payload.ConversionRates.TryGetValue("ZAR", out var zarPerUnit))
        {
            throw new InvalidOperationException($"No ZAR rate returned for currency '{code}'.");
        }

        var zar = decimal.Round(amount * zarPerUnit, 2, MidpointRounding.AwayFromZero);

        var rateDate = DateOnly.FromDateTime(
            DateTimeOffset.FromUnixTimeSeconds(payload.TimeLastUpdateUnix).UtcDateTime);

        return new CurrencyConversionResult(
            zar,
            zarPerUnit,
            rateDate,
            code);
    }

    private sealed class ExchangeRateApiLatestResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = "";

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = "";

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal>? ConversionRates { get; set; }
    }
}
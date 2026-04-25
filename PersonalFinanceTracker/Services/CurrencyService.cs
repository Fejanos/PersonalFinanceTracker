using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

/// <summary>
/// Holds the currently selected display currency and exchange rates.
/// All Transaction.Amount values are stored in the BaseCurrency (HUF);
/// this service converts them on display and converts user input back.
/// </summary>
public partial class CurrencyService : ObservableObject
{
    public const string BaseCurrency = "HUF";

    private readonly HttpClient _http = new();
    private Dictionary<string, decimal> _rates = new() { [BaseCurrency] = 1m };

    [ObservableProperty]
    private CurrencyInfo _currentCurrency;

    public IReadOnlyList<CurrencyInfo> AvailableCurrencies { get; } =
    [
        new("HUF", "Ft", "Hungarian Forint"),
        new("EUR", "€",  "Euro"),
        new("USD", "$",  "US Dollar"),
        new("GBP", "£",  "British Pound"),
    ];

    public CurrencyService()
    {
        _currentCurrency = AvailableCurrencies[0];
    }

    public async Task InitializeAsync()
    {
        try
        {
            var codes = string.Join(",",
                AvailableCurrencies.Where(c => c.Code != BaseCurrency).Select(c => c.Code));
            var url = $"https://api.frankfurter.app/latest?from={BaseCurrency}&to={codes}";

            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var rates = doc.RootElement.GetProperty("rates");

            var dict = new Dictionary<string, decimal> { [BaseCurrency] = 1m };
            foreach (var prop in rates.EnumerateObject())
                dict[prop.Name] = prop.Value.GetDecimal();
            _rates = dict;
            OnPropertyChanged(nameof(CurrentCurrency)); // trigger UI refresh
        }
        catch
        {
            // Sensible fallback rates if the API is unreachable.
            _rates = new Dictionary<string, decimal>
            {
                [BaseCurrency] = 1m,
                ["EUR"] = 0.0025m,
                ["USD"] = 0.0027m,
                ["GBP"] = 0.0021m
            };
        }
    }

    public decimal FromBase(decimal amountInBase)
    {
        var rate = _rates.GetValueOrDefault(CurrentCurrency.Code, 1m);
        return amountInBase * rate;
    }

    public decimal ToBase(decimal amountInDisplayCurrency)
    {
        var rate = _rates.GetValueOrDefault(CurrentCurrency.Code, 1m);
        return rate == 0 ? amountInDisplayCurrency : amountInDisplayCurrency / rate;
    }

    public string Format(decimal amountInBase)
    {
        var converted = FromBase(amountInBase);
        var decimals = CurrentCurrency.Code == "HUF" ? 0 : 2;
        return $"{converted.ToString($"N{decimals}")} {CurrentCurrency.Symbol}";
    }
}

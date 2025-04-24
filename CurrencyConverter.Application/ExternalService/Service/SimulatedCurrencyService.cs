using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Models.Responses;

namespace CurrencyConverter.Application.ExternalService.Service;

public class SimulatedCurrencyService : IExternalCurrencyService
{
    private ILoggerManager Logger1 { get; }
    private int _callCount;

    public SimulatedCurrencyService(ILoggerManager logger)
    {
        Logger1 = logger;
    }
    
    public async Task<RealTimeResponse> GetRealTimeRatesAsync(string baseCurrency)
    {
        _callCount++;
        if (_callCount % 5 == 0)
        {
            Logger1.LogWarn("Simulated rate limit exceeded for real-time rates.");
            throw new HttpRequestException("Rate limit exceeded (simulated).");
        }
        var response = new RealTimeResponse
        {
            BaseCurrency = baseCurrency,
            Date = DateTime.UtcNow.Date,
            Rates = new Dictionary<string, decimal>
            {
                {"GBP", 0.80m},
                {"EUR", 0.92m},
                {"JPY", 155.00m}
            }
        };
        Logger1.LogInfo($"Real-time rates fetched for {baseCurrency}");
        
        return await Task.FromResult(response);
    }

    public async Task<HistoricalResponse> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime start, DateTime end)
    {
        _callCount++;
        if (_callCount % 5 == 0)
        {
            Logger1.LogWarn("Simulated rate limit exceeded for historical rates.");
            throw new HttpRequestException("Rate limit exceeded (simulated).");
        }
        
        var rates = new Dictionary<string, decimal>();
        DateTime currentDate = start;
        
        while (currentDate <= end)
        {
            rates[currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)] = 0.79m + (decimal)(new Random().NextDouble() * 0.05);
            currentDate = currentDate.AddDays(1);
        }

        var response = new HistoricalResponse
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Rates = rates
        };
        
        Logger1.LogInfo($"Historical rates fetched from {start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} to {end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} for {baseCurrency} to {targetCurrency}");
        return await Task.FromResult(response);
    }
}

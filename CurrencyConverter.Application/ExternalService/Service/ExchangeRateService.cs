using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Models.Responses;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CurrencyConverter.Application.ExternalService.Service;

public class ExchangeRateService : IExternalCurrencyService
{
    private readonly SimulatedCurrencyService _simulatedService;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ExchangeRateService(SimulatedCurrencyService simulatedService, ILoggerManager logger)
    {
        _simulatedService = simulatedService;
        ILoggerManager logger1 = logger;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger1.LogWarn($"Retry {retryCount} after {timeSpan} due to {exception.Message}");
                });
    }

    public async Task<RealTimeResponse> GetRealTimeRatesAsync(string baseCurrency)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _simulatedService.GetRealTimeRatesAsync(baseCurrency));
    }

    public async Task<HistoricalResponse> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime start, DateTime end)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _simulatedService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, start, end));
    }
}

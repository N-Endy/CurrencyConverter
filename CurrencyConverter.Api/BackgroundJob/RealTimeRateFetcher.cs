using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Api.BackgroundJob;

public class RealTimeRateFetcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerManager _logger;
    private readonly IExternalCurrencyService _externalService;
    private readonly TimeSpan _fetchInterval = TimeSpan.FromHours(1);

    private const string BaseCurrency = "USD";

    public RealTimeRateFetcher(
        IServiceProvider serviceProvider,
        IExternalCurrencyService externalService,
        ILoggerManager logger)
    {
        _serviceProvider = serviceProvider;
        _externalService = externalService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInfo("Fetching real-time exchange rates...");

                using IServiceScope scope = _serviceProvider.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                RealTimeResponse response = await _externalService.GetRealTimeRatesAsync(BaseCurrency);

                DateTime date = response.Date;

                foreach (KeyValuePair<string, decimal> rate in response.Rates)
                {
                    dbContext.ExchangeRates.Add(new ExchangeRate
                    {
                        BaseCurrency = BaseCurrency,
                        TargetCurrency = rate.Key,
                        Rate = rate.Value,
                        CreatedAt = date
                    });
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInfo("Exchange rates fetched and saved. Next run in 6 hours.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching real-time rates: {ex.Message}");
            }
            await Task.Delay(_fetchInterval, stoppingToken);
            
        }
    }
}


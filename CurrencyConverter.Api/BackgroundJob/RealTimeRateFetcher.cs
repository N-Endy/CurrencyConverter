using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Api.BackgroundJob;

public class RealTimeRateFetcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggerManager _logger;
    //private readonly IExternalCurrencyService _externalService;
    private readonly TimeSpan _fetchInterval = TimeSpan.FromHours(1);

    private const string BaseCurrency = "USD";

    public RealTimeRateFetcher(
        IServiceScopeFactory scopeFactory,
        //IExternalCurrencyService externalService,
        ILoggerManager logger)
    {
        _scopeFactory = scopeFactory;
        //_externalService = externalService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInfo("Fetching real-time exchange rates...");

                using IServiceScope scope = _scopeFactory.CreateScope();
                IExternalCurrencyService externalService = scope.ServiceProvider.GetRequiredService<IExternalCurrencyService>();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                RealTimeResponse response;
                try
                {
                    response = await externalService.GetRealTimeRatesAsync(BaseCurrency);
                }
                catch (HttpRequestException ex)
                {
                    throw new ExternalServiceException($"Failed to fetch real-time rates for {BaseCurrency}: {ex.Message}");
                }
                // DateTime date = response.Date;

                foreach (KeyValuePair<string, decimal> rate in response.Rates)
                {
                    DateTime createdAt = response.Date;
                    
                    // Check if the record already exists
                    bool exists = await dbContext.ExchangeRates
                        .AnyAsync(e => e.BaseCurrency == response.BaseCurrency &&
                                       e.TargetCurrency == rate.Key &&
                                       e.CreatedAt == createdAt &&
                                       !e.IsHistorical, stoppingToken);


                    if (exists)
                    {
                        dbContext.ExchangeRates.Add(new ExchangeRate
                        {
                            BaseCurrency = BaseCurrency,
                            TargetCurrency = rate.Key,
                            Rate = rate.Value,
                            CreatedAt = createdAt,
                            IsHistorical = false
                        });
                    }
                }

                try
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInfo($"Real-time rates stored for {BaseCurrency}");
                }
                catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                {
                    throw new DatabaseException("Failed to save real-time rates due to a unique constraint violation.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching real-time rates: {ex.Message}");
            }
            await Task.Delay(_fetchInterval, stoppingToken);
        }
    }
}


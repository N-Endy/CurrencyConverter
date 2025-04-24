using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DateTime = System.DateTime;

namespace CurrencyConverter.Api.BackgroundJob;


public class HistoricalRateFetcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggerManager _logger;
    //private readonly IExternalCurrencyService _externalService;
    private readonly TimeSpan _fetchInterval = TimeSpan.FromDays(1); // Fetch daily


    private readonly (string baseCurrency, string targetCurrency)[] _trackedPairs =
    [
        ("USD", "GBP"),
        ("USD", "EUR"),
        ("GBP", "EUR")
    ];

    public HistoricalRateFetcher(
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
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    IExternalCurrencyService externalService = scope.ServiceProvider.GetRequiredService<IExternalCurrencyService>();
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    DateTime endDate = DateTime.UtcNow.Date;
                    DateTime startDate = endDate.AddYears(-1);

                    _logger.LogInfo("Fetching historical exchange rates...");

                    foreach (var (baseCurrency, targetCurrency) in _trackedPairs)
                    {
                        HistoricalResponse response;
                        try
                        {
                            response = await externalService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate);
                        }
                        catch (HttpRequestException ex)
                        {
                            throw new ExternalServiceException($"Failed to fetch historical rates for {baseCurrency}/{targetCurrency}: {ex.Message}");
                        }
                        
                        foreach (var (dateString, rateValue) in response.Rates)
                        {
                            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                // Check if the record already exists
                                bool exists = await dbContext.ExchangeRates
                                    .AnyAsync(e => e.BaseCurrency == baseCurrency &&
                                                   e.TargetCurrency == targetCurrency &&
                                                   e.CreatedAt == parsedDate &&
                                                   e.IsHistorical, stoppingToken);

                                if (!exists)
                                {
                                    dbContext.ExchangeRates.Add(new ExchangeRate
                                    {
                                        BaseCurrency = baseCurrency,
                                        TargetCurrency = targetCurrency,
                                        Rate = rateValue,
                                        CreatedAt = parsedDate,
                                        IsHistorical = true
                                    });
                                }
                            }
                        }
                    }

                    try
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInfo("Historical exchange rates saved.");
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                    {
                        throw new DatabaseException("Failed to save exchange rates due to a unique constraint violation.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching historical rates: {ex.Message}");
                }

                await Task.Delay(_fetchInterval, stoppingToken);
            }
        }
}


using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using DateTime = System.DateTime;

namespace CurrencyConverter.Api.BackgroundJob;


public class HistoricalRateFetcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerManager _logger;
    private readonly IExternalCurrencyService _externalService;

    private readonly (string baseCurrency, string targetCurrency)[] _trackedPairs =
    [
        ("USD", "GBP"),
        ("USD", "EUR"),
        ("GBP", "EUR")
    ];

    public HistoricalRateFetcher(
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
        using IServiceScope scope = _serviceProvider.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DateTime endDate = DateTime.UtcNow.Date;
        DateTime startDate = endDate.AddDays(-5);

        _logger.LogInfo("Fetching historical exchange rates...");

        foreach ((string baseCurrency, string targetCurrency) in _trackedPairs)
        {
            HistoricalResponse response = await _externalService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate);
            
            foreach (var (dateString, rateValue) in response.Rates)
            {
                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
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

            await dbContext.SaveChangesAsync(stoppingToken);
        }

        _logger.LogInfo("Historical exchange rates saved.");
    }
}


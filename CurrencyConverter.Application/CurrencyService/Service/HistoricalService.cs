using System.Globalization;
using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using CurrencyConverter.Infrastructure.Repository.Interface;

namespace CurrencyConverter.Application.CurrencyService.Service;

public class HistoricalService : IHistoricalService
{
    private readonly IExternalCurrencyService _exchangeRateService;
    public ICurrencyRepository CurrencyRepository { get; }
    private readonly ApplicationDbContext _dbContext;
    public ILoggerManager Logger1 { get; }
    
    public HistoricalService(IExternalCurrencyService exchangeRateService, ICurrencyRepository currencyRepository, ApplicationDbContext dbContext, ILoggerManager logger)
    {
        _exchangeRateService = exchangeRateService;
        CurrencyRepository = currencyRepository;
        _dbContext = dbContext;
        Logger1 = logger;
    }
    
    public async Task GetHistoricalRatesAsync()
    {
        (string, string)[] majorPairs = [("USD", "GBP"), ("USD", "EUR"), ("GBP", "EUR")];
        DateTime endDate = DateTime.UtcNow;
        DateTime startDate = endDate.AddYears(-1);

        foreach (var (baseCurrency, targetCurrency) in majorPairs)
        {
            HistoricalResponse response = await _exchangeRateService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate);

            foreach (KeyValuePair<string, decimal> rate in response.Rates)
            {
                var historicalRate = new ExchangeRate
                {
                    BaseCurrency = response.BaseCurrency,
                    TargetCurrency = response.TargetCurrency,
                    CreatedAt = DateTime.ParseExact(rate.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Rate = rate.Value
                };
                _dbContext.ExchangeRates.Add(historicalRate);
            }
        }
        await _dbContext.SaveChangesAsync();
        Logger1.LogInfo("Historical rates fetched and stored successfully.");
    }
}

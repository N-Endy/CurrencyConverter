using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Requests;
using CurrencyConverter.Infrastructure.Database;
using CurrencyConverter.Infrastructure.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Infrastructure.Repository.Service;

public class CurrencyRepository : ICurrencyRepository
{
    private ApplicationDbContext DbContext { get; }

    public CurrencyRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }
    
    public async Task<ExchangeRate> GetLatestExchangeRateAsync(CurrencyConversionRequest request)
    {
        ExchangeRate? rate = await DbContext.ExchangeRates
            .Where(r => r.BaseCurrency == request.FromCurrency && r.TargetCurrency == request.ToCurrency)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return rate;
    }

    public async Task<ExchangeRate> GetHistoricalExchangeRateAsync(CurrencyConversionRequest request)
    {
        ExchangeRate? rate = await DbContext.ExchangeRates
            .Where(r => 
                r.BaseCurrency == request.FromCurrency && r.TargetCurrency == request.ToCurrency && r.CreatedAt.Date == request.Date)
            .FirstOrDefaultAsync();

        return rate;
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllHistoricalExchangeRatesAsync(CurrencyConversionRequest request, DateTime from, DateTime to)
    {
        List<ExchangeRate> rates = await DbContext.ExchangeRates
            .Where(r => 
                r.BaseCurrency == request.FromCurrency && r.TargetCurrency == request.ToCurrency
                &&
                r.CreatedAt >= from.Date &&
                r.CreatedAt.Date <= to.Date)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return rates;
    }
}

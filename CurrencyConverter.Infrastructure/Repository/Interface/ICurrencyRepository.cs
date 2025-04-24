using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Requests;

namespace CurrencyConverter.Infrastructure.Repository.Interface;

public interface ICurrencyRepository
{
    Task<ExchangeRate> GetLatestExchangeRateAsync(CurrencyConversionRequest request);
    Task<ExchangeRate> GetHistoricalExchangeRateAsync(CurrencyConversionRequest request);
    // Task<ExchangeRate> GetExchangeRateByIdAsync(int id);
    // Task<IEnumerable<ExchangeRate>> GetAllExchangeRatesAsync();
    // Task AddExchangeRateAsync(ExchangeRate exchangeRate);
    // Task UpdateExchangeRateAsync(ExchangeRate exchangeRate);
    // Task DeleteExchangeRateAsync(int id);
    // Task<bool> ExchangeRateExistsAsync(int id);
    // Task<bool> ExchangeRateExistsAsync(string baseCurrency, string targetCurrency);
    // Task<bool> ExchangeRateExistsAsync(string baseCurrency, string targetCurrency, DateTime date);
    Task<IEnumerable<ExchangeRate>> GetAllHistoricalExchangeRatesAsync(CurrencyConversionRequest request, DateTime from, DateTime to);
}

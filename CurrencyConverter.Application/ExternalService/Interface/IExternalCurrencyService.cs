using CurrencyConverter.Domain.Models.Responses;

namespace CurrencyConverter.Application.ExternalService.Interface;

public interface IExternalCurrencyService
{
    Task<RealTimeResponse> GetRealTimeRatesAsync(string baseCurrency);
    Task<HistoricalResponse> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime start, DateTime end);
}

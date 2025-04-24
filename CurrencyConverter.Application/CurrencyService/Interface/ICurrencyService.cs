using CurrencyConverter.Domain.Models.Requests;
using CurrencyConverter.Domain.Models.Responses;

namespace CurrencyConverter.Application.CurrencyService.Interface;

public interface ICurrencyService
{
    Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request);
    Task<CurrencyConversionResponse> HistoricalConvertAsync(CurrencyConversionRequest request);
    Task<List<CurrencyConversionResponse>> GetHistoricalRatesAsync(CurrencyConversionRequest request, DateTime startDate, DateTime endDate);
}

namespace CurrencyConverter.Application.Conversion;

public interface ICurrencyConversionService
{
    Task<decimal> ConvertCurrencyAsync(string from, string to, decimal amount);
    Task<decimal> HistoricalConvertAsync(string from, string to, decimal amount, DateTime date);
    Task<Dictionary<DateTime, decimal>> GetHistoricalRatesAsync(string baseCurrency, 
        string targetCurrency, DateTime startDate, DateTime endDate);
}

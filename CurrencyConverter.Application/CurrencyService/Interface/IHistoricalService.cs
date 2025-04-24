namespace CurrencyConverter.Application.CurrencyService.Interface;

public interface IHistoricalService
{
    Task GetHistoricalRatesAsync();
}

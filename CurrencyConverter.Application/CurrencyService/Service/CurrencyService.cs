using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Requests;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Repository.Interface;

namespace CurrencyConverter.Application.CurrencyService.Service;

public class CurrencyService : ICurrencyService
{
    private ICurrencyRepository CurrencyRepository { get; }

    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        CurrencyRepository = currencyRepository;
    }

    public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(CurrencyConversionRequest request)
    {
        ExchangeRate? rate = await CurrencyRepository.GetLatestExchangeRateAsync(request);
        if (rate == null)
        {
            throw new Exception("Exchange rate not found.");
        }

        decimal convertedAmount = request.Amount * rate.Rate;

        return new CurrencyConversionResponse
        {
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Amount = request.Amount,
            ConvertedAmount = convertedAmount,
            Rate = rate.Rate,
            ConversionDate = rate.CreatedAt
        };
    }

    public async Task<CurrencyConversionResponse> HistoricalConvertAsync(CurrencyConversionRequest request)
    {
        ExchangeRate? rate = await CurrencyRepository.GetHistoricalExchangeRateAsync(request);
        if (rate == null)
        {
            throw new Exception("Historical exchange rate not found.");
        }

        decimal convertedAmount = request.Amount * rate.Rate;

        return new CurrencyConversionResponse
        {
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Amount = request.Amount,
            ConvertedAmount = convertedAmount,
            Rate = rate.Rate,
            ConversionDate = rate.CreatedAt
        };
    }
    
    public async Task<List<CurrencyConversionResponse>> GetHistoricalRatesAsync(CurrencyConversionRequest request, DateTime startDate, DateTime endDate)
    {
        var rates = (List<ExchangeRate>) await CurrencyRepository.GetAllHistoricalExchangeRatesAsync(request, startDate, endDate);
        if (rates == null || !rates.Any())
        {
            throw new Exception("No historical exchange rates found.");
        }

        var responses = new List<CurrencyConversionResponse>();
        foreach (ExchangeRate rate in rates)
        {
            decimal convertedAmount = request.Amount * rate.Rate;
            responses.Add(new CurrencyConversionResponse
            {
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                Amount = request.Amount,
                ConvertedAmount = convertedAmount,
                Rate = rate.Rate,
                ConversionDate = rate.CreatedAt
            });
        }

        return responses;
    }
}

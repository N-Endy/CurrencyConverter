using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Exceptions;
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
        if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
        {
            throw new ValidationException("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException("Amount must be greater than zero.");
        }
        
        ExchangeRate? rate = await CurrencyRepository.GetLatestExchangeRateAsync(request);
        if (rate == null)
        {
            throw new NotFoundException("Exchange rate not found.");
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
        if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
        {
            throw new NotFoundException("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            throw new NotFoundException("Amount must be greater than zero.");
        }
        
        ExchangeRate? rate = await CurrencyRepository.GetHistoricalExchangeRateAsync(request);
        if (rate == null)
        {
            throw new NotFoundException("Historical exchange rate not found.");
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
        if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
        {
            throw new NotFoundException("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            throw new NotFoundException("Amount must be greater than zero.");
        }

        if (startDate >= endDate)
        {
            throw new NotFoundException("Start date must be earlier than end date.");
        }
        
        var rates = (List<ExchangeRate>) await CurrencyRepository.GetAllHistoricalExchangeRatesAsync(request, startDate, endDate);
        if (rates == null || !rates.Any())
        {
            throw new NotFoundException("No historical exchange rates found.");
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

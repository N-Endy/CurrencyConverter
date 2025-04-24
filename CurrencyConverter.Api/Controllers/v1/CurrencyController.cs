using Asp.Versioning;
using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Domain.Exceptions;
using CurrencyConverter.Domain.Models.Requests;
using CurrencyConverter.Domain.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CurrencyController : ControllerBase
{
    private ICurrencyService CurrencyService { get; }

    public CurrencyController(ICurrencyService currencyService)
    {
        CurrencyService = currencyService;
    }
    
    [HttpGet("convert")]
    public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
    {
        if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
        {
            throw new ValidationException("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            throw new ValidationException("Amount must be greater than zero.");
        }

        try
        {
            CurrencyConversionResponse result = await CurrencyService.ConvertCurrencyAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("historical-rates")]
    public async Task<IActionResult> GetHistoricalRates([FromBody] CurrencyConversionRequest request, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
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

        try
        {
            List<CurrencyConversionResponse> result = await CurrencyService.GetHistoricalRatesAsync(request, startDate, endDate);
            return Ok(result.Select(r => new { r.ConversionDate, r.Rate }));
            //return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpGet("convert/historical")]
    public async Task<IActionResult> HistoricalConvert([FromBody] CurrencyConversionRequest request)
    {
        if (string.IsNullOrEmpty(request.FromCurrency) || string.IsNullOrEmpty(request.ToCurrency))
        {
            throw new NotFoundException("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            throw new NotFoundException("Amount must be greater than zero.");
        }

        try
        {
            CurrencyConversionResponse result = await CurrencyService.HistoricalConvertAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

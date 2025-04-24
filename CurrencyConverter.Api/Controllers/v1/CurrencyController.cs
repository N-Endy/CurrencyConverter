using Asp.Versioning;
using CurrencyConverter.Application.CurrencyService.Interface;
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
            return BadRequest("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
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
            return BadRequest("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        if (startDate >= endDate)
        {
            return BadRequest("Start date must be earlier than end date.");
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
            return BadRequest("FromCurrency and ToCurrency are required.");
        }

        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
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

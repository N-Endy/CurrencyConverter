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
    
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
    {
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
    
    [HttpPost("historical-rates")]
    public async Task<IActionResult> GetHistoricalRates([FromBody] CurrencyConversionRequest request, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            List<CurrencyConversionResponse> result = await CurrencyService.GetHistoricalRatesAsync(request, startDate, endDate);
            return Ok(result.Select(r => new { r.ConversionDate, r.Rate }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("convert/historical")]
    public async Task<IActionResult> HistoricalConvert([FromBody] CurrencyConversionRequest request)
    {
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

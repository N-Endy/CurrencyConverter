using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class HistoricalRatesController : ControllerBase
{
    private readonly IHistoricalService _historicalService;
    private readonly ILoggerManager _logger;

    public HistoricalRatesController(IHistoricalService historicalService, ILoggerManager logger)
    {
        _historicalService = historicalService;
        _logger = logger;
    }

    [HttpPost("fetch")]
    public async Task<IActionResult> FetchHistoricalRates()
    {
        try
        {
            await _historicalService.GetHistoricalRatesAsync();
            return Ok("Historical rates stored.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error storing historical rates: {ex.Message}");
            return StatusCode(500, "Error storing historical rates.");
        }
    }
}

using System.Globalization;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.Models.Responses;
using CurrencyConverter.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class HistoricalRatesController : ControllerBase
{
    private readonly IExternalCurrencyService _exchangeRateService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILoggerManager _logger;

    public HistoricalRatesController(IExternalCurrencyService exchangeRateService, ApplicationDbContext dbContext, ILoggerManager logger)
    {
        _exchangeRateService = exchangeRateService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("fetch")]
    public async Task<IActionResult> FetchHistoricalRates()
    {
        try
        {
            (string, string)[] majorPairs = [("USD", "GBP"), ("USD", "EUR"), ("GBP", "EUR")];
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = endDate.AddYears(-1);

            foreach (var (baseCurrency, targetCurrency) in majorPairs)
            {
                HistoricalResponse response = await _exchangeRateService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate);

                foreach (KeyValuePair<string, decimal> rate in response.Rates)
                {
                    var historicalRate = new ExchangeRate
                    {
                        BaseCurrency = response.BaseCurrency,
                        TargetCurrency = response.TargetCurrency,
                        CreatedAt = DateTime.ParseExact(rate.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Rate = rate.Value
                    };
                    _dbContext.ExchangeRates.Add(historicalRate);
                }
            }
            await _dbContext.SaveChangesAsync();
            _logger.LogInfo("Historical rates stored successfully.");
            return Ok("Historical rates stored.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error storing historical rates: {ex.Message}");
            return StatusCode(500, "Error storing historical rates.");
        }
    }
}

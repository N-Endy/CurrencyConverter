namespace CurrencyConverter.Api.BackgroundJob;

public class RatesBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //await _rateService.FetchLatestRates();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

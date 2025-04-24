namespace CurrencyConverter.Domain.Models.Responses;

public class HistoricalResponse
{
    public string BaseCurrency { get; set; } = null!;
    public string TargetCurrency { get; set; } = null!;
    public Dictionary<string, decimal> Rates { get; set; } = [];
}

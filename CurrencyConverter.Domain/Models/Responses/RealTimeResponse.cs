namespace CurrencyConverter.Domain.Models.Responses;

public class RealTimeResponse
{
    public string BaseCurrency { get; set; } = "USD";
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = [];
}

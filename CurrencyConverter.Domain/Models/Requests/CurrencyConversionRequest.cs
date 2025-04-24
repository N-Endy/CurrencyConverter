namespace CurrencyConverter.Domain.Models.Requests;

public class CurrencyConversionRequest
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
}

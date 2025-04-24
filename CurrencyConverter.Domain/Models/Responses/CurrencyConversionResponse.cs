namespace CurrencyConverter.Domain.Models.Responses;

public class CurrencyConversionResponse
{
    public string FromCurrency { get; set; }
    public string ToCurrency { get; set; }
    public decimal Amount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal Rate { get; set; }
    public DateTime ConversionDate { get; set; }
}

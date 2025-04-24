using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyConverter.Domain.Entities;

public class ExchangeRate
{
    [Column("ExchangeRateId")]
    public int Id { get; set; }
    public string BaseCurrency { get; set; }
    public string TargetCurrency { get; set; }
    public decimal Rate { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsHistorical { get; set; }
}

using CurrencyConverter.Domain.Models.Requests;
using FluentValidation;

namespace CurrencyConverter.Application.Validators;


public class CurrencyConversionRequestValidator : AbstractValidator<CurrencyConversionRequest>
{
    public CurrencyConversionRequestValidator()
    {
        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("Base currency is required.")
            .Length(3);

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("Target currency is required.")
            .Length(3);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}


using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ProductManagementAPI.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = Convert.ToDecimal(min);
        _max = Convert.ToDecimal(max);
        ErrorMessage = $"Price must be between {_min.ToString("C", CultureInfo.CurrentCulture)} and {_max.ToString("C", CultureInfo.CurrentCulture)}.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;
        if (decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
        {
            return dec >= _min && dec <= _max;
        }
        return false;
    }
}
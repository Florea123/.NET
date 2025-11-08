using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ProductManagementAPI.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly Regex SkuRegex = new(@"^[A-Za-z0-9\-]{5,20}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ValidSKUAttribute()
    {
        ErrorMessage = "SKU must be 5-20 characters, alphanumeric and may include hyphens.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;
        var str = value.ToString()!.Replace(" ", string.Empty);
        return SkuRegex.IsMatch(str);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var attributes = context.Attributes;
        attributes["data-val"] = "true";
        attributes["data-val-validsku"] = ErrorMessage ?? "Invalid SKU.";
        attributes["data-val-validsku-pattern"] = "^[A-Za-z0-9\\-]{5,20}$";
    }
}
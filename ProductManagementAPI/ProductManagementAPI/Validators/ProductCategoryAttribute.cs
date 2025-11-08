using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly ProductCategory[] _allowed;

    public ProductCategoryAttribute(params ProductCategory[] allowed)
    {
        _allowed = allowed ?? Array.Empty<ProductCategory>();
        var names = _allowed.Select(a => a.ToString()).ToArray();
        ErrorMessage = "Category must be one of: " + string.Join(", ", names);
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;
        if (value is ProductCategory pc)
        {
            return _allowed.Length == 0 || _allowed.Contains(pc);
        }

        if (Enum.TryParse(typeof(ProductCategory), value.ToString(), out var parsed))
        {
            return _allowed.Length == 0 || _allowed.Contains((ProductCategory)parsed);
        }

        return false;
    }
}
    
using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using System;
using System.Linq;

namespace ProductManagementAPI.Mapping.Resolvers;

public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var brand = source.Brand;
        if (string.IsNullOrWhiteSpace(brand)) return "?";

        var parts = brand.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var first = char.ToUpperInvariant(parts.First()[0]);
            var last = char.ToUpperInvariant(parts.Last()[0]);
            return $"{first}{last}";
        }

        return char.ToUpperInvariant(parts[0][0]).ToString();
    }
}
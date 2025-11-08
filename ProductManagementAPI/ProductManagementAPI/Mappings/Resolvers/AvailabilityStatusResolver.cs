using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Mapping.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable) return "Out of Stock";
        if (source.IsAvailable && source.StockQuantity == 0) return "Unavailable";
        if (source.IsAvailable && source.StockQuantity == 1) return "Last Item";
        if (source.IsAvailable && source.StockQuantity <= 5) return "Limited Stock";
        return "In Stock";
    }
}
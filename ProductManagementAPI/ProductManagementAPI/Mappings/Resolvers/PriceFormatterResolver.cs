using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using System.Globalization;

namespace ProductManagementAPI.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2", CultureInfo.CurrentCulture);
    }
}
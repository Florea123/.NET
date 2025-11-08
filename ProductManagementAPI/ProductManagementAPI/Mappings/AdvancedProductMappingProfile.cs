
using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Mapping.Resolvers;
using System;

namespace ProductManagementAPI.Mapping;

public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
        CreateMap<CreateProductProfileRequest, Product>()
            .ConstructUsing(src => new Product(
                Guid.NewGuid(),
                src.Name,
                src.Brand,
                src.SKU,
                src.Category,
                src.Category == ProductCategory.Home ? src.Price * 0.9m : src.Price,
                src.ReleaseDate,
                src.Category == ProductCategory.Home ? null : src.ImageUrl,
                src.StockQuantity,
                src.StockQuantity > 0,
                DateTime.UtcNow
            ));

        CreateMap<Product, ProductProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.ProductAge, opt => opt.MapFrom<ProductAgeResolver>())
            .ForMember(dest => dest.BrandInitials, opt => opt.MapFrom<BrandInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
    }
}
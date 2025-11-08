
using AutoMapper;
using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, CreateProductRequest>();

        CreateMap<CreateProductRequest, Product>()
            .ConstructUsing(src => new Product(
                Guid.NewGuid(),
                src.Name,
                src.Brand,
                src.SKU,
                src.Category,
                src.Price,
                src.ReleaseDate,
                src.ImageUrl,
                src.StockQuantity,
                src.IsAvailable,
                src.CreatedAt
            ));
        
        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
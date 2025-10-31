using AutoMapper;
using ProductManagementAPI.Features.Products;

namespace ProductManagement.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
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
                DateTime.UtcNow
            ));
    }
    
}
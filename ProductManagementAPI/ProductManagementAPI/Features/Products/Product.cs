
namespace ProductManagementAPI.Features.Products;

public record Product(
    Guid Id,
    string Name,
    string Brand,
    string SKU,
    ProductCategory Category,
    decimal Price,
    DateTime ReleaseDate,
    string? ImageUrl,
    int StockQuantity = 0,
    bool? IsAvailableOverride = null,
    DateTime CreatedAt = default
)
{
    public bool IsAvailable => IsAvailableOverride ?? StockQuantity > 0;
}
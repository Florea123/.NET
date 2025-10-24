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
    int StockQuantity,
    bool IsAvailable,
    DateTime CreatedAt
    );
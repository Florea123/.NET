namespace ProductManagementAPI.Features.Products;

public record UpdateProductRequest(Guid id, string Name, string Brand, string SKU, ProductCategory Category, decimal Price, DateTime ReleaseDate, string? ImageUrl, int StockQuantity, bool IsAvailable);
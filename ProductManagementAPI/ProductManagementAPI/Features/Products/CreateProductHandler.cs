using ProductManagementAPI.Persistance;

namespace ProductManagementAPI.Features.Products;

public class CreateProductHandler(ProductManagementContext context,ILogger<CreateProductHandler> logger)
{
    public async Task<IResult> Handle(CreateProductRequest request)
    {
        var product = new Product
        (
            Guid.NewGuid(),
            request.Name,
            request.Brand,
            request.SKU,
            request.Category,
            request.Price,
            request.ReleaseDate,
            request.ImageUrl,
            request.StockQuantity,
            request.IsAvailable,
            request.CreatedAt
        );

        context.Products.Add(product);
        await context.SaveChangesAsync();

        logger.LogInformation("Product created with ID: {ProductId}", product.Id);

        return Results.Created($"/products/{product.Id}", product);
    }
}
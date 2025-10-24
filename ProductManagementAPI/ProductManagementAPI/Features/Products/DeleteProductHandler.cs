using ProductManagementAPI.Persistance;

namespace ProductManagementAPI.Features.Products;

public class DeleteProductHandler(ProductManagementContext context, ILogger<DeleteProductHandler> logger)
{
    public async Task<IResult> Handle(DeleteProductRequest request)
    {
        var product = await context.Products.FindAsync(request.Id);
        if (product == null)
        {
            logger.LogWarning("Product with ID: {ProductId} not found for deletion", request.Id);
            return Results.NotFound($"Product with ID: {request.Id} not found.");
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        logger.LogInformation("Product with ID: {ProductId} deleted successfully", request.Id);
        return Results.Ok($"Product with ID: {request.Id} deleted successfully.");
    }
}
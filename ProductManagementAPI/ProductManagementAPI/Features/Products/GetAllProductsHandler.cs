using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Persistance;

namespace ProductManagementAPI.Features.Products;

public class GetAllProductsHandler(ProductManagementContext context, ILogger<GetAllProductsHandler> logger)
{
    public async Task<IResult> Handle(GetAllProductsRequest request)
    {
        var products = await context.Products.ToListAsync();

        logger.LogInformation("Retrieved {ProductCount} products", products.Count);

        return Results.Ok(products);
    }
}
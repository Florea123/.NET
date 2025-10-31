using AutoMapper;
using FluentValidation;
using ProductManagementAPI.Persistance;

namespace ProductManagementAPI.Features.Products;

public class UpdateProductHandler(ProductManagementContext dbContext, IMapper mapper, IValidator<UpdateProductRequest> validator)
{
    public async Task<IResult> Handle(UpdateProductRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        var updatedProduct = mapper.Map<Product>(request);
        
        dbContext.Products.Update(updatedProduct);
        
        await dbContext.SaveChangesAsync();
        return Results.NoContent();
    }
}
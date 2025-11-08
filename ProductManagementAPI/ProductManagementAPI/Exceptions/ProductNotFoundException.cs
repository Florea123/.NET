namespace ProductManagementAPI.Exceptions;

public class ProductNotFoundException : BaseException
{
    protected ProductNotFoundException(Guid productId) 
        : base($"Product with Id {productId} was not found", 404, "PRODUCT_NOT_FOUND")
    {
        
    }
}
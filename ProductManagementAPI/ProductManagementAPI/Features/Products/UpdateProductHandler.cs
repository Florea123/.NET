// csharp
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Persistance;

public class UpdateProductHandler
{
    private readonly ProductManagementContext _context;
    private readonly ILogger<UpdateProductHandler> _logger;
    private readonly IValidator<UpdateProductRequest> _validator;
    private readonly IMapper _mapper;

    public UpdateProductHandler(ProductManagementContext context, ILogger<UpdateProductHandler> logger, IValidator<UpdateProductRequest> validator, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<IResult> Handle(UpdateProductRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
            _logger.LogWarning("Validation failed for UpdateProductRequest: {Details}", details);
            return Results.BadRequest(new { ErrorCode = "VALIDATION_ERROR", Message = "Validation failed", Details = details });
        }

        var existing = await _context.Products.FindAsync(request.Id);
        if (existing == null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found for update", request.Id);
            return Results.NotFound($"Product with ID: {request.Id} not found.");
        }
        
        _mapper.Map(request, existing);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product with ID: {ProductId} updated successfully", request.Id);
        return Results.Ok(existing);
    }
}
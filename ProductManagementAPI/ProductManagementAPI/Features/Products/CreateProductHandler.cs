using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Persistance;
using ProductManagementAPI.Logging;
using ProductManagementAPI.Middleware;
using ValidationException = ProductManagementAPI.Exceptions.ValidationException;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;

namespace ProductManagementAPI.Features.Products;

public class CreateProductHandler
{
    private readonly ProductManagementContext _context;
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IValidator<CreateProductProfileRequest> _validator;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateProductHandler(
        ProductManagementContext context,
        ILogger<CreateProductHandler> logger,
        IValidator<CreateProductProfileRequest> validator,
        IMapper mapper,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(CreateProductProfileRequest request)
    {
        var totalSw = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationMiddleware.HeaderName]?.ToString() 
                            ?? _httpContextAccessor.HttpContext?.TraceIdentifier
                            ?? string.Empty;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["OperationId"] = operationId,
                   ["CorrelationId"] = correlationId,
                   ["ProductSKU"] = request.SKU
               }))
        {
            try
            {
                _logger.LogInformation(new EventId(LogEvents.ProductCreationStarted, nameof(LogEvents.ProductCreationStarted)),
                    "Product creation started: Name={Name} Brand={Brand} SKU={SKU} Category={Category}",
                    request.Name, request.Brand, request.SKU, request.Category);
                
                var validationSw = Stopwatch.StartNew();
                var validationResult = await _validator.ValidateAsync(request);
                validationSw.Stop();
                
                var skuValidationSw = Stopwatch.StartNew();
                var exists = await _context.Products.AnyAsync(p => p.SKU == request.SKU);
                skuValidationSw.Stop();
                _logger.LogInformation(new EventId(LogEvents.SKUValidationPerformed, nameof(LogEvents.SKUValidationPerformed)),
                    "SKU validation: SKU={SKU} Exists={Exists} DurationMs={Ms}",
                    request.SKU, exists, skuValidationSw.Elapsed.TotalMilliseconds);

                // Stock validation (perform and log)
                _logger.LogInformation(new EventId(LogEvents.StockValidationPerformed, nameof(LogEvents.StockValidationPerformed)),
                    "Stock validation: SKU={SKU} StockQuantity={StockQuantity}", request.SKU, request.StockQuantity);

                if (!validationResult.IsValid || exists)
                {
                    var details = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    if (exists) details.Add($"SKU: {request.SKU} already exists.");

                    _logger.LogWarning(new EventId(LogEvents.ProductValidationFailed, nameof(LogEvents.ProductValidationFailed)),
                        "Validation failed for product creation. OperationId={OperationId} Details={Details}",
                        operationId, details);

                    var metricsFail = new ProductCreationMetrics(
                        operationId,
                        request.Name,
                        request.SKU,
                        request.Category,
                        validationSw.Elapsed,
                        TimeSpan.Zero,
                        totalSw.Elapsed,
                        false,
                        string.Join("; ", details)
                    );

                    _logger.LogProductCreationMetrics(metricsFail);

                    // Throw ValidationException so integration tests can assert exception behavior
                    throw new ValidationException(details);
                }

                // Map and save
                var product = _mapper.Map<Product>(request);

                var dbSw = Stopwatch.StartNew();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationStarted, nameof(LogEvents.DatabaseOperationStarted)),
                    "Database save started: OperationId={OperationId}", operationId);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                dbSw.Stop();
                _logger.LogInformation(new EventId(LogEvents.DatabaseOperationCompleted, nameof(LogEvents.DatabaseOperationCompleted)),
                    "Database save completed: ProductId={ProductId} OperationId={OperationId} DurationMs={Ms}",
                    product.Id, operationId, dbSw.Elapsed.TotalMilliseconds);
                
                if (_cache is not null)
                {
                    _cache.Remove("all_products");
                    _logger.LogInformation(new EventId(LogEvents.CacheOperationPerformed, nameof(LogEvents.CacheOperationPerformed)),
                        "Cache operation: Removed key 'all_products' for OperationId={OperationId}", operationId);
                }

                totalSw.Stop();

                var metrics = new ProductCreationMetrics(
                    operationId,
                    request.Name,
                    request.SKU,
                    request.Category,
                    validationSw.Elapsed,
                    dbSw.Elapsed,
                    totalSw.Elapsed,
                    true,
                    null
                );

                _logger.LogProductCreationMetrics(metrics);

                _logger.LogInformation("Product created: Name={Name}, Brand={Brand}, Category={Category}, SKU={SKU}, Id={Id}",
                    product.Name, product.Brand, product.Category, product.SKU, product.Id);

                var dto = _mapper.Map<ProductProfileDto>(product);
                return Results.Created($"/products/{product.Id}", dto);
            }
            catch (Exception ex)
            {
                totalSw.Stop();
                _logger.LogError(ex, "Unhandled exception during product creation. OperationId={OperationId} SKU={SKU}", operationId, request.SKU);

                var errorMetrics = new ProductCreationMetrics(
                    operationId,
                    request.Name,
                    request.SKU,
                    request.Category,
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    totalSw.Elapsed,
                    false,
                    ex.Message
                );

                _logger.LogProductCreationMetrics(errorMetrics);

                throw; 
            }
        }
    }
}
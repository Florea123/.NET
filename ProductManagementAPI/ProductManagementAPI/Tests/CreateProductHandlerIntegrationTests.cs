using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Mapping;
using ProductManagementAPI.Persistance;
using ProductManagementAPI.Validators;
using ProductManagementAPI.Exceptions;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Middleware;

namespace ProductManagementAPI.Tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly ProductManagementContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _handlerLoggerMock;
    private readonly Mock<ILogger<CreateProductProfileValidator>> _validatorLoggerMock;
    private readonly CreateProductProfileValidator _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerIntegrationTests()
    {
        // Unique in-memory DB per test instance
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ProductManagementContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        _context = new ProductManagementContext(options);

        // AutoMapper with both profiles
        var cfg = new MapperConfiguration(c =>
        {
            c.AddProfile(new ProductMappingProfile());
            c.AddProfile(new AdvancedProductMappingProfile());
        });
        _mapper = cfg.CreateMapper();

        // Memory cache
        _cache = new MemoryCache(new MemoryCacheOptions());

        // Loggers
        _handlerLoggerMock = new Mock<ILogger<CreateProductHandler>>();
        _validatorLoggerMock = new Mock<ILogger<CreateProductProfileValidator>>();

        // Validator that depends on context and logger
        _validator = new CreateProductProfileValidator(_context, _validatorLoggerMock.Object);

        // HttpContextAccessor with correlation id in Items
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationMiddleware.HeaderName] = "test-corr-id";
        _httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

        // Create handler
        _handler = new CreateProductHandler(_context, _handlerLoggerMock.Object, _validator, _mapper, _cache, _httpContextAccessor);
    }

    public void Dispose()
    {
        _context?.Dispose();
        (_cache as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        // Arrange
        var req = new CreateProductProfileRequest(
            Name: "Smart Phone X",
            Brand: "Acme Corp",
            SKU: "SKU-12345",
            Category: ProductCategory.Electronics,
            Price: 199.99m,
            ReleaseDate: DateTime.UtcNow.AddMonths(-2),
            ImageUrl: "https://example.com/image.jpg",
            StockQuantity: 10
        );

        // Act
        var _ = await _handler.Handle(req);

        // Assert - verify product persisted and mappings via AutoMapper
        var product = await _context.Products.SingleOrDefaultAsync(p => p.SKU == req.SKU);
        Assert.NotNull(product);

        var dto = _mapper.Map<ProductProfileDto>(product);

        Assert.Equal("Electronics & Technology", dto.CategoryDisplayName);
        Assert.Equal("AC", dto.BrandInitials); // Acme Corp -> A C
        Assert.Equal("New Release", dto.ProductAge); // CreatedAt set at mapping to now, so new release
        var currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
        Assert.StartsWith(currencySymbol, dto.FormattedPrice);
        Assert.Equal("In Stock", dto.AvailabilityStatus);

        // Verify ProductCreationStarted log called once (basic verification)
        _handlerLoggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        // Arrange - existing product with same SKU
        var existingProduct = new Product(
            Guid.NewGuid(),
            "Existing Product",
            "Existing Brand",
            "DUP-SKU-1",
            ProductCategory.Electronics,
            100m,
            DateTime.UtcNow.AddMonths(-1),
            "https://example.com/img.jpg",
            5,
            true,
            DateTime.UtcNow.AddDays(-1)
        );

        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        var req = new CreateProductProfileRequest(
            Name: "New Product",
            Brand: "Existing Brand",
            SKU: "DUP-SKU-1",
            Category: ProductCategory.Electronics,
            Price: 150m,
            ReleaseDate: DateTime.UtcNow.AddMonths(-1),
            ImageUrl: "https://example.com/new.jpg",
            StockQuantity: 3
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(req));
        Assert.NotNull(ex.Errors);
        Assert.True(ex.Errors.Any(e => e.Contains("already exists", StringComparison.OrdinalIgnoreCase)));

        // Verify ProductValidationFailed log called at least once
        _handlerLoggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        // Arrange
        var req = new CreateProductProfileRequest(
            Name: "Comfort Chair",
            Brand: "Home Goods",
            SKU: "HOME-001",
            Category: ProductCategory.Home,
            Price: 100m,
            ReleaseDate: DateTime.UtcNow.AddDays(-10),
            ImageUrl: "https://example.com/chair.jpg",
            StockQuantity: 2
        );

        // Act
        var _ = await _handler.Handle(req);

        // Assert - verify product persisted and mapping behavior
        var product = await _context.Products.SingleOrDefaultAsync(p => p.SKU == req.SKU);
        Assert.NotNull(product);

        var dto = _mapper.Map<ProductProfileDto>(product);

        Assert.Equal("Home & Garden", dto.CategoryDisplayName);
        Assert.Equal(90m, dto.Price); // 10% discount applied at creation mapping
        Assert.Null(dto.ImageUrl); // ImageUrl should be null for Home category due to mapping
    }
}
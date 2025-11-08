using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Mapping;
using ProductManagementAPI.Middleware;
using ProductManagementAPI.Persistance;
using ProductManagementAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc
    (
        "v1",
        new OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "API for managing products.",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com",
            }
        });
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ProductManagementContext>(options =>
    options.UseSqlite("Data Source=productmanagement.db"));

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Register both AutoMapper profiles explicitly
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductMappingProfile>();
    cfg.AddProfile<AdvancedProductMappingProfile>();
});

// Register resolvers (if needed via DI)
builder.Services.AddTransient<ProductManagementAPI.Mapping.Resolvers.CategoryDisplayResolver>();
builder.Services.AddTransient<ProductManagementAPI.Mapping.Resolvers.PriceFormatterResolver>();
builder.Services.AddTransient<ProductManagementAPI.Mapping.Resolvers.ProductAgeResolver>();
builder.Services.AddTransient<ProductManagementAPI.Mapping.Resolvers.BrandInitialsResolver>();
builder.Services.AddTransient<ProductManagementAPI.Mapping.Resolvers.AvailabilityStatusResolver>();

// Register handlers
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetAllProductsHandler>();
builder.Services.AddScoped<DeleteProductHandler>();
builder.Services.AddScoped<UpdateProductHandler>();

// Register CreateProductProfileValidator as scoped and all validators from its assembly
builder.Services.AddScoped<CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductManagementContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI
        (
            c=>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API V1");
                c.RoutePrefix = string.Empty;
                c.DisplayRequestDuration();
            }
        );

    app.MapOpenApi();
}

// Add global exception handling and correlation middleware
app.UseGlobalExceptionMiddleware();
app.UseCorrelationMiddleware();
app.UseCors("DevCors");

app.UseHttpsRedirection();

// Product endpoints with improved metadata for documentation
app.MapPost("/products", async (CreateProductProfileRequest req, CreateProductHandler handler) =>
    await handler.Handle(req))
    .WithName("CreateProduct")
    .WithTags("Products")
    .Produces<ProductProfileDto>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);

app.MapGet("/products", async (GetAllProductsHandler handler) =>
    await handler.Handle(new GetAllProductsRequest()))
    .WithTags("Products")
    .Produces<IEnumerable<ProductProfileDto>>(StatusCodes.Status200OK);

app.MapDelete("/products/{id:guid}", async (Guid id, DeleteProductHandler handler) =>
{
    await handler.Handle(new DeleteProductRequest(id));
})
.WithTags("Products");

app.MapPut("/products/{id:guid}",
    async (Guid id, UpdateProductRequest request, UpdateProductHandler handler) =>
    {
        var updatedRequest = request with { Id = id };
        var result = await handler.Handle(updatedRequest);
        return result;
    })
    .WithTags("Products");

app.Run();
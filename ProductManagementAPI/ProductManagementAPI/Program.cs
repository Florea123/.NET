using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Persistance;
using ProductManagementAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ProductManagementContext>(options =>
    options.UseSqlite(("Data Source=productmanagement.db")));
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetAllProductsHandler>();
builder.Services.AddScoped<DeleteProductHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc
    (
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "An API to manage products",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "API Support",
                Email = "support@eexample.com"
            }
        }
    );
});

var app = builder.Build();

//Ensure the database is created at runtime
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductManagementContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI
    (c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API");
            c.RoutePrefix = string.Empty;
            c.DisplayRequestDuration();
        }
    );
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.MapPost("/product", async (CreateProductRequest req, CreateProductHandler handler) =>
    await handler.Handle(req));
app.MapGet("/product", async (GetAllProductsHandler handler) => 
    await handler.Handle(new GetAllProductsRequest()));
app.MapDelete("/product/{id:guid}", async (Guid id, DeleteProductHandler handler) =>
{
    await handler.Handle(new DeleteProductRequest(id));
});


app.Run();

using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Persistance;

public class ProductManagementContext(DbContextOptions<ProductManagementContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
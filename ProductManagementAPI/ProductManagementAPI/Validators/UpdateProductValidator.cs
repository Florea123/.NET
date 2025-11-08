using FluentValidation;
using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Validators;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().MinimumLength(3).WithMessage("Product Name must be at least 3 characters long.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(x => x.Category).IsInEnum().WithMessage("Category must be a valid ProductCategory.");
        RuleFor(x => x.SKU).NotNull().NotEmpty().WithMessage("SKU is required.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Stock Quantity cannot be negative.");
        RuleFor(x => x.ReleaseDate).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release Date cannot be in the future.");
        RuleFor(x => x.Brand).NotNull().NotEmpty().WithMessage("Brand is required.");
        RuleFor(x => x.IsAvailable).NotNull().WithMessage("Availability status is required.");
        RuleFor(x => x.CreatedAt).NotNull().WithMessage("Creation date is required.");
        ///RuleFor(x=> x.ImageUrl).Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("Image URL must be a valid URL if provided.");
    }
}
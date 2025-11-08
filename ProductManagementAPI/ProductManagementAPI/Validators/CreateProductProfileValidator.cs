using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Persistance;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProductManagementAPI.Validators;

public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ProductManagementContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;

    private static readonly string[] InappropriateWords = new[] { "badword1", "badword2", "restricted1" };
    private static readonly string[] HomeRestrictedWords = new[] { "hazard", "flammable" };
    private static readonly string[] TechnologyKeywords = new[] { "smart", "AI", "processor", "chip", "wireless", "bluetooth", "wifi", "usb" };
    private static readonly Regex BrandRegex = new(@"^[A-Za-z0-9\s\-\.'']+$", RegexOptions.Compiled);
    private static readonly Regex SkuRegex = new(@"^[A-Za-z0-9\-]{5,20}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] ImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public CreateProductProfileValidator(ProductManagementContext context, ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .Length(1, 200).WithMessage("Product name must be between 1 and 200 characters.")
            .Must(BeValidName).WithMessage("Product name contains inappropriate content.")
            .MustAsync(BeUniqueName).WithMessage("Product name must be unique for the same brand.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .Length(2, 100).WithMessage("Brand must be between 2 and 100 characters.")
            .Must(BeValidBrandName).WithMessage("Brand contains invalid characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .Must(BeValidSKU).WithMessage("SKU must be 5-20 characters, alphanumeric and may include hyphens.")
            .MustAsync(BeUniqueSKU).WithMessage("SKU must be unique in the system.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid ProductCategory.");

        RuleFor(x => x.Price)
            .GreaterThan(0m).WithMessage("Price must be greater than 0.")
            .LessThan(10000m).WithMessage("Price must be less than $10,000.");

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release Date cannot be in the future.")
            .Must(d => d.Year >= 1900).WithMessage("Release Date cannot be before year 1900.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock Quantity cannot be negative.")
            .LessThanOrEqualTo(100_000).WithMessage("Stock Quantity cannot exceed 100,000.");

        RuleFor(x => x.ImageUrl)
            .Cascade(CascadeMode.Stop)
            .Must(BeValidImageUrl).When(x => !string.IsNullOrWhiteSpace(x.ImageUrl)).WithMessage("ImageUrl must be a valid HTTP/HTTPS image URL ending with .jpg/.png/.gif/.webp/.jpeg");

        // complex business rules
        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("Business rules validation failed.");

        // Conditional validations
        When(x => x.Category == ProductCategory.Electronics, () =>
        {
            RuleFor(x => x.Price).GreaterThanOrEqualTo(50m).WithMessage("Electronics must have a minimum price of $50.00.");
            RuleFor(x => x.Name).Must(ContainTechnologyKeywords).WithMessage("Electronics product name must contain technology keywords.");
            RuleFor(x => x.ReleaseDate).Must(d => d >= DateTime.UtcNow.AddYears(-5)).WithMessage("Electronics must be released within the last 5 years.");
        });

        When(x => x.Category == ProductCategory.Home, () =>
        {
            RuleFor(x => x.Price).LessThanOrEqualTo(200m).WithMessage("Home products must have a price of $200.00 or less.");
            RuleFor(x => x.Name).Must(BeAppropriateForHome).WithMessage("Home product name contains restricted content.");
        });

        When(x => x.Category == ProductCategory.Clothing, () =>
        {
            RuleFor(x => x.Brand).MinimumLength(3).WithMessage("Clothing brand must be at least 3 characters long.");
        });

        // cross-field expensive product rule
        RuleFor(x => x)
            .Must(x => !(x.Price > 100m && x.StockQuantity > 20))
            .WithMessage("Expensive products (>$100) must have stock ≤ 20 units.");
    }

    private bool BeValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var lower = name.ToLowerInvariant();
        var found = InappropriateWords.Any(w => lower.Contains(w.ToLowerInvariant()));
        if (found) _logger.LogWarning("Name validation failed due to inappropriate content: {Name}", name);
        return !found;
    }

    private async Task<bool> BeUniqueName(CreateProductProfileRequest req, string name, CancellationToken ct)
    {
        var exists = await _context.Products.AnyAsync(p =>
            p.Name == name && p.Brand == req.Brand, ct);

        if (exists) _logger.LogInformation("Duplicate product name detected for Brand={Brand} Name={Name}", req.Brand, name);
        return !exists;
    }

    private bool BeValidBrandName(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand)) return false;
        var ok = BrandRegex.IsMatch(brand);
        if (!ok) _logger.LogWarning("Brand validation failed for value: {Brand}", brand);
        return ok;
    }

    private bool BeValidSKU(string sku)
    {
        if (sku is null) return false;
        var cleaned = sku.Replace(" ", string.Empty);
        var ok = SkuRegex.IsMatch(cleaned);
        if (!ok) _logger.LogWarning("SKU format invalid: {SKU}", sku);
        return ok;
    }

    private async Task<bool> BeUniqueSKU(string sku, CancellationToken ct)
    {
        var cleaned = sku?.Replace(" ", string.Empty) ?? string.Empty;
        var exists = await _context.Products.AnyAsync(p => p.SKU == cleaned, ct);
        if (exists) _logger.LogInformation("SKU already exists: {SKU}", cleaned);
        return !exists;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) return false;
        var ext = System.IO.Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        var ok = ImageExtensions.Contains(ext);
        if (!ok) _logger.LogWarning("Image URL invalid extension: {Url}", url);
        return ok;
    }

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest req, CancellationToken ct)
    {
        // Rule 1: Daily limit
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var dailyCount = await _context.Products.CountAsync(p => p.CreatedAt >= todayStart && p.CreatedAt < tomorrowStart, ct);
        if (dailyCount >= 500)
        {
            _logger.LogWarning("Daily product addition limit reached: {Count}", dailyCount);
            return false;
        }

        // Rule 2: Electronics minimum price
        if (req.Category == ProductCategory.Electronics && req.Price < 50m)
        {
            _logger.LogWarning("Electronics minimum price rule violated. Price={Price}", req.Price);
            return false;
        }

        // Rule 3: Home content restrictions
        if (req.Category == ProductCategory.Home)
        {
            var lower = req.Name.ToLowerInvariant();
            if (HomeRestrictedWords.Any(w => lower.Contains(w)))
            {
                _logger.LogWarning("Home product content restriction violated. Name={Name}", req.Name);
                return false;
            }
        }

        // Rule 4: High-value stock limit
        if (req.Price > 500m && req.StockQuantity > 10)
        {
            _logger.LogWarning("High-value product stock limit violated. Price={Price} Stock={Stock}", req.Price, req.StockQuantity);
            return false;
        }

        _logger.LogInformation("All business rules passed for Operation on SKU={SKU}", req.SKU);
        return true;
    }

    private bool ContainTechnologyKeywords(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var lower = name.ToLowerInvariant();
        return TechnologyKeywords.Any(k => lower.Contains(k.ToLowerInvariant()));
    }

    private bool BeAppropriateForHome(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var lower = name.ToLowerInvariant();
        return !InappropriateWords.Any(w => lower.Contains(w.ToLowerInvariant()));
    }
}
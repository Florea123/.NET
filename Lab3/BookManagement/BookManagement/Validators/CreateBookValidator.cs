using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x=> x.Title).NotNull().NotEmpty().MinimumLength(1).WithMessage("Title must be at least 1 character long.");
        RuleFor(x=> x.Author).NotNull().NotEmpty().MinimumLength(3).WithMessage("Author must be at least 3 characters long.");
        RuleFor(x=> x.YearPublished).InclusiveBetween(1450, DateTime.Now.Year).WithMessage($"YearPublished must be between 1450 and {DateTime.Now.Year}.");
    }
}
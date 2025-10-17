namespace BookManagement.Features.Books;

public record UpdateBookRequest(Guid Id, string Title, string Author, int YearPublished);
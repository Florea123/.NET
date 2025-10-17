namespace BookManagement.Features.Books;

public record GetAllBookRequest(string ?Author,string SortBy = "title", string SortOrder="asc",int PageNumber=1, int PageSize=100);
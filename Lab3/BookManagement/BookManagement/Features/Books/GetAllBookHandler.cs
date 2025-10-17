using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class GetAllBookHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;
    
    public async Task<IResult> Handle(GetAllBookRequest request)
    {
        var query = _context.Books.AsQueryable();
        //filter by author if provided
        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            var filter = request.Author.Trim().ToLower();
            query = query.Where(b => b.Author.ToLower().Contains(filter));
        }

        var sortBy = (request.SortBy ?? "title").Trim().ToLower();
        var sortOrder = (request.SortOrder ?? "asc").Trim().ToLower();

        if (sortBy == "year" || sortBy == "yearpublished")
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(b => b.YearPublished)
                : query.OrderBy(b => b.YearPublished);
        }
        else // default to title
        {
            query = sortOrder == "desc"
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title);
        }
        
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        
        var books = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Results.Ok(books);
    }
}
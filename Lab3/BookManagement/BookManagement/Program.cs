using BookManagement.Features.Books;
using BookManagement.Middleware;
using BookManagement.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BookManagementContext>(options =>
        options.UseSqlite(("Data Source=bookmanagement.db")));
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookRequest>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateBookRequest>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookManagementContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/books", async (CreateBookRequest req, CreateBookHandler handler) =>
    await handler.Handle(req));
app.MapGet("/books", async ([AsParameters] GetAllBookRequest req, GetAllBookHandler handler) =>
await handler.Handle(req));
app.MapDelete("/books/{id:guid}", async (Guid id, DeleteBookHandler handler) =>
    await handler.Handle(new DeleteBookRequest(id)));
app.MapPut("/books/{id:guid}", async (Guid id, UpdateBookRequest req, UpdateBookHandler handler) =>
    await handler.Handle(req with { Id = id }));



app.Run();

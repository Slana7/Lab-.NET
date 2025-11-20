﻿using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using Lab4.Data;
using Lab4.Common.Middleware;
using Lab4.Features.Books;
using Lab4.Features.Books.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite("Data Source=books.db"));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add AutoMapper with both profiles
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add FluentValidation - register all validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add correlation middleware (must be first to ensure correlation ID is available for all logs)
app.UseMiddleware<CorrelationMiddleware>();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    context.Database.EnsureCreated();
}

// Configure minimal API endpoints
app.MapPost("/books", async (CreateBookProfileRequest request, IMediator mediator) =>
{
    var command = new CreateBookCommand(request);
    var book = await mediator.Send(command);
    return Results.Created($"/books/{book.Id}", book);
})
.WithName("CreateBook")
.WithTags("Books")
.WithSummary("Create a new book")
.WithDescription("Creates a new book with advanced validation and mapping")
.WithOpenApi();

app.MapGet("/books/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var book = await mediator.Send(new GetBookByIdQuery(id));
    return book is not null ? Results.Ok(book) : Results.NotFound();
})
.WithName("GetBookById")
.WithTags("Books")
.WithSummary("Get a book by ID")
.WithDescription("Retrieves a single book by its unique identifier")
.WithOpenApi();

app.MapGet("/books", async (IMediator mediator) =>
{
    var books = await mediator.Send(new GetAllBooksQuery());
    return Results.Ok(books);
})
.WithName("GetAllBooks")
.WithTags("Books")
.WithSummary("Get all books")
.WithDescription("Retrieves all books from the database")
.WithOpenApi();

app.MapDelete("/books/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteBookCommand(id));
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBook")
.WithTags("Books")
.WithSummary("Delete a book")
.WithDescription("Deletes a book by its unique identifier")
.WithOpenApi();

app.Run();

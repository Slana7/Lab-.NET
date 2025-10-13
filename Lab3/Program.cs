using Microsoft.EntityFrameworkCore;
using MediatR;
using Lab3.Data;
using Lab3.Commands;
using Lab3.Queries;
using Lab3.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite("Data Source=books.db"));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    context.Database.EnsureCreated();
}

// Configure minimal API endpoints
app.MapPost("/books", async (CreateBookCommand command, IMediator mediator) =>
{
    var book = await mediator.Send(command);
    return Results.Created($"/books/{book.Id}", book);
})
.WithName("CreateBook")
.WithOpenApi();

app.MapGet("/books/{id:int}", async (int id, IMediator mediator) =>
{
    var book = await mediator.Send(new GetBookByIdQuery(id));
    return book is not null ? Results.Ok(book) : Results.NotFound();
})
.WithName("GetBookById")
.WithOpenApi();

app.MapGet("/books", async (IMediator mediator) =>
{
    var books = await mediator.Send(new GetAllBooksQuery());
    return Results.Ok(books);
})
.WithName("GetAllBooks")
.WithOpenApi();

app.MapDelete("/books/{id:int}", async (int id, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteBookCommand(id));
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBook")
.WithOpenApi();

app.Run();

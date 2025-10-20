using Microsoft.EntityFrameworkCore;
using MediatR;
using Lab4.Data;
using Lab4.Commands;
using Lab4.Queries;
using Lab4.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite("Data Source=books.db"));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

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

app.MapGet("/books/{id:guid}", async (Guid id, IMediator mediator) =>
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

app.MapDelete("/books/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteBookCommand(id));
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBook")
.WithOpenApi();

app.Run();

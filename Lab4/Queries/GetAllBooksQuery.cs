using MediatR;
using Lab4.Models;

namespace Lab4.Queries;

public record GetAllBooksQuery : IRequest<List<Book>>;

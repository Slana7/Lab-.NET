using MediatR;
using Lab3.Models;

namespace Lab3.Queries;

public record GetAllBooksQuery() : IRequest<List<Book>>;

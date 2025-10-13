using MediatR;
using Lab3.Models;

namespace Lab3.Queries;

public record GetBookByIdQuery(int Id) : IRequest<Book?>;

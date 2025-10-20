using MediatR;
using Lab4.Models;

namespace Lab4.Queries;

public record GetBookByIdQuery(Guid Id) : IRequest<Book?>;

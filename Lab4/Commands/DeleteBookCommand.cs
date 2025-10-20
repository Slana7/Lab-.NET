using MediatR;

namespace Lab4.Commands;

public record DeleteBookCommand(Guid Id) : IRequest<bool>;

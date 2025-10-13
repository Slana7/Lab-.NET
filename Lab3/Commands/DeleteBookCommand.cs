using MediatR;

namespace Lab3.Commands;

public record DeleteBookCommand(int Id) : IRequest<bool>;

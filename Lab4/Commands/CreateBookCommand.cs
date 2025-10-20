using MediatR;
using Lab4.Models;

namespace Lab4.Commands;

public record CreateBookCommand(
    string Title,
    string Author,
    int Year
) : IRequest<Book>;

using MediatR;
using Lab3.Models;

namespace Lab3.Commands;

public record CreateBookCommand(
    string Title,
    string Author,
    int Year
) : IRequest<Book>;

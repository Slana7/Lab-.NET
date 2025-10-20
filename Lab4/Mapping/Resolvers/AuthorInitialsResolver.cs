using AutoMapper;
using Lab4.Models;

namespace Lab4.Mapping.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.Author))
            return "?";

        var nameParts = source.Author.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length == 0)
            return "?";
        
        if (nameParts.Length == 1)
            return nameParts[0][0].ToString().ToUpper();
        
        // Two or more names: first letter of first and last names
        return $"{nameParts[0][0]}{nameParts[^1][0]}".ToUpper();
    }
}


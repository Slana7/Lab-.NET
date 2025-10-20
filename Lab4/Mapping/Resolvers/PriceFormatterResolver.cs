using AutoMapper;
using Lab4.Models;

namespace Lab4.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2");
    }
}


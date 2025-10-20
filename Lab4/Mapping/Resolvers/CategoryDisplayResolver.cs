using AutoMapper;
using Lab4.Models;

namespace Lab4.Mapping.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            BookCategory.Fiction => "Fiction & Literature",
            BookCategory.NonFiction => "Non-Fiction",
            BookCategory.Technical => "Technical & Professional",
            BookCategory.Children => "Children's Books",
            _ => "Uncategorized"
        };
    }
}


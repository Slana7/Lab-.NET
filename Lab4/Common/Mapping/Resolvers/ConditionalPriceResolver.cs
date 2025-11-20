using AutoMapper;
using Lab4.Features.Books;

namespace Lab4.Common.Mapping.Resolvers;

public class ConditionalPriceResolver : IValueResolver<Book, object, decimal>
{
    public decimal Resolve(Book source, object destination, decimal destMember, ResolutionContext context)
    {
        // Apply 10% discount for Children category
        if (source.Category == BookCategory.Children)
        {
            return source.Price * 0.9m;
        }
        
        // Return actual price for all other categories
        return source.Price;
    }
}



using AutoMapper;
using Lab4.Features.Books;
using System.Globalization;

namespace Lab4.Common.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        // Apply the same conditional logic as ConditionalPriceResolver
        var price = source.Category == BookCategory.Children 
            ? source.Price * 0.9m 
            : source.Price;
            
        return price.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}



using AutoMapper;
using Lab4.Features.Books;

namespace Lab4.Common.Mapping.Resolvers;

public class ConditionalCoverImageResolver : IValueResolver<Book, object, string?>
{
    public string? Resolve(Book source, object destination, string? destMember, ResolutionContext context)
    {
        // Return null for Children category (content filtering)
        if (source.Category == BookCategory.Children)
        {
            return null;
        }
        
        // Return actual URL for Fiction, NonFiction, Technical categories
        return source.CoverImageUrl;
    }
}


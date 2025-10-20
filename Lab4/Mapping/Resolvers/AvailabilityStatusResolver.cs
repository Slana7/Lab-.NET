using AutoMapper;
using Lab4.Models;

namespace Lab4.Mapping.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
            return "Out of Stock";
        
        if (source.StockQuantity == 0)
            return "Unavailable";
        
        if (source.StockQuantity == 1)
            return "Last Copy";
        
        if (source.StockQuantity <= 5)
            return "Limited Stock";
        
        return "In Stock";
    }
}


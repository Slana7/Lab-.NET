using AutoMapper;
using Lab4.Models;

namespace Lab4.Mapping.Resolvers;

public class PublishedAgeResolver : IValueResolver<Book, object, string>
{
    public string Resolve(Book source, object destination, string destMember, ResolutionContext context)
    {
        var daysSincePublished = (DateTime.UtcNow - source.PublishedDate).TotalDays;

        if (daysSincePublished < 30)
            return "New Release";
        
        if (daysSincePublished < 365)
        {
            var months = (int)(daysSincePublished / 30);
            return $"{months} month{(months == 1 ? "" : "s")} old";
        }
        
        if (daysSincePublished < 1825) // 5 years
        {
            var years = (int)(daysSincePublished / 365);
            return $"{years} year{(years == 1 ? "" : "s")} old";
        }
        
        return "Classic";
    }
}


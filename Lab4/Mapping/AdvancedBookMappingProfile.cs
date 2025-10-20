using AutoMapper;
using Lab4.Models;
using Lab4.DTOs;
using Lab4.Mapping.Resolvers;

namespace Lab4.Mapping;

public class AdvancedBookMappingProfile : Profile
{
    public AdvancedBookMappingProfile()
    {
        // Map CreateBookProfileRequest to Book
        CreateMap<CreateBookProfileRequest, Book>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.PublishedDate.Year));

        // Map Book to BookProfileDto with custom resolvers
        CreateMap<Book, BookProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
            .ForMember(dest => dest.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
    }
}


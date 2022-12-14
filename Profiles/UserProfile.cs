using AutoMapper;
using Google.Apis.Auth;

namespace multi_login.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Entities.User, Models.UserFriendlyDTO>();

        CreateMap<Models.UserForCreationDTO, Entities.User>();

        CreateMap<GoogleJsonWebSignature.Payload, Entities.User>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "normal"))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => "Google"))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }
}
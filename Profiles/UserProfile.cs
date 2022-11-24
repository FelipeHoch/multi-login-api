using AutoMapper;

namespace multi_login.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Entities.User, Models.UserDTO>();

        CreateMap<Models.UserForCreationDTO, Entities.User>(); 
    }
}

using AutoMapper;
using tree_form_API.Models;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRegistrationDTO, User>();
        CreateMap<User, UserResponseDTO>();
    }
}

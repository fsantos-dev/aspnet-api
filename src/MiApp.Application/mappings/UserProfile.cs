using AutoMapper;
using MiApp.Application.Dtos;
using MiApp.Domain.Entities;


namespace MiApp.Application.Mappings;


public class UserProfile : Profile
{

    public UserProfile()
    {
        // Para el request del login (RegisterRequestDto → User)
        CreateMap<LoginRequestDto, User>();

        // Para el request del registro (RegisterRequestDto → User)
        CreateMap<RegisterRequestDto, User>();

        // Para la response del registro y el login (User → LoginResponseDto)
        CreateMap<User, LoginResponseDto>();
    }
   
}
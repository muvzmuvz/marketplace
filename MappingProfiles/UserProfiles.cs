using AutoMapper;
using marketplace_api.Models;
using marketplace_api.ModelsDto;

namespace marketplace_api.Mapping;

public class UserProfiles : Profile
{
    public UserProfiles()
    {
        CreateMap<UserDto, User>()
    .ForMember(user => user.Name, opt => opt.MapFrom(dto => dto.Name))
    .ForMember(user => user.Email, opt => opt.MapFrom(dto => dto.Email))
    .ForMember(user => user.HashPassword, opt => opt.MapFrom(dto => dto.HashPassword));

        CreateMap<User, UserDto>();

        CreateMap<User,UserResponseDto>();

        CreateMap<AdminDto, User>()
                .ForMember(dest => dest.HashPassword, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        CreateMap<User, AdminDto>();

        CreateMap<User, UserResponseDto>();

        CreateMap<User, UserLoginDto>();

        CreateMap<UserLoginDto, User>();
    }
}

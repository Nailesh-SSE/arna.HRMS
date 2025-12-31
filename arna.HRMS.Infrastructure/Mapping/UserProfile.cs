using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Map entity -> DTO
        CreateMap<User, UserDto>();

        // Map DTO -> entity 
        CreateMap<UserDto, User>();
    }
}
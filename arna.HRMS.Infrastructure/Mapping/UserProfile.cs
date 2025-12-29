using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // ======================
        // INSERT
        // ======================
        CreateMap<CreateUserRequest, User>();

        // ======================
        // UPDATE
        // ======================
        CreateMap<UpdateUserRequest, User>();
            

        // ======================
        // RESPONSE
        // ======================
        CreateMap<User, UserDto>();
    }
}

using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        // Map entity -> DTO
        CreateMap<Role, RoleDto>();

        // Map DTO -> entity 
        CreateMap<RoleDto, Role>();
    }
}

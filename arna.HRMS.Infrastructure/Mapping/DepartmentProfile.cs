using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<DepartmentDto, Department>();
        CreateMap<DepartmentDto, Department>();
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest=>dest.parentDepartMentName,
                opt=>opt.MapFrom(src=> src.ParentDepartment != null ? $"{src.ParentDepartment.Name}":""));
    }
}

using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<DepartmentDto, Department>();
        CreateMap<DepartmentDto, Department>();
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest=>dest.ParentDepartMentName,
                opt=>opt.MapFrom(src=> src.ParentDepartment != null ? $"{src.ParentDepartment.Name}":""));
    }
}

using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<EmployeeDto, Employee>().ReverseMap()
            .ForMember(dest => dest.DepartmentCode,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.Code : ""))
            .ForMember(dest => dest.ManagerFullName,
                opt => opt.MapFrom(src =>
                    src.Manager != null
                        ? $"{src.Manager.FirstName} {src.Manager.LastName}"
                        : ""));
    }
}

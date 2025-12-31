using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;


public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();
        CreateMap<Employee, EmployeeDto>()
              .ForMember(dest => dest.DepartmentCode,
                opt => opt.MapFrom(src =>
                    src.Department.Code ?? ""))
              .ForMember(dest => dest.ManagerFullName,
                opt => opt.MapFrom(src =>
                    src.Manager.FirstName + src.Manager.LastName ?? ""));

    }
}

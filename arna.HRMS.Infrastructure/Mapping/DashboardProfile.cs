using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class DashboardProfile : Profile
{
    public DashboardProfile()
    {
        CreateMap<Employee, EmployeeDto>();

        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee.FirstName + " " + s.Employee.LastName))
            .ForMember(d => d.EmployeeNumber,
                o => o.MapFrom(s => s.Employee.EmployeeNumber))
            .ForMember(d => d.LeaveTypeName,
                o => o.MapFrom(s => s.LeaveType.LeaveNameId.ToString()));
    }
}

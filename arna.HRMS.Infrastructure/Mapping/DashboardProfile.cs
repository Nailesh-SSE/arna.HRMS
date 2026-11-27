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
                o => o.MapFrom(s => s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}".Trim() : string.Empty))
            .ForMember(d => d.EmployeeNumber,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty))
            .ForMember(d => d.LeaveTypeName,
                o => o.MapFrom(s => s.LeaveType != null ? ((arna.HRMS.Core.Enums.LeaveName)s.LeaveType.LeaveNameId).ToString() : string.Empty));

        CreateMap<AttendanceRequest, AttendanceRequestDto>()
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}".Trim() : string.Empty))
            .ForMember(d => d.EmployeeNumber,
                o => o.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty));
    }
}

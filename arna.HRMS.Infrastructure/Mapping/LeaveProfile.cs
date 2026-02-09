using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class LeaveProfile : Profile
{
    public LeaveProfile()
    {
        //Leave Master
        CreateMap<LeaveMasterDto, LeaveMaster>().ReverseMap();

        //Leave Request
        // DTO → Entity
        CreateMap<LeaveRequestDto, LeaveRequest>()
            .ForMember(d => d.ApprovedBy,
                o => o.MapFrom(s =>
                    s.ApprovedBy.HasValue && s.ApprovedBy.Value > 0
                        ? s.ApprovedBy
                        : null))

            .ForMember(d => d.ApprovalNotes,
                o => o.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.ApprovalNotes)
                        ? null
                        : s.ApprovalNotes))

            .ForMember(d => d.ApprovedDate,
                o => o.MapFrom(s =>
                    s.ApprovedDate.HasValue
                        ? s.ApprovedDate
                        : null));

        // Entity → DTO
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(d => d.LeaveTypeName,
                o => o.MapFrom(s => s.LeaveType != null
                    ? s.LeaveType.LeaveName
                    : null))
            .ForMember(d => d.EmployeeName,
                o => o.MapFrom(s => s.Employee != null
                    ? $"{s.Employee.FirstName} {s.Employee.LastName}"
                    : null))
            .ForMember(d => d.ApprovedByName,
                o => o.MapFrom(s => s.ApprovedByEmployee != null
                    ? $"{s.ApprovedByEmployee.FirstName} {s.ApprovedByEmployee.LastName}"
                    : null))
            .ForMember(d => d.EmployeeNumber,
                o => o.MapFrom(s => s.Employee != null
                    ? s.Employee.EmployeeNumber
                    : null));
        
        //Leave Balance
        CreateMap<EmployeeLeaveBalanceDto, EmployeeLeaveBalance>();

        CreateMap<EmployeeLeaveBalance, EmployeeLeaveBalanceDto>()
            .ForMember(dest => dest.LeaveMasterName, 
            opt => opt.MapFrom(src => src.LeaveMaster != null ? $"{src.LeaveMaster.LeaveName}" : ""));
    }
}

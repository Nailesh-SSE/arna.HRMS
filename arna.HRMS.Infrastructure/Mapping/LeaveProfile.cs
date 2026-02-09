using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class LeaveProfile : Profile
{
    public LeaveProfile()
    {
        // ============================
        // LeaveType ↔ LeaveTypeDto
        // ============================

        CreateMap<LeaveTypeDto, LeaveType>()
            .ForMember(
                dest => dest.LeaveNameId,
                opt => opt.MapFrom(src => (int)src.LeaveNameId)
            );

        CreateMap<LeaveType, LeaveTypeDto>()
            .ForMember(
                dest => dest.LeaveNameId,
                opt => opt.MapFrom(src => (LeaveName)src.LeaveNameId)
            );

        // ============================
        // LeaveRequest ↔ LeaveRequestDto
        // ============================

        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(
                dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee != null
                        ? $"{src.Employee.FirstName} {src.Employee.LastName}"
                        : null)
            )
            .ForMember(
                dest => dest.ApprovedByName,
                opt => opt.MapFrom(src =>
                    src.ApprovedByEmployee != null
                        ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}"
                        : null)
            )
            .ForMember(
                dest => dest.LeaveTypeName,
                opt => opt.MapFrom(src =>
                    src.LeaveType != null
                        ? ((LeaveName)src.LeaveType.LeaveNameId).ToString()
                        : null)
            );

        CreateMap<LeaveRequestDto, LeaveRequest>()
            .ForMember(
                dest => dest.LeaveTypeId,
                opt => opt.MapFrom(src => src.LeaveTypeId)
            );

    }
}

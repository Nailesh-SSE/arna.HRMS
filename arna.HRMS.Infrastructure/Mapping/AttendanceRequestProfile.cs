using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class AttendanceRequestProfile : Profile
{
    public AttendanceRequestProfile()
    {
        CreateMap<AttendanceRequestDto, AttendanceRequest>()
            .ForMember(dest => dest.ReasonType,
                opt => opt.MapFrom(src => src.ReasonType.ToString()))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(src => src.Location.ToString()));

        CreateMap<AttendanceRequest, AttendanceRequestDto>()
            .ForMember(dest => dest.ReasonType,
                opt => opt.MapFrom(src =>
                    Enum.Parse<AttendanceReasonType>(src.ReasonType)))
            .ForMember(dest => dest.Location,
                opt => opt.MapFrom(src =>
                    Enum.Parse<AttendanceLocation>(src.Location)))
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee != null
                        ? $"{src.Employee.FirstName} {src.Employee.LastName}"
                        : string.Empty));
    }
}

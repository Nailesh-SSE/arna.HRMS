using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<CreateAttendanceRequest, Attendance>()
            .ForMember(dest => dest.ClockIn,
                opt => opt.MapFrom(src =>
                    src.ClockInTime.HasValue
                        ? src.Date.Date + src.ClockInTime.Value
                        : (DateTime?)null))
            .ForMember(dest => dest.ClockOut,
                opt => opt.MapFrom(src =>
                    src.ClockOutTime.HasValue
                        ? src.Date.Date + src.ClockOutTime.Value
                        : (DateTime?)null))
            .ForMember(dest => dest.TotalHours,
                opt => opt.MapFrom(src =>
                    TimeSpan.FromHours(src.WorkingHours)));

        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.ClockInTime,
                opt => opt.MapFrom(src =>
                    src.ClockIn.HasValue
                        ? src.ClockIn.Value.TimeOfDay
                        : TimeSpan.Zero))
            .ForMember(dest => dest.ClockOutTime,
                opt => opt.MapFrom(src =>
                    src.ClockOut.HasValue
                        ? src.ClockOut.Value.TimeOfDay
                        : (TimeSpan?)null))
            .ForMember(dest => dest.WorkingHours,
                opt => opt.MapFrom(src =>
                    src.TotalHours.HasValue
                        ? src.TotalHours.Value.TotalHours
                        : 0));
    }
}

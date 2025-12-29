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
                    src.Date.Date + src.CheckInTime))

            .ForMember(dest => dest.ClockOut,
                opt => opt.MapFrom(src =>
                    src.CheckOutTime.HasValue
                        ? src.Date.Date + src.CheckOutTime.Value
                        : (DateTime?)null))

            .ForMember(dest => dest.TotalHours,
                opt => opt.MapFrom(src =>
                    TimeSpan.FromHours(src.WorkingHours)));

        CreateMap<UpdateAttendanceRequest, Attendance>()
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src =>
            (arna.HRMS.Core.Enums.AttendanceStatus)src.Status))
    .ForMember(dest => dest.ClockOut,
        opt => opt.MapFrom(src =>
            src.CheckOutTime.HasValue
                ? DateTime.Today + src.CheckOutTime.Value
                : (DateTime?)null))
    .ForMember(dest => dest.TotalHours,
        opt => opt.MapFrom(src =>
            TimeSpan.FromHours(src.WorkingHours)))
    .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
    .ForMember(dest => dest.Date, opt => opt.Ignore())
    .ForMember(dest => dest.ClockIn, opt => opt.Ignore());




        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.CheckInTime,
                opt => opt.MapFrom(src =>
                    src.ClockIn.HasValue
                        ? src.ClockIn.Value.TimeOfDay
                        : TimeSpan.Zero))

            .ForMember(dest => dest.CheckOutTime,
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

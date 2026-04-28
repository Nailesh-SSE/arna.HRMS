using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class FestivalHolidayProfile : Profile
{
    public FestivalHolidayProfile()
    {
        CreateMap<FestivalHoliday, FestivalHolidayDto>()
           .ForMember(d => d.FestivalName, opt => opt.MapFrom(s => s.FestivalName))
           .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date))
           .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
           .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id));

        // DTO -> Entity
        CreateMap<FestivalHolidayDto, FestivalHoliday>()
            .ForMember(e => e.FestivalName, opt => opt.MapFrom(d => d.FestivalName))
            .ForMember(e => e.Date, opt => opt.MapFrom(d => d.Date))
            .ForMember(e => e.Description, opt => opt.MapFrom(d => d.Description))
            // DayOfWeek kept in entity; populate from Date when creating/updating
            .ForMember(e => e.DayOfWeek, opt => opt.MapFrom(d => d.Date.DayOfWeek.ToString()));
    }
}

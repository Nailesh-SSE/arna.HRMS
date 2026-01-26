using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class FestivalHolidayProfile : Profile
{
    public FestivalHolidayProfile()
    {
        CreateMap<FestivalHolidayDto, FestivalHoliday>().ReverseMap();
    }
}

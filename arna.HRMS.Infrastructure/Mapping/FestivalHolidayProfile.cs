using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class FestivalHolidayProfile : Profile
{
    public FestivalHolidayProfile()
    {
        CreateMap<FestivalHolidayDto, FestivalHoliday>().ReverseMap();
    }
}

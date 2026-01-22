using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class AttendanceRequestProfile : Profile
{
    public AttendanceRequestProfile()
    {
        CreateMap<AttendanceRequestDto, AttendanceRequest>().ReverseMap()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee != null
                        ? $"{src.Employee.FirstName} {src.Employee.LastName}"
                        : string.Empty))
            .ForMember(dest => dest.ApprovedByName,
                opt => opt.MapFrom(src =>
                    src.ApprovedByEmployee != null
                        ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}"
                        : string.Empty))
            ;
    }
}

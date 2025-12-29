using AutoMapper;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Mapping;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        // INSERT
        CreateMap<CreateDepartmentRequest, Department>();

        // UPDATE
        CreateMap<UpdateDepartmentRequest, Department>();

        // RESPONSE
        CreateMap<Department, DepartmentDto>();
    }
}

using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Map entity -> DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : ""))
            .ForMember(dest => dest.Role,
                opt => opt.MapFrom(src =>
                    src.Role != null ? src.Role.Name : ""));

        // Map DTO -> entity 
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt =>
                opt.MapFrom(src => HashPassword(src.Password) ?? ""))
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());
    }

    private string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
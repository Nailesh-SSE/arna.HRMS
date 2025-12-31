using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = arna.HRMS.Core.DTOs.Requests.RegisterRequest;

namespace arna.HRMS.Infrastructure.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<LoginRequest, UserDto>();

        CreateMap<RegisterRequest, UserDto>()
            .ForMember(dest => dest.PasswordHash, opt =>
                opt.MapFrom(src => HashPassword(src.Password) ?? ""));

        // Map entity -> DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : ""));

        // Map DTO -> entity 
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt =>
                opt.MapFrom(src => HashPassword(src.Password) ?? ""));
    }

    private string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
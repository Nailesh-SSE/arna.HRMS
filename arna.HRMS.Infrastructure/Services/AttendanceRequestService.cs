using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _attendanceRequestRepository;
    private readonly IMapper _mapper;

    public AttendanceRequestService(AttendanceRequestRepository attendanceRequestRepository, IMapper mapper)
    {
        _attendanceRequestRepository = attendanceRequestRepository;
        _mapper = mapper;
    }

    public async Task<List<AttendanceRequestDto>> GetAttendanceRequestAsync()
    {
        var AttendanceRequest = await _attendanceRequestRepository.GetAttendanceRequestAsync();
        return _mapper.Map<List<AttendanceRequestDto>>(AttendanceRequest);
    }

    public async Task<AttendanceRequestDto?> GetAttendenceRequestByIdAsync(int id)
    {
        var attendance = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);
        return attendance == null ? null : _mapper.Map<AttendanceRequestDto>(attendance);
    }

    public async Task<AttendanceRequestDto> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto)
    {
        var attendancerRequestEntity = _mapper.Map<AttendanceRequest>(attendanceRequestDto);
        var createdAttendanceRequest = await _attendanceRequestRepository.CreateAttendanceRequestAsync(attendancerRequestEntity);

        return _mapper.Map<AttendanceRequestDto>(createdAttendanceRequest);
    }

    public async Task<bool> UpdateAttendanceRequestStatusAsync(int id)
    {
        return await _attendanceRequestRepository.UpdateAttendanceRequestStatusAsync(id);
    }
}

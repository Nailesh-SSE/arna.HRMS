using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;

    public AttendanceService(AttendanceRepository attendanceRepository, IMapper mapper)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync()
    {
        var Attendance = await _attendanceRepository.GetAttendenceAsync();
        return Attendance.Select(e => _mapper.Map<AttendanceDto>(e)).ToList();
    }

    public async Task<AttendanceDto?> GetAttendenceByIdAsync(int id)
    {
        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
        if (attendance == null) return null;
        var attendancedto = _mapper.Map<AttendanceDto>(attendance);

        return attendancedto;
    }

    public async Task<AttendanceDto> CreateAttendanceAsync(Attendance attendance)
    {
        var createdAttendance = await _attendanceRepository.CreateAttendanceDtoAsync(attendance);
        return _mapper.Map<AttendanceDto>(createdAttendance);
    }
    //public async Task<AttendanceDto> UpdateAttendanceAsync(Attendance attendance)
    //{
    //    var updatedAttendance = await _attendanceRepository.UpdateAttendanceAsync(attendance);
    //    return _mapper.Map<AttendanceDto>(updatedAttendance);
    //}
    //public async Task<Attendance?> GetAttendanceEntityByIdAsync(int id)
    //{
    //    return await _attendanceRepository.GetAttendanceByIdAsync(id);
    //}

}

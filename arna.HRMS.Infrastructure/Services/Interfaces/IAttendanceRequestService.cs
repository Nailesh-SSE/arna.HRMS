using arna.HRMS.Core.Enums;
﻿using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IAttendanceRequestService
{
    Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync();
    Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestesAsync();
    Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id);
    Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestsByEmployeeAsync(int employeeId);
    Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusCancleAsync(int id, int EmployeeId);
}

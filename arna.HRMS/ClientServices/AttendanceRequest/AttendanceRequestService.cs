using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.AttendanceRequest;

public interface IAttendanceRequestService
{
    Task<ApiResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync();
    Task<ApiResult<AttendanceRequestDto>> GetAttendanceRequestByIdAsync(int id);
    Task<ApiResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto AttendanceRequestDto);
    Task<ApiResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(int id);
}

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly ApiClients.AttendanceRequestApi _attendanceRequest;

    public AttendanceRequestService(ApiClients api)
    {
        _attendanceRequest = api.AttendanceRequest;
    }

    public Task<ApiResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync()
        => _attendanceRequest.GetAll();

    public Task<ApiResult<AttendanceRequestDto>> GetAttendanceRequestByIdAsync(int id)
        => _attendanceRequest.GetById(id);

    public Task<ApiResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto dto)
        => _attendanceRequest.Create(dto);

    public Task<ApiResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(int id)
        => _attendanceRequest.ApproveRequest(id);  
}

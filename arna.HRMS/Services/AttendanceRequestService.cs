using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IAttendanceRequestService
{
    Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync();
    Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId);
    Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(Status? status);
    Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId, Status? status);
    Task<ApiResult<AttendanceRequestViewModel>> GetAttendanceRequestByIdAsync(int id);
    Task<ApiResult<AttendanceRequestViewModel>> CreateAttendanceRequestAsync(AttendanceRequestViewModel model);
    Task<ApiResult<bool>> UpdateAttendanceRequestAsync(AttendanceRequestViewModel model);
    Task<ApiResult<bool>> DeleteAttendanceRequestAsync(int id);
    Task<ApiResult<bool>> UpdateRequestStatusAsync(int id, Status status);
    Task<ApiResult<bool>> CancelRequestAsync(int id);
}

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly ApiClients.AttendanceRequestApi _attendanceRequest;

    public AttendanceRequestService(ApiClients api)
    {
        _attendanceRequest = api.AttendanceRequests;
    }

    public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync()
    {
        return await _attendanceRequest.GetAll();
    }

    public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId)
    {
        return await _attendanceRequest.GetAll(empId);
    }

    public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(Status? status)
    {
        return await _attendanceRequest.GetAll(null, status);
    }

    public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId, Status? status)
    {
        return await _attendanceRequest.GetAll(empId, status);
    }

    public async Task<ApiResult<AttendanceRequestViewModel>> GetAttendanceRequestByIdAsync(int id)
    {
        return await _attendanceRequest.GetByIdAsync(id);
    }

    public async Task<ApiResult<AttendanceRequestViewModel>> CreateAttendanceRequestAsync(AttendanceRequestViewModel model)
    {
        return await _attendanceRequest.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateAttendanceRequestAsync(AttendanceRequestViewModel model)
    {
        return await _attendanceRequest.UpdateAsync(model.Id, model);
    }

    public async Task<ApiResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        return await _attendanceRequest.DeleteAsync(id);
    }

    public async Task<ApiResult<bool>> UpdateRequestStatusAsync(int id, Status status)
    {
        return await _attendanceRequest.UpdateStatusAsync(id, status);
    }

    public async Task<ApiResult<bool>> CancelRequestAsync(int id)
    {
        return await _attendanceRequest.CancelRequestAsync(id); 
    }
}
using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.ClientServices.Client.AttendanceRequest;

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
    Task<ApiResult<bool>> UpdateRequestStatus(int id, Status status);
}

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly ApiClients.AttendanceRequestApi _attendanceRequest;

    public AttendanceRequestService(ApiClients api)
    {
        _attendanceRequest = api.AttendanceRequests;
    }

    public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync() =>
        _attendanceRequest.GetAll();

    public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId) =>
        _attendanceRequest.GetAll(empId);

    public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(Status? status) =>
        _attendanceRequest.GetAll(status);

    public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllAsync(int? empId, Status? status) =>
        _attendanceRequest.GetAll(empId, status);

    public Task<ApiResult<AttendanceRequestViewModel>> GetAttendanceRequestByIdAsync(int id) =>
        _attendanceRequest.GetById(id);

    public Task<ApiResult<AttendanceRequestViewModel>> CreateAttendanceRequestAsync(AttendanceRequestViewModel model) =>
        _attendanceRequest.Create(model);

    public Task<ApiResult<bool>> UpdateAttendanceRequestAsync(AttendanceRequestViewModel model) =>
        _attendanceRequest.Update(model);

    public Task<ApiResult<bool>> DeleteAttendanceRequestAsync(int id) =>
        _attendanceRequest.Delete(id);

    public Task<ApiResult<bool>> UpdateRequestStatus(int id, Status status) =>
        _attendanceRequest.UpdateRequestStatus(id, status);
}

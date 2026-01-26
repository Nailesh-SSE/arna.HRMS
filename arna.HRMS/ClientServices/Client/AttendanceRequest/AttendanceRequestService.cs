using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.ClientServices.Client.AttendanceRequest;

public interface IAttendanceRequestService
{
    Task<ApiResult<List<AttendanceRequestViewModel>>> GetAttendanceRequestAsync();
    Task<ApiResult<AttendanceRequestViewModel>> GetAttendanceRequestByIdAsync(int id);
    Task<ApiResult<AttendanceRequestViewModel>> CreateAttendanceRequestAsync(AttendanceRequestViewModel model);
    Task<ApiResult<AttendanceRequestViewModel>> UpdateAttendanceRequestAsync(int id);
}

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly ApiClients.AttendanceRequestApi _attendanceRequest;

    public AttendanceRequestService(ApiClients api)
    {
        _attendanceRequest = api.AttendanceRequest;
    }

    public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAttendanceRequestAsync()
        => _attendanceRequest.GetAll();

    public Task<ApiResult<AttendanceRequestViewModel>> GetAttendanceRequestByIdAsync(int id)
        => _attendanceRequest.GetById(id);

    public Task<ApiResult<AttendanceRequestViewModel>> CreateAttendanceRequestAsync(AttendanceRequestViewModel model)
        => _attendanceRequest.Create(model);

    public Task<ApiResult<AttendanceRequestViewModel>> UpdateAttendanceRequestAsync(int id)
        => _attendanceRequest.ApproveRequest(id);  
}

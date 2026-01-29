using arna.HRMS.ClientServices.Http;
using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels.Attendance;
using Microsoft.JSInterop;

namespace arna.HRMS.ClientServices.Client.Clock;

public interface IClockService
{
    Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model);
    Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId);
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}

public class ClockService : IClockService
{
    private readonly ApiClients.AttendanceApi _attendance;
    private readonly IGeolocationService _geo;
    private TaskCompletionSource<(double, double)?>? _tcs;

    public ClockService(ApiClients api, IGeolocationService geo)
    {
        _attendance = api.Attendance;
        _geo = geo;
    }
 
    public Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model)
        => _attendance.Create(model);

    public async Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId)
        => await _attendance.GetClockStatus(employeeId);

    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        _tcs = new TaskCompletionSource<(double, double)?>();

        await _geo.GetCurrentPositionAsync(
            this,
            nameof(OnSuccess),
            nameof(OnError),
            new PositionOptions
            {
                EnableHighAccuracy = true,
                Timeout = 10000,
                MaximumAge = 0
            }
        );

        return await _tcs.Task;
    }

    [JSInvokable]
    public void OnSuccess(GeolocationPosition position)
    {
        _tcs?.TrySetResult(
            (position.Coords.Latitude, position.Coords.Longitude)
        );
    }

    [JSInvokable]
    public void OnError(GeolocationPositionError error)
    {
        _tcs?.TrySetResult(null);
    }

}

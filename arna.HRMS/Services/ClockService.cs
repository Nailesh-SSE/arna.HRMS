using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Services.Http;
using Microsoft.JSInterop;

namespace arna.HRMS.Services;

public interface IClockService
{
    Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model);
    Task<ApiResult<AttendanceViewModel>> GetClockStatusAsync(int employeeId);
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}

public class ClockService : IClockService
{
    private readonly ApiClients.AttendanceApi _attendance;
    private readonly IGeolocationService _geo;

    private TaskCompletionSource<(double Latitude, double Longitude)?>? _tcs;

    public ClockService(ApiClients api, IGeolocationService geo)
    {
        _attendance = api.Attendance;
        _geo = geo;
    }

    public async Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model)
    {
        return await _attendance.CreateAsync(model);
    }

    public async Task<ApiResult<AttendanceViewModel>> GetClockStatusAsync(int employeeId)
    {
        return await _attendance.GetClockStatusAsync(employeeId);
    }

    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        _tcs = new TaskCompletionSource<(double Latitude, double Longitude)?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

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
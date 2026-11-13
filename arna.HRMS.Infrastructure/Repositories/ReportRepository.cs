using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class ReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceReportDto>> GetDailyAttendanceReportAsync(
        int? year,
        int? month,
        int? employeeId,
        AttendanceStatus? statusId,
        DeviceType? device,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Year", (object?)year ?? DBNull.Value),
            new SqlParameter("@Month", (object?)month ?? DBNull.Value),
            new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value),
            new SqlParameter("@StatusId", (object?)statusId ?? DBNull.Value),
            new SqlParameter("@DepartmentId", DBNull.Value), // optional
            new SqlParameter("@DeviceId", (object?)device ?? DBNull.Value),
            new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
            new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value)
        };

        var result = await _context
            .Set<AttendanceReportDto>()
            .FromSqlRaw(
                @"EXEC dbo.Sp_DailyAttendanceReport 
                    @Year, 
                    @Month, 
                    @EmployeeId, 
                    @StatusId, 
                    @DepartmentId, 
                    @DeviceId, 
                    @FromDate, 
                    @ToDate",
                parameters.ToArray())
            .ToListAsync();

        await EnrichProfileImagesAsync(result);
        return result;
    }

    public async Task<List<EmployeeAttendanceReportDto>> GetEmployeeAttendanceReportAsync(
        int? year,
        int? month,
        int? employeeId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var parameters = new[]
        {
            new SqlParameter("@Year", (object?)year ?? DBNull.Value),
            new SqlParameter("@Month", (object?)month ?? DBNull.Value),
            new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value),
            new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
            new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value)
        };

        var result = await _context
            .Set<EmployeeAttendanceReportDto>()
            .FromSqlRaw(
                "EXEC dbo.Sp_EmployeesAttendanceReport @Year, @Month, @EmployeeId, @FromDate, @ToDate",
                parameters)
            .ToListAsync();

        await EnrichProfileImagesAsync(result);
        return result;
    }

    public async Task<List<LeaveSummaryReportDto>> GetLeaveSummaryReportAsync(
        int? year,
        int? month,
        DateTime? fromDate, 
        DateTime? toDate,
        int? departmentId)
    {
        var parameters = new[]
        {
            new SqlParameter("@Year", (object?)year ?? DBNull.Value),
            new SqlParameter("@Month", (object?)month ?? DBNull.Value),
             new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
            new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value),
            new SqlParameter("@DepartmentId", (object?)departmentId ?? DBNull.Value)
        };
        var result = await _context
            .Set<LeaveSummaryReportDto>()
            .FromSqlRaw(
                @"EXEC dbo.Sp_LeaveSummaryReport 
                    @Year,
                    @Month,
                    @FromDate,
                    @ToDate,
                    @DepartmentId",
                parameters)
            .ToListAsync();
        await EnrichProfileImagesAsync(result);
        return result;
    }
    public async Task<List<EmployeeLeaveDetailsReportDto>> GetEmployeeLeaveDetailsReportAsync(
        int? year,
        int? month,
        DateTime? fromDate,
        DateTime? toDate,
        int? employeeId,
        string? employeeNumber)
    {
        var parameters = new[]
        {
            new SqlParameter("@Year", (object?)year ?? DBNull.Value),
            new SqlParameter("@Month", (object?)month ?? DBNull.Value),
             new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
            new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value),
            new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value),
            new SqlParameter("@EmployeeNumber", (object?)employeeNumber ?? DBNull.Value)
        };
        var result = await _context
            .Set<EmployeeLeaveDetailsReportDto>()
            .FromSqlRaw(
                @"EXEC dbo.Sp_EmployeeLeaveDetailsReport 
                    @Year,
                    @Month,
                    @FromDate,
                    @ToDate,
                    @EmployeeId,
                    @EmployeeNumber",
                parameters)
            .ToListAsync();
        await EnrichProfileImagesAsync(result);
        return result;
    }

    private async Task<Dictionary<int, string?>> GetEmployeeImagesMapAsync()
    {
        var rows = await _context.Employees
            .AsNoTracking()
            .Select(e => new { e.Id, e.ProfileImage })
            .ToListAsync();
        return rows.ToDictionary(e => e.Id, e => e.ProfileImage);
    }

    private async Task EnrichProfileImagesAsync(IEnumerable<AttendanceReportDto> list)
    {
        if (!list.Any()) return;
        var images = await GetEmployeeImagesMapAsync();
        foreach (var item in list)
        {
            if (images.TryGetValue(item.EmployeeId, out var img))
                item.ProfileImage = img;
        }
    }

    private async Task EnrichProfileImagesAsync(IEnumerable<EmployeeAttendanceReportDto> list)
    {
        if (!list.Any()) return;
        var images = await GetEmployeeImagesMapAsync();
        foreach (var item in list)
        {
            if (images.TryGetValue(item.EmployeeId, out var img))
                item.ProfileImage = img;
        }
    }

    private async Task EnrichProfileImagesAsync(IEnumerable<LeaveSummaryReportDto> list)
    {
        if (!list.Any()) return;
        var images = await GetEmployeeImagesMapAsync();
        foreach (var item in list)
        {
            if (item.EmployeeId.HasValue && images.TryGetValue(item.EmployeeId.Value, out var img))
                item.ProfileImage = img;
        }
    }

    private async Task EnrichProfileImagesAsync(IEnumerable<EmployeeLeaveDetailsReportDto> list)
    {
        if (!list.Any()) return;
        var images = await GetEmployeeImagesMapAsync();
        foreach (var item in list)
        {
            if (images.TryGetValue(item.EmployeeId, out var img))
                item.ProfileImage = img;
        }
    }
}

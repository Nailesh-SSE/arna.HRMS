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


    public async Task<List<AttendanceReportDto>> GetAttendanceReportAsync(
    int? year,
    int? month,
    int? employeeId,
    AttendanceStatus? statusId,
    DeviceType? device)
    {
        var parameters = new[]
        {
        new SqlParameter("@Year", (object?)year ?? DBNull.Value),
        new SqlParameter("@Month", (object?)month ?? DBNull.Value),
        new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value),
        new SqlParameter("@StatusId", (object?)statusId ?? DBNull.Value),
        new SqlParameter("@DepartmentId", DBNull.Value), // if needed later
        new SqlParameter("@DeviceId", (object?)device ?? DBNull.Value)
    };

        return await _context
            .Set<AttendanceReportDto>()
            .FromSqlRaw(
                "EXEC dbo.Sp_AttendanceReport @Year, @Month, @EmployeeId, @StatusId, @DepartmentId, @DeviceId",
                parameters)
            .ToListAsync();
    }
}

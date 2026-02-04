using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Helper.Attendance;

public static class AttendanceSummaryBuilder
{
    public static List<AttendenceSummaryCard> Build(
        List<MonthlyAttendanceViewModel> attendance)
    {
        if (attendance == null || !attendance.Any())
            return new();

        var employees = attendance
            .SelectMany(d => d.Employees)
            .Where(e => !string.IsNullOrWhiteSpace(e.Status))
            .ToList();

        return new List<AttendenceSummaryCard>
        {
            Create(employees, "Present",  "text-success"),
            Create(employees, "Holiday",  "text-primary"),
            Create(employees, "Leave",    "text-warning"),
            Create(employees, "Late",     "text-info"),
            Create(employees, "Absent",   "text-danger"),
            Create(employees, "Half-Day", "text-secondary")
        };
    }

    private static AttendenceSummaryCard Create(
        List<EmployeeDailyAttendanceViewModel> employees,
        string status,
        string css)
    {
        return new AttendenceSummaryCard
        {
            Label = status,
            Count = employees.Count(e => e.Status == status),
            Text = css
        };
    }
}

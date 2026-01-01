using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Attendance;

public static class AttendanceSummaryBuilder
{
    public static List<AttendenceSummaryCard> Build(
        List<MonthlyAttendanceDto> attendance)
    {
        if (attendance == null || !attendance.Any())
            return new List<AttendenceSummaryCard>();

        return new List<AttendenceSummaryCard>
        {
            Create(attendance, "Present",  "text-success"),
            Create(attendance, "Holiday",  "text-primary"),
            Create(attendance, "Leave",    "text-warning"),
            Create(attendance, "Late",     "text-info"),
            Create(attendance, "Absent",   "text-danger"),
            Create(attendance, "Half-Day",      "text-secondary")
        };
    }

    private static AttendenceSummaryCard Create(
        List<MonthlyAttendanceDto> attendance,
        string status,
        string css)
    {
        return new AttendenceSummaryCard
        {
            Label = status,
            Count = attendance.Count(x => x.Status.ToString() == status),
            Text = css
        };
    }
}

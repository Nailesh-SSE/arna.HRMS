using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Helper.Attendance;

public static class AttendanceSummaryBuilder
{
    public static List<AttendenceSummaryCard> Build(
        List<MonthlyAttendanceViewModel> attendance)
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
        List<MonthlyAttendanceViewModel> attendance,
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

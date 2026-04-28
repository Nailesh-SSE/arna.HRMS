using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Helper.Attendance;

public static class AttendanceSummaryBuilder
{
    // ================= ATTENDANCE SUMMARY =================
    public static List<AttendenceSummaryCard> Build(List<MonthlyAttendanceViewModel> attendance)
    {
        if (attendance == null || !attendance.Any())
            return new();
        // Aggregate counts in a single pass to avoid multiple iterations over the same collection
        var statusCounts = attendance
            .SelectMany(d => d.Employees)
            .Where(e => !string.IsNullOrWhiteSpace(e.Status))
            .Select(e => e.Status!.Trim())
            .ToLookup(s => s, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return new List<AttendenceSummaryCard>
        {
            Create(statusCounts, "Present",  "text-success"),
            Create(statusCounts, "Holiday",  "text-primary"),
            Create(statusCounts, "Leave",    "text-warning"),
            Create(statusCounts, "Late",     "text-info"),
            Create(statusCounts, "Absent",   "text-danger"),
            Create(statusCounts, "Half-Day", "text-secondary")
        };
    }

    // ================= REQUEST SUMMARY =================
    public static List<AttendenceSummaryCard> Build(List<AttendanceRequestViewModel> requests)
    {
        if (requests == null)
            return new();
        // Count requests by Status (single pass) to avoid repeated Enum name lookups
        var countsByStatus = requests
            .Where(r => r != null)
            .GroupBy(r => r.StatusId)
            .ToDictionary(g => g.Key, g => g.Count());

        int getCountFor(Status s) => countsByStatus.TryGetValue(s, out var c) ? c : 0;

        return new()
        {
            new AttendenceSummaryCard
            {
                Label = Status.Pending.ToString(),
                Count = getCountFor(Status.Pending),
                Text = "text-warning"
            },
            new AttendenceSummaryCard
            {
                Label = Status.Approved.ToString(),
                Count = getCountFor(Status.Approved),
                Text = "text-success"
            },
            new AttendenceSummaryCard
            {
                Label = Status.Rejected.ToString(),
                Count = getCountFor(Status.Rejected),
                Text = "text-danger"
            },
            new AttendenceSummaryCard
            {
                Label = Status.Cancelled.ToString(),
                Count = getCountFor(Status.Cancelled),
                Text = "text-secondary"
            }
        };
    }

    // ================= ATTENDANCE =================
    // Helper: build card from status counts dictionary (case-insensitive keys)
    private static AttendenceSummaryCard Create(Dictionary<string, int> statusCounts, string status, string css)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(status) && statusCounts.TryGetValue(status.Trim(), out var c))
            count = c;

        return new AttendenceSummaryCard
        {
            Label = status,
            Count = count,
            Text = css
        };
    }
}

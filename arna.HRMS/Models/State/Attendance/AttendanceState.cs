using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.Models.State.Attendance;

public class AttendanceState
{
    /* ================= VIEW ================= */
    public const string TABLE = "table";
    public const string CALENDAR = "calendar";

    public string ViewMode { get; set; } = TABLE;

    /* ================= MONTH ================= */
    public string SelectedMonth { get; set; } = DateTime.Now.ToString("yyyy-MM");
    public int Year { get; set; }
    public int Month { get; set; }

    /* ================= DATA ================= */
    public List<MonthlyAttendanceDto> AttendanceList { get; set; } = new();
    public Dictionary<DateOnly, MonthlyAttendanceDto> AttendanceLookup { get; set; } = new();
    public List<AttendenceSummaryCard> SummaryCards { get; set; } = new();

    public TimeSpan TotalWorkingHours { get; set; }

    /* ================= HELPERS ================= */
    public void ParseMonth()
    {
        if (string.IsNullOrWhiteSpace(SelectedMonth))
            return;

        var parts = SelectedMonth.Split('-');
        if (parts.Length != 2)
            return;

        if (!int.TryParse(parts[0], out var year))
            return;

        if (!int.TryParse(parts[1], out var month))
            return;

        // 🔥 HARD CLAMP
        if (month < 1 || month > 12)
            return;

        Year = year;
        Month = month;
    }
}

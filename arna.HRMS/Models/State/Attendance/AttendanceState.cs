using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.Models.State.Attendance;

public class AttendanceState
{
    public const string TABLE = "table";
    public const string CALENDAR = "calendar";
    public const string REQUEST = "requests"; 

    public string ViewMode { get; set; } = TABLE;

    /* ================= MONTH ================= */
    public string SelectedMonth { get; set; } = DateTime.Now.ToString("yyyy-MM");
    public int Year { get; set; }
    public int Month { get; set; }

    /* ================= ADMIN ================= */
    public DateTime? SelectedDate { get; set; }
    public int? SelectedEmployeeId { get; set; }
    public string? SelectedEmployeeName { get; set; } = string.Empty;
    public Status? SelectedStatus { get; set; }

    /* ================= DATA (GROUPED) ================= */
    public List<MonthlyAttendanceViewModel> AttendanceList { get; set; } = new();

    public Dictionary<DateOnly, MonthlyAttendanceViewModel> AttendanceLookup { get; set; } = new();

    public List<AttendenceSummaryCard> SummaryCards { get; set; } = new();

    public TimeSpan TotalWorkingHours { get; set; }

    public List<AttendanceRequestViewModel> AttendanceRequestList { get; set; } = new(); 

    /* ================= HELPERS ================= */
    public void ParseMonth()
    {
        if (string.IsNullOrWhiteSpace(SelectedMonth))
            return;

        var parts = SelectedMonth.Split('-');
        if (parts.Length != 2)
            return;

        if (int.TryParse(parts[0], out var y) &&
            int.TryParse(parts[1], out var m))
        {
            Year = y;
            Month = m;
        }
    }
}

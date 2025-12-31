using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.ViewModels;

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

    public double TotalWorkingHours { get; set; }

    /* ================= HELPERS ================= */
    public void ParseMonth()
    {
        var p = SelectedMonth.Split('-');
        Year = int.Parse(p[0]);
        Month = int.Parse(p[1]);
    }
}

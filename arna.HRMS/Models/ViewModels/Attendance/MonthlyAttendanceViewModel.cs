namespace arna.HRMS.Models.ViewModels.Attendance;

public class MonthlyAttendanceViewModel 
{
    public DateOnly Date { get; set; }
    public string Day { get; set; } = string.Empty;
    public List<EmployeeDailyAttendanceViewModel> Employees { get; set; } = new();
}

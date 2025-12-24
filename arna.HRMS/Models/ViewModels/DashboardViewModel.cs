namespace arna.HRMS.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }


    public int TotalLeaveRequests { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeaveRequests { get; set; }
    public int RejectedLeaveRequests { get; set; }


    public int TodayPresentEmployees { get; set; }
    public int TodayAbsentEmployees { get; set; }
    public int TodayOnLeaveEmployees { get; set; }
}

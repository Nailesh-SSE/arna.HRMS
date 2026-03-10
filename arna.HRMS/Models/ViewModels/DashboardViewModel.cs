using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Models.ViewModels;

public class DashboardViewModel
{
    public List<EmployeeViewModel> Employees { get; set; } = new();
    public List<LeaveTypeViewModel> LeaveTypes { get; set; } = new();
    public List<LeaveRequestViewModel> Requests { get; set; } = new();

    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancledRequests { get; set; }
    public int TotalEmployees { get; set; }
}

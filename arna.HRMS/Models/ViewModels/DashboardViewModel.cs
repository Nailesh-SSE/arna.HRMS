using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Models.ViewModels;

public class DashboardViewModel
{
    public List<EmployeeViewModel> Employees { get; set; } = new();
    public List<LeaveTypeViewModel> LeaveTypes { get; set; } = new();
    public List<LeaveRequestViewModel> Requests { get; set; } = new();

    public int TotalRequests => Requests.Count;
    public int PendingRequests => Requests.Count(r => r.StatusId == Status.Pending);
    public int ApprovedRequests => Requests.Count(r => r.StatusId == Status.Approved);
    public int RejectedRequests => Requests.Count(r => r.StatusId == Status.Rejected);
    public int CancledRequests => Requests.Count(r => r.StatusId == Status.Cancelled);
    public int TotalEmployees => Employees.Count;
}

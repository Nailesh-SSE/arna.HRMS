using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class DashboardDto
{
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<LeaveTypeDto> LeaveTypes { get; set; } = new();
    public List<LeaveRequestDto> Requests { get; set; } = new();

    public int TotalRequests => Requests.Count;
    public int PendingRequests => Requests.Count(r => r.StatusId == Status.Pending);
    public int ApprovedRequests => Requests.Count(r => r.StatusId == Status.Approved);
    public int RejectedRequests => Requests.Count(r => r.StatusId == Status.Rejected);
    public int CancledRequests => Requests.Count(r => r.StatusId == Status.Cancelled);
    public int TotalEmployees => Employees.Count;
}

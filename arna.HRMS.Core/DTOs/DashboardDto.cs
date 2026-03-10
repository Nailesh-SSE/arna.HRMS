namespace arna.HRMS.Core.DTOs;

public class DashboardDto
{
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<LeaveTypeDto> LeaveTypes { get; set; } = new();
    public List<LeaveRequestDto> Requests { get; set; } = new();

    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancledRequests { get; set; }
    public int TotalEmployees { get; set; }
}
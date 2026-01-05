using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.DTOs;

public class TimesheetDto : CommonDto
{
    public int EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public double HoursWorked { get; set; }
    public string? TaskDescription { get; set; }
    public bool IsApproved { get; set; }
}

using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.DTOs;

public class TimesheetDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public double HoursWorked { get; set; }
    public string? TaskDescription { get; set; }
    public bool IsApproved { get; set; }
}

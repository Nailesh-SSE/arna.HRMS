namespace arna.HRMS.Core.Entities;

public class Timesheet : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    //public TimesheetStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Notes { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; }
    public Employee ApprovedByEmployee { get; set; }
    //public ICollection<TimesheetEntry> Entries { get; set; }
}

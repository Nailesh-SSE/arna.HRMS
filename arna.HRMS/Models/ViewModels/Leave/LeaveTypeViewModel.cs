using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Leave;

public class LeaveTypeViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Leave Name is required")]
    public LeaveName LeaveNameId { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "Leave days is required")]
    
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

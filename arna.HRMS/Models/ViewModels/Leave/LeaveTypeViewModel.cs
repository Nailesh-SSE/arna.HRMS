using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Leave;

public class LeaveTypeViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Leave type is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a leave type.")]
    public LeaveName LeaveNameId { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Leave days is required.")]
    [Range(1, 365, ErrorMessage = "Leave days must be between 1 and 365.")]
    public int MaxPerYear { get; set; }

    public bool IsPaid { get; set; } = true;
}

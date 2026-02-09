using arna.HRMS.Core.Common;
using arna.HRMS.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs;

public class LeaveTypeDto : BaseEntity
{
    [Required(ErrorMessage ="Leave Name is required")]
    public LeaveName LeaveNameId { get; set; }
    public string? Description { get; set; }
    [Required(ErrorMessage = "Leave days is required")]
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

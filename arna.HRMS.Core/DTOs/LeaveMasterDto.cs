using arna.HRMS.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs;

public class LeaveMasterDto : BaseEntity
{
    [Required(ErrorMessage ="Leave Name is required")]
    public string LeaveName { get; set; }
    public string? Description { get; set; }
    [Required(ErrorMessage = "Leave days is required")]
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

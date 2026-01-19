using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class LeaveMasterDto : CommonDto
{
    [Required(ErrorMessage ="Leave Name is required")]
    public string LeaveName { get; set; }
    public string? Description { get; set; }
    [Required(ErrorMessage = "Leave days is required")]
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Leave;

public class LeaveMasterViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Leave Name is required")]
    public string LeaveName { get; set; }
    public string? Description { get; set; }
    [Required(ErrorMessage = "Leave days is required")] 
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

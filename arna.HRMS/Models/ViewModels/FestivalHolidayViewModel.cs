using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class FestivalHolidayViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Date is required.")]
    public DateTime Date { get; set; }

    public string Days => Date.DayOfWeek.ToString();

    [Required(ErrorMessage = "Festival name is required.")]
    [StringLength(100, ErrorMessage = "Festival name cannot exceed 100 characters.")]
    public string FestivalName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}

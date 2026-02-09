using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class FestivalHolidayViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    public string Days => Date.DayOfWeek.ToString();
    [Required(ErrorMessage = "Festival name is required")]
    public string FestivalName { get; set; }
    public string? Description { get; set; }
}

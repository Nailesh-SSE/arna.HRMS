using arna.HRMS.Core.Common.Base;

namespace arna.HRMS.Core.Entities;

public class FestivalHoliday : BaseEntity
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; }
    public string FestivalName { get; set; }
    public string? Description { get; set; }
    
}

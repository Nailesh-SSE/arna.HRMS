using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class FestivalHoliday : BaseEntity
{
    public DateTime Date { get; set; }
    public string FestivalName { get; set; }
    public string? Description { get; set; }
    
}

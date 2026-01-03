namespace arna.HRMS.Models.Common;

public class CommonDto
{
    public int Id { get; set; }
    public DateTime CreatedBy { get; set; } = DateTime.Now;
    public DateTime UpdatedBy { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}

namespace arna.HRMS.Models.Common;

public class CommonDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}

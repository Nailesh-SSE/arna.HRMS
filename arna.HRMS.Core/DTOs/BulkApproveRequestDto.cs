namespace arna.HRMS.Core.DTOs;

public class BulkApproveRequestDto
{
    public List<int> RequestIds { get; set; } = new();
    public int ApprovedBy { get; set; }
}


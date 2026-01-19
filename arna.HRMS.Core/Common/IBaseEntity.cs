namespace arna.HRMS.Core.Common;

public interface IBaseEntity
{
    int Id { get; set; }
    DateTime CreatedBy { get; set; }
    DateTime UpdatedBy { get; set; }
    bool IsDeleted { get; set; }
}

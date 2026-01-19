namespace arna.HRMS.Core.Common;

public interface IBaseEntity
{
    int Id { get; set; }
    DateTime CreatedOn { get; set; }
    DateTime UpdatedOn { get; set; }
    bool IsDeleted { get; set; }
}

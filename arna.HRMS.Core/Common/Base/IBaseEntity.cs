namespace arna.HRMS.Core.Common.Base;

public interface IBaseEntity
{
    int Id { get; set; }
    DateTime CreatedOn { get; set; }
    DateTime UpdatedOn { get; set; }
    bool IsDeleted { get; set; }
}

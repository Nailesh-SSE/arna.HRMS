namespace arna.HRMS.Core.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }

    // Navigation Properties
    public Department ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; }
    public ICollection<Employee> Employees { get; set; }
}

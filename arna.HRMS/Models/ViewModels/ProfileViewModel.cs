namespace arna.HRMS.Models.ViewModels;

public class ProfileViewModel
{
    public bool IsEmployee { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Position { get; set; }
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
    public string EmployeeNumber { get; set; }
    public string ManagerFullName { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; }

    public ProfileViewModel(EmployeeViewModel e)
    {
        IsEmployee = true;
        FirstName = e.FirstName; 
        LastName = e.LastName; 
        Email = e.Email;
        PhoneNumber = e.PhoneNumber; 
        Position = e.Position; 
        EmployeeNumber = e.EmployeeNumber ?? string.Empty;
        ManagerFullName = e.ManagerFullName ?? string.Empty;
        HireDate = e.HireDate; 
        CreatedOn = e.CreatedOn;
        IsActive = e.IsActive;
    }

    public ProfileViewModel(dynamic u)
    {
        IsEmployee = false;
        FirstName = u.FirstName;
        LastName = u.LastName;
        Email = u.Email;
        PhoneNumber = u.PhoneNumber;
        CreatedOn = u.CreatedOn;
    }
}

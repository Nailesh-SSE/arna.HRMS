namespace arna.HRMS.Models.ViewModels;

public class ProfileViewModel
{
    public bool IsEmployee { get; set; }
    public int Id { get; set; }
    public int? EmployeeId { get; set; }
    public int? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string OfficeEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Position { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string ManagerFullName { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal Salary { get; set; }

    public ProfileViewModel() { }

    public ProfileViewModel(EmployeeViewModel e)
    {
        IsEmployee = true;
        Id = e.Id;
        EmployeeId = e.Id;
        FirstName = e.FirstName ?? string.Empty; 
        LastName = e.LastName ?? string.Empty; 
        Email = e.Email ?? string.Empty;
        OfficeEmail = e.OfficeEmail ?? string.Empty;
        PhoneNumber = e.PhoneNumber ?? string.Empty; 
        DateOfBirth = e.DateOfBirth == default ? null : e.DateOfBirth;
        Position = e.Position ?? string.Empty; 
        DepartmentId = e.DepartmentId;
        DepartmentName = e.DepartmentName ?? string.Empty;
        DepartmentCode = e.DepartmentCode ?? string.Empty;
        EmployeeNumber = e.EmployeeNumber ?? string.Empty;
        ManagerFullName = e.ManagerFullName ?? string.Empty;
        HireDate = e.HireDate == default ? null : e.HireDate; 
        CreatedOn = e.CreatedOn;
        IsActive = e.IsActive;
        Salary = e.Salary;
    }

    public ProfileViewModel(UserViewModel u)
    {
        IsEmployee = false;
        Id = u.Id;
        UserId = u.Id;
        Username = u.Username ?? string.Empty;
        FirstName = u.FirstName ?? string.Empty;
        LastName = u.LastName ?? string.Empty;
        Email = u.Email ?? string.Empty;
        PhoneNumber = u.PhoneNumber ?? string.Empty;
        Role = u.Role ?? string.Empty;
        CreatedOn = u.CreatedOn;
        IsActive = u.IsActive;
    }

    public ProfileViewModel(dynamic u)
    {
        IsEmployee = false;
        try { Id = u.Id; } catch { }
        try { Username = u.Username ?? string.Empty; } catch { }
        try { FirstName = u.FirstName ?? string.Empty; } catch { }
        try { LastName = u.LastName ?? string.Empty; } catch { }
        try { Email = u.Email ?? string.Empty; } catch { }
        try { PhoneNumber = u.PhoneNumber ?? string.Empty; } catch { }
        try { Role = u.Role ?? string.Empty; } catch { }
        try { CreatedOn = u.CreatedOn; } catch { }
        try { IsActive = u.IsActive; } catch { }
    }
}

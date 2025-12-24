using arna.HRMS.Core.Entities;
using Microsoft.AspNetCore.Identity;


namespace arna.HRMS.Infrastructure.Data.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? HireDate { get; set; }
    public int? EmployeeId { get; set; }

    public Employee Employee { get; set; }
}

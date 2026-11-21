using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Models.ViewModels.Leave;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Tests.Unit.ViewModels;

[TestFixture]
public class ViewModelValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #region EmployeeViewModel Tests

    [Test]
    public void EmployeeViewModel_WhenValid_PassesValidation()
    {
        var model = new EmployeeViewModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@personal.com",
            OfficeEmail = "john.doe@company.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1995, 5, 20),
            HireDate = new DateTime(2023, 1, 10),
            DepartmentId = 1,
            Designation = EmployeePosition.SoftwareDeveloper,
            Salary = 65000m
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void EmployeeViewModel_WhenRequiredFieldsMissing_FailsValidation()
    {
        var model = new EmployeeViewModel();

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "First name is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Last name is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Personal email is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Office email is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Phone number is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Please select a department."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Designation is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Salary must be greater than 0."), Is.True);
    }

    [TestCase("invalid-email")]
    [TestCase("user@")]
    [TestCase("@domain.com")]
    [TestCase("user@domain")]
    [TestCase("user@.com")]
    public void EmployeeViewModel_WhenEmailInvalid_FailsWithInvalidEmailMessage(string invalidEmail)
    {
        var model = new EmployeeViewModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = invalidEmail,
            OfficeEmail = "john.doe@company.com",
            PhoneNumber = "1234567890",
            DateOfBirth = new DateTime(1995, 5, 20),
            HireDate = new DateTime(2023, 1, 10),
            DepartmentId = 1,
            Designation = EmployeePosition.SoftwareDeveloper,
            Salary = 50000m
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Please enter a valid email address."), Is.True);
    }

    [TestCase("abc")]
    [TestCase("1234")]
    [TestCase("phone12345")]
    [TestCase("10293847")]
    public void EmployeeViewModel_WhenPhoneInvalid_FailsWithInvalidPhoneMessage(string invalidPhone)
    {
        var model = new EmployeeViewModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@personal.com",
            OfficeEmail = "john.doe@company.com",
            PhoneNumber = invalidPhone,
            DateOfBirth = new DateTime(1995, 5, 20),
            HireDate = new DateTime(2023, 1, 10),
            DepartmentId = 1,
            Designation = EmployeePosition.SoftwareDeveloper,
            Salary = 50000m
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Phone number must be exactly 10 digits."), Is.True);
    }

    [TestCase(0)]
    [TestCase(-100)]
    public void EmployeeViewModel_WhenSalaryZeroOrNegative_FailsWithSalaryRangeMessage(decimal invalidSalary)
    {
        var model = new EmployeeViewModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@personal.com",
            OfficeEmail = "john.doe@company.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateTime(1995, 5, 20),
            HireDate = new DateTime(2023, 1, 10),
            DepartmentId = 1,
            Designation = EmployeePosition.SoftwareDeveloper,
            Salary = invalidSalary
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Salary must be greater than 0."), Is.True);
    }

    #endregion

    #region UserViewModel Tests

    [Test]
    public void UserViewModel_WhenValid_PassesValidation()
    {
        var model = new UserViewModel
        {
            Username = "john.doe",
            Email = "john.doe@company.com",
            FirstName = "John",
            LastName = "Doe",
            RoleId = 2,
            PhoneNumber = "+1234567890",
            Password = "Password123!"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void UserViewModel_WhenUsernameTooShort_FailsValidation()
    {
        var model = new UserViewModel
        {
            Username = "ab",
            Email = "john.doe@company.com",
            FirstName = "John",
            LastName = "Doe",
            RoleId = 2,
            PhoneNumber = "+1234567890"
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Username must be at least 3 characters."), Is.True);
    }

    [TestCase("invalid-email")]
    [TestCase("test@")]
    public void UserViewModel_WhenEmailInvalid_FailsValidation(string invalidEmail)
    {
        var model = new UserViewModel
        {
            Username = "john.doe",
            Email = invalidEmail,
            FirstName = "John",
            LastName = "Doe",
            RoleId = 2,
            PhoneNumber = "+1234567890"
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Please enter a valid email address."), Is.True);
    }

    [TestCase("123")]
    [TestCase("not-a-phone")]
    [TestCase("10293847")]
    public void UserViewModel_WhenPhoneInvalid_FailsValidation(string invalidPhone)
    {
        var model = new UserViewModel
        {
            Username = "john.doe",
            Email = "john.doe@company.com",
            FirstName = "John",
            LastName = "Doe",
            RoleId = 2,
            PhoneNumber = invalidPhone
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Phone number must be exactly 10 digits."), Is.True);
    }

    #endregion

    #region DepartmentViewModel Tests

    [Test]
    public void DepartmentViewModel_WhenValid_PassesValidation()
    {
        var model = new DepartmentViewModel
        {
            Name = "Engineering",
            Code = "ENG-01",
            Description = "Engineering Department"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void DepartmentViewModel_WhenNameOrCodeMissing_FailsValidation()
    {
        var model = new DepartmentViewModel();

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Department name is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Department code is required."), Is.True);
    }

    #endregion

    #region RoleViewModel Tests

    [Test]
    public void RoleViewModel_WhenValid_PassesValidation()
    {
        var model = new RoleViewModel
        {
            Name = "HR Manager",
            Description = "Human Resources Manager"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void RoleViewModel_WhenNameMissing_FailsValidation()
    {
        var model = new RoleViewModel();

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Role name is required."), Is.True);
    }

    #endregion

    #region FestivalHolidayViewModel Tests

    [Test]
    public void FestivalHolidayViewModel_WhenValid_PassesValidation()
    {
        var model = new FestivalHolidayViewModel
        {
            FestivalName = "New Year",
            Date = new DateTime(2026, 1, 1),
            Description = "New Year celebration"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void FestivalHolidayViewModel_WhenFestivalNameMissing_FailsValidation()
    {
        var model = new FestivalHolidayViewModel
        {
            Date = new DateTime(2026, 1, 1)
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Festival name is required."), Is.True);
    }

    #endregion

    #region AttendanceRequestViewModel Tests

    [Test]
    public void AttendanceRequestViewModel_WhenValid_PassesValidation()
    {
        var model = new AttendanceRequestViewModel
        {
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            ReasonTypeId = AttendanceReasonType.WorkFromHome,
            LocationId = AttendanceLocation.Remote,
            ClockIn = DateTime.Today.AddHours(9),
            ClockOut = DateTime.Today.AddHours(18),
            BreakDuration = TimeSpan.FromHours(1),
            Description = "Working remotely today."
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void AttendanceRequestViewModel_WhenRequiredFieldsMissing_FailsValidation()
    {
        var model = new AttendanceRequestViewModel();

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "From date is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "To date is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Please select a reason."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Please select a location."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Please select your clock-in time."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Please select your clock-out time."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Break duration is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Message is required."), Is.True);
    }

    #endregion

    #region LeaveTypeViewModel Tests

    [Test]
    public void LeaveTypeViewModel_WhenValid_PassesValidation()
    {
        var model = new LeaveTypeViewModel
        {
            LeaveNameId = LeaveName.CasualLeave,
            MaxPerYear = 12,
            Description = "Casual leave policy"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void LeaveTypeViewModel_WhenMaxPerYearZeroOrNegative_FailsValidation()
    {
        var model = new LeaveTypeViewModel
        {
            LeaveNameId = LeaveName.CasualLeave,
            MaxPerYear = 0
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Leave days must be between 1 and 365."), Is.True);
    }

    #endregion

    #region LeaveRequestViewModel Tests

    [Test]
    public void LeaveRequestViewModel_WhenValid_PassesValidation()
    {
        var model = new LeaveRequestViewModel
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2),
            Reason = "Family event"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void LeaveRequestViewModel_WhenReasonMissing_FailsValidation()
    {
        var model = new LeaveRequestViewModel
        {
            LeaveTypeId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2)
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Reason is required."), Is.True);
    }

    #endregion

    #region LoginViewModel Tests

    [Test]
    public void LoginViewModel_WhenValid_PassesValidation()
    {
        var model = new LoginViewModel
        {
            UserName = "admin",
            Password = "SecurePassword123!"
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void LoginViewModel_WhenCredentialsMissing_FailsValidation()
    {
        var model = new LoginViewModel();

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Username is required."), Is.True);
        Assert.That(errors.Any(e => e.ErrorMessage == "Password is required."), Is.True);
    }

    #endregion

    #region AttendanceViewModel Tests

    [Test]
    public void AttendanceViewModel_WhenValid_PassesValidation()
    {
        var model = new AttendanceViewModel
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            Notes = "Standard clock-in session note."
        };

        var errors = ValidateModel(model);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void AttendanceViewModel_WhenNotesMissing_FailsValidation()
    {
        var model = new AttendanceViewModel
        {
            EmployeeId = 1,
            Date = DateTime.Today
        };

        var errors = ValidateModel(model);

        Assert.That(errors.Any(e => e.ErrorMessage == "Note is required."), Is.True);
    }

    #endregion
}

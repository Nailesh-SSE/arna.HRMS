namespace arna.HRMS.Core.Enums;

public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    HR = 3,
    Manager = 4,
    Employee = 5
}

public static class UserRoleGroups
{
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);
    public const string Admin = nameof(UserRole.Admin);
    public const string HR = nameof(UserRole.HR);
    public const string Manager = nameof(UserRole.Manager);
    public const string Employee = nameof(UserRole.Employee);

    public const string AdminRoles =
        SuperAdmin + "," +
        Admin + "," +
        HR + "," +
        Manager;
}
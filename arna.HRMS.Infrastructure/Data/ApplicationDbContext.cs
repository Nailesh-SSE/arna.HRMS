using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AttendanceRequest> AttendanceRequest { get; set; } 
        public DbSet<FestivalHoliday> FestivalHoliday { get; set; }
        public DbSet<LeaveMaster> LeaveMasters { get; set; }
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations
            builder.ApplyConfiguration(new EmployeeConfiguration());
            builder.ApplyConfiguration(new DepartmentConfiguration());
            builder.ApplyConfiguration(new AttendanceConfiguration());
            builder.ApplyConfiguration(new LeaveRequestConfiguration());
            builder.ApplyConfiguration(new TimesheetConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new AttendanceRequestConfiguration());
            builder.ApplyConfiguration(new FestivalHolidayConfiguration()); 
            builder.ApplyConfiguration(new LeaveMasterConfiguration());
            builder.ApplyConfiguration(new EmployeeLeaveBalanceConfiguration());

            // ===== Users =====
            builder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "SuperAdmin",
                    Email = "superadmin123@gmail.com",
                    PasswordHash = HashPassword("superadmin@123"),
                    FirstName = "Super",
                    LastName = "Admin",
                    PhoneNumber = "9999999999",
                    Role = UserRole.SuperAdmin,
                    RefreshToken = null,
                    RefreshTokenExpiryTime = null,
                    Password = "superadmin@123",
                    EmployeeId = null
                }
            );

            // ===== Departments =====
            builder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "Human Resources",
                    Code = "HR",
                    Description = "Handles recruitment, payroll, and employee relations",
                    ParentDepartmentId = 1
                },
                new Department
                {
                    Id = 2,
                    Name = "Information Technology",
                    Code = "IT",
                    Description = "Manages IT infrastructure and software systems",
                    ParentDepartmentId = 2
                },
                new Department
                {
                    Id = 3,
                    Name = "Finance",
                    Code = "FIN",
                    Description = "Responsible for accounting and financial management",
                    ParentDepartmentId = 3
                },
                new Department
                {
                    Id = 4,
                    Name = "Administration",
                    Code = "ADMIN",
                    Description = "Office administration and facilities management",
                    ParentDepartmentId = 4
                }
            );

            // ===== Roles =====
            builder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full permissions"
                },
                new Role
                {
                    Id = 2,
                    Name = "Admin",
                    Description = "Administrator with full permissions"
                },
                new Role
                {
                    Id = 3,
                    Name = "HR",
                    Description = "Human Resources role"
                },
                new Role
                {
                    Id = 4,
                    Name = "Manager",
                    Description = "Manager role with team oversight"
                },
                new Role
                {
                    Id = 5,
                    Name = "Employee",
                    Description = "Standard employee role"
                }
            );
        }

        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

}

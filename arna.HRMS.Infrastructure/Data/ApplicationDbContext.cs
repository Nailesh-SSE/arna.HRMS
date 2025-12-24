using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Data.Configurations;
using arna.HRMS.Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<User> Users { get; set; }

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

            // Seed Identity Roles
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator with full permissions",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 2,
                    Name = "HR",
                    NormalizedName = "HR",
                    Description = "Human Resources role",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 3,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "Manager role with team oversight",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 4,
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    Description = "Standard employee role",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );
        }

    }

}

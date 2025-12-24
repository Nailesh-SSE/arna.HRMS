using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TotalHours)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.OvertimeHours)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.RegularHours)   // <-- Add this
    .HasColumnType("decimal(18,2)");

        // Fixed: Use Notes instead of Comments
        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        // Relationship with Employee
        builder.HasOne(t => t.Employee)
            .WithMany(e => e.Timesheets)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Approver (Manager/Employee)
        builder.HasOne(t => t.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(t => t.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

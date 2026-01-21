using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.TotalDays)
            .IsRequired();

        builder.Property(lr => lr.Reason)
            .HasMaxLength(500);

        builder.Property(lr => lr.ApprovalNotes)
            .HasMaxLength(500);

        builder.Property(lr => lr.Status)
            .HasConversion<string>()      
            .HasMaxLength(20)
            .HasDefaultValue(LeaveStatus.Pending)
            .IsRequired();

        // Relationship with Employee (Requestor)
        builder.HasOne(lr => lr.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Approver (Employee)
        builder.HasOne(lr => lr.ApprovedByEmployee)
            .WithMany(e => e.ApprovedLeaveRequests)
            .HasForeignKey(lr => lr.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lr => lr.LeaveType)
            .WithMany()
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class EmployeeLeaveBalanceConfiguration : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> builder)
    {
        builder.HasKey(elb => elb.Id);

        builder.Property(elb => elb.TotalLeaves)
            .IsRequired();

        builder.Property(elb => elb.UsedLeaves)
            .IsRequired();

        builder.Property(elb => elb.RemainingLeaves)
            .IsRequired();
        builder.Property(elb => elb.Year)
            .IsRequired();

        builder.HasOne(elb => elb.Employee)
            .WithMany(e => e.EmployeeLeaveBalance)
            .HasForeignKey(elb => elb.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(elb => elb.LeaveMaster)
            .WithMany(lm => lm.EmployeeLeaveBalances)
            .HasForeignKey(elb => elb.LeaveMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(elb => new { elb.EmployeeId, elb.LeaveMasterId });

        builder.HasIndex(elb => elb.EmployeeId);
        builder.HasIndex(elb => elb.LeaveMasterId);
    }
}


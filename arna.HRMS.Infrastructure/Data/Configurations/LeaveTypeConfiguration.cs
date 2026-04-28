using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.HasKey(lm => lm.Id);

        builder.Property(lm => lm.LeaveNameId)
            .IsRequired();

        builder.HasIndex(lm => lm.LeaveNameId)
            .IsUnique();

        builder.Property(lm => lm.Description)
            .HasMaxLength(500);

        builder.HasMany(lm => lm.LeaveRequests)
            .WithOne(lr => lr.LeaveType)
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        
    }
}

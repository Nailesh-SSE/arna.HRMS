using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class LeaveMasterConfiguration : IEntityTypeConfiguration<LeaveMaster>
{
    public void Configure(EntityTypeBuilder<LeaveMaster> builder)
    {
        builder.HasKey(lm => lm.Id);

        builder.Property(lm => lm.LeaveName)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(lm => lm.LeaveName)
            .IsUnique();

        builder.Property(lm => lm.Description)
            .HasMaxLength(500);

        builder.HasMany(lm => lm.LeaveRequests)
            .WithOne(lr => lr.LeaveType)
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        
    }
}

using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class AttendanceRequestConfiguration : IEntityTypeConfiguration<AttendanceRequest>
{
    public void Configure(EntityTypeBuilder<AttendanceRequest> builder)
    {
        builder.HasKey(ae => ae.Id);

        builder.Property(ae => ae.Date)
            .IsRequired();

        builder.Property(ae => ae.ReasonType)
            .IsRequired();
        builder.Property(ae => ae.Location)
            .IsRequired();
        builder.Property(ae => ae.Description)
            .HasMaxLength(1000)
            .HasColumnType("varchar");
        builder.Property(ae => ae.ClockIn)
            .IsRequired(false);
            
        builder.Property(ae => ae.ClockOut)
            .IsRequired(false);
        builder.Property(ae => ae.BreakDuration)
            .HasColumnType("time");
        builder.Property(ae => ae.TotalHours)
            .HasColumnType("time");
        builder.Property(ae => ae.IsApproved)
            .HasDefaultValue(false);

        // Relationship with Employee
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRequest)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

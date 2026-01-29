using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.ClockIn)
            .IsRequired(false);

        builder.Property(a => a.ClockOut)
            .IsRequired(false);

        builder.Property(a => a.TotalHours)
            .HasColumnType("time");

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a=> a.Latitude)
            .IsRequired(false);

        builder.Property(a=> a.Longitude)
            .IsRequired(false);

        // Relationship with Employee
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

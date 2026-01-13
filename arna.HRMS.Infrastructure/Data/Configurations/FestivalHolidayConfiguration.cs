using arna.HRMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace arna.HRMS.Infrastructure.Data.Configurations;

public class FestivalHolidayConfiguration : IEntityTypeConfiguration<FestivalHoliday>
{
    public void Configure(EntityTypeBuilder<FestivalHoliday> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
               .IsRequired();

        builder.Property(x => x.FestivalName)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.FestivalName);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.CourseId);
        
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(c => c.Code)
            .IsUnique();
    }
}

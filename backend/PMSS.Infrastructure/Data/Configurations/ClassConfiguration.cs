using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(c => c.ClassId);
        
        builder.Property(c => c.ClassCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(c => new { c.SemesterId, c.CourseId, c.ClassCode })
            .IsUnique()
            .HasDatabaseName("unique_class_per_semester_course");
        
        builder.HasOne(c => c.Semester)
            .WithMany(s => s.Classes)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(c => c.Course)
            .WithMany(co => co.Classes)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(c => c.Teacher)
            .WithMany(u => u.TaughtClasses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

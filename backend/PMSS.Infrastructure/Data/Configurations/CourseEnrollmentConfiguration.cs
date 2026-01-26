using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class ClassEnrollmentConfiguration : IEntityTypeConfiguration<ClassEnrollment>
{
    public void Configure(EntityTypeBuilder<ClassEnrollment> builder)
    {
        builder.HasKey(ce => new { ce.ClassId, ce.UserId });
        
        builder.HasIndex(ce => new { ce.UserId, ce.CourseId })
            .IsUnique()
            .HasDatabaseName("no_duplicate_course_per_semester");
        
        builder.HasOne(ce => ce.Class)
            .WithMany(c => c.ClassEnrollments)
            .HasForeignKey(ce => ce.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ce => ce.User)
            .WithMany(u => u.ClassEnrollments)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ce => ce.Course)
            .WithMany()
            .HasForeignKey(ce => ce.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

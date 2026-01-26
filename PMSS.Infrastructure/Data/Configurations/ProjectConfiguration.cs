using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.ProjectId);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasOne(p => p.Class)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

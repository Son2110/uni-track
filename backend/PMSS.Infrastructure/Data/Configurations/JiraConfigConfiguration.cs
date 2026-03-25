using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class JiraConfigConfiguration : IEntityTypeConfiguration<JiraConfig>
{
    public void Configure(EntityTypeBuilder<JiraConfig> builder)
    {
        builder.HasKey(jc => jc.JiraConfigId);
        
        builder.Property(jc => jc.JiraUrl)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(jc => jc.ApiToken)
            .IsRequired()
            .HasMaxLength(2048);
        
        builder.HasIndex(jc => jc.ProjectId)
            .IsUnique();
        
        builder.HasOne(jc => jc.Project)
            .WithOne(p => p.JiraConfig)
            .HasForeignKey<JiraConfig>(jc => jc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

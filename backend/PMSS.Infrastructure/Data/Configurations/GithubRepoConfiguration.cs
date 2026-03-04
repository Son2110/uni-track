using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class GithubRepoConfiguration : IEntityTypeConfiguration<GithubRepo>
{
    public void Configure(EntityTypeBuilder<GithubRepo> builder)
    {
        builder.HasKey(gr => gr.GithubRepoId);
        
        builder.Property(gr => gr.RepoOwnerName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(gr => gr.RepoName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(gr => new { gr.RepoOwnerName, gr.RepoName })
            .IsUnique();
        
        builder.Property(gr => gr.ApiToken)
            .HasMaxLength(255);

        builder.Property(gr => gr.TotalCommits)
            .HasDefaultValue(0);

        builder.Property(gr => gr.TotalAdditions)
            .HasDefaultValue(0);

        builder.Property(gr => gr.TotalDeletions)
            .HasDefaultValue(0);
        
        builder.HasOne(gr => gr.Project)
            .WithMany(p => p.GithubRepos)
            .HasForeignKey(gr => gr.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

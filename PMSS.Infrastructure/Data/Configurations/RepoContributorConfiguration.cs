using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class RepoContributorConfiguration : IEntityTypeConfiguration<RepoContributor>
{
    public void Configure(EntityTypeBuilder<RepoContributor> builder)
    {
        builder.HasKey(rc => new { rc.GithubRepoId, rc.GithubUsername });
        
        builder.Property(rc => rc.GithubUsername)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(rc => rc.GithubEmail)
            .HasMaxLength(255);
        
        builder.HasOne(rc => rc.GithubRepo)
            .WithMany(gr => gr.RepoContributors)
            .HasForeignKey(rc => rc.GithubRepoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(rc => rc.User)
            .WithMany(u => u.RepoContributors)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

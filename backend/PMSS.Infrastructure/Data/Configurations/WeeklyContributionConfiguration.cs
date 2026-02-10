using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class WeeklyContributionConfiguration : IEntityTypeConfiguration<WeeklyContribution>
{
    public void Configure(EntityTypeBuilder<WeeklyContribution> builder)
    {
        builder.HasKey(wc => wc.WeeklyContributionId);

        // Unique constraint: one record per repo + week
        builder.HasIndex(wc => new { wc.GithubRepoId, wc.WeekTimestamp })
            .IsUnique();

        // Index for efficient querying by repo
        builder.HasIndex(wc => wc.GithubRepoId);

        // Index for date range queries
        builder.HasIndex(wc => new { wc.WeekStart, wc.WeekEnd });

        builder.Property(wc => wc.TotalCommits)
            .HasDefaultValue(0);

        builder.Property(wc => wc.TotalAdditions)
            .HasDefaultValue(0);

        builder.Property(wc => wc.TotalDeletions)
            .HasDefaultValue(0);

        builder.HasOne(wc => wc.GithubRepo)
            .WithMany(gr => gr.WeeklyContributions)
            .HasForeignKey(wc => wc.GithubRepoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

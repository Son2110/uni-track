using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class UserWeeklyContributionConfiguration : IEntityTypeConfiguration<UserWeeklyContribution>
{
    public void Configure(EntityTypeBuilder<UserWeeklyContribution> builder)
    {
        builder.HasKey(uwc => uwc.UserWeeklyContributionId);

        builder.Property(uwc => uwc.GithubUsername)
            .IsRequired()
            .HasMaxLength(255);

        // Unique constraint: one record per user per week (GitHub username)
        builder.HasIndex(uwc => new { uwc.WeeklyContributionId, uwc.GithubUsername })
            .IsUnique();

        // Index for efficient querying by weekly contribution
        builder.HasIndex(uwc => uwc.WeeklyContributionId);

        // Index for efficient querying by user
        builder.HasIndex(uwc => uwc.UserId);

        // Index for efficient querying by GitHub username
        builder.HasIndex(uwc => uwc.GithubUsername);

        builder.Property(uwc => uwc.Commits)
            .HasDefaultValue(0);

        builder.Property(uwc => uwc.Additions)
            .HasDefaultValue(0);

        builder.Property(uwc => uwc.Deletions)
            .HasDefaultValue(0);

        // Relationship: Many UserWeeklyContributions belong to one WeeklyContribution
        builder.HasOne(uwc => uwc.WeeklyContribution)
            .WithMany(wc => wc.UserContributions)
            .HasForeignKey(uwc => uwc.WeeklyContributionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Many UserWeeklyContributions can belong to one User (optional)
        builder.HasOne(uwc => uwc.User)
            .WithMany(u => u.WeeklyContributions)
            .HasForeignKey(uwc => uwc.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

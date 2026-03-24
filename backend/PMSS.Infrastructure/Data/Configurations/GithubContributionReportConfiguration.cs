using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class GithubContributionReportConfiguration : IEntityTypeConfiguration<GithubContributionReport>
{
    public void Configure(EntityTypeBuilder<GithubContributionReport> builder)
    {
        builder.HasKey(x => x.GithubContributionReportId);

        builder.Property(x => x.ExecutiveSummary)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.ModelProvider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.InsightsJson)
            .IsRequired();

        builder.Property(x => x.MarkdownContent)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GeneratedByUser)
            .WithMany()
            .HasForeignKey(x => x.GeneratedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => new { x.ProjectId, x.CreatedAt });
    }
}
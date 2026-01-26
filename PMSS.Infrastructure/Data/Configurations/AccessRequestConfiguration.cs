using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.HasKey(ar => ar.RequestId);
        
        builder.Property(ar => ar.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.HasIndex(ar => new { ar.RequesterId, ar.ProjectId, ar.Status });
        
        builder.HasOne(ar => ar.Requester)
            .WithMany(u => u.AccessRequests)
            .HasForeignKey(ar => ar.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(ar => ar.Project)
            .WithMany(p => p.AccessRequests)
            .HasForeignKey(ar => ar.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

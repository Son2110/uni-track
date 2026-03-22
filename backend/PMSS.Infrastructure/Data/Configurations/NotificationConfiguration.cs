using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);
        
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);
        
        builder.Property(n => n.CreatedAt)
            .IsRequired();
        
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}

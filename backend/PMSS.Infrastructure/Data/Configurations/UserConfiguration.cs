using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMSS.Domain.Entities;

namespace PMSS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(u => u.HashedPassword)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Email).IsUnique();
        
        builder.Property(u => u.GithubUsername)
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.GithubUsername).IsUnique();
        
        builder.Property(u => u.GithubEmail)
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.GithubEmail).IsUnique();
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();
    }
}

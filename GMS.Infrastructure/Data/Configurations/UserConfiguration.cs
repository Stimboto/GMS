using GMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        // Unique index on Email
        builder.HasIndex(u => u.Email).IsUnique();

        // One Role -> Many Users is configured in RoleConfiguration
        // Many Notifications -> One User is configured in NotificationConfiguration
        
        // Grievances submitted by the user
        builder.HasMany(u => u.SubmittedGrievances)
            .WithOne(g => g.SubmittedByUser)
            .HasForeignKey(g => g.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Grievances assigned to the user (Officer)
        builder.HasMany(u => u.AssignedGrievances)
            .WithOne(g => g.AssignedOfficer)
            .HasForeignKey(g => g.AssignedOfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

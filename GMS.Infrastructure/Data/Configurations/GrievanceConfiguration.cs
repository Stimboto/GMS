using GMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMS.Infrastructure.Data.Configurations;

public class GrievanceConfiguration : IEntityTypeConfiguration<Grievance>
{
    public void Configure(EntityTypeBuilder<Grievance> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .IsRequired();

        builder.Property(g => g.Category)
            .IsRequired()
            .HasMaxLength(100);

        // Enums mapping can be configured if using string representation, but int is default and fine for EF Core.

        // Useful indexes
        builder.HasIndex(g => g.DepartmentId);
        builder.HasIndex(g => g.Status);
        builder.HasIndex(g => g.Priority);

        // Relationships
        // One Department -> Many Grievances configured in DepartmentConfiguration
        // User -> Grievances configured in UserConfiguration

        // Many Attachments -> One Grievance
        builder.HasMany(g => g.Attachments)
            .WithOne(a => a.Grievance)
            .HasForeignKey(a => a.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many StatusHistory -> One Grievance
        builder.HasMany(g => g.StatusHistories)
            .WithOne(sh => sh.Grievance)
            .HasForeignKey(sh => sh.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

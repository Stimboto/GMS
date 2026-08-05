using GMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMS.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        // One Department -> Many Grievances
        builder.HasMany(d => d.Grievances)
            .WithOne(g => g.Department)
            .HasForeignKey(g => g.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Useful index
        builder.HasIndex(d => d.DepartmentName).IsUnique();

        // Seed sample departments
        builder.HasData(
            new Department { Id = 1, DepartmentName = "Water Supply", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 2, DepartmentName = "Electricity", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 3, DepartmentName = "Roads", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 4, DepartmentName = "Sanitation", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 5, DepartmentName = "Health", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 6, DepartmentName = "Education", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 7, DepartmentName = "Transport", CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

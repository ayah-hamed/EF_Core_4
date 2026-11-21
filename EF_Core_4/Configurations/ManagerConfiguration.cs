using EF_Core_4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF_Core_4.Configurations;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.HireDate)
            .HasColumnType("date");

        builder.Property(m => m.BranchCode)
            .IsRequired()
            .HasMaxLength(20);

        // a branch can be assigned to only one manager
        builder.HasOne(m => m.Branch)
            .WithOne(b => b.Manager)
            .HasForeignKey<Manager>(m => m.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
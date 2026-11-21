using EF_Core_4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF_Core_4.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.AccountNumber);

        builder.Property(a => a.AccountNumber)
            .HasMaxLength(30)
            .ValueGeneratedNever();

        builder.Property(a => a.CurrentBalance)
            .HasPrecision(18, 2);

        builder.Property(a => a.AccountType)
            .IsRequired();

        builder.Property(a => a.OpeningDate)
            .HasColumnType("date");

        builder.Property(a => a.BranchCode)
            .IsRequired()
            .HasMaxLength(20);

        // don't delete a branch while it still has accounts
        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Accounts)
            .HasForeignKey(a => a.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using EF_Core_4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF_Core_4.Configurations;

public class TransactionConfiguration
    : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.TransactionNumber);

        builder.Property(t => t.TransactionNumber)
            .HasMaxLength(40)
            .ValueGeneratedNever();

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.TransactionType)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(t => t.AccountNumber)
            .IsRequired()
            .HasMaxLength(30);

        // protect transaction history from automatic deletion
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountNumber)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
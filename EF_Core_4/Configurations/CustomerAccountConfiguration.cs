using EF_Core_4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF_Core_4.Configurations;

public class CustomerAccountConfiguration
    : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        // a customer can own the same account only once
        builder.HasKey(ca => new
        {
            ca.CustomerId,
            ca.AccountNumber
        });

        builder.Property(ca => ca.AccountNumber)
            .HasMaxLength(30);

        builder.Property(ca => ca.OwnershipType)
            .IsRequired();

        builder.Property(ca => ca.OwnershipStartDate)
            .HasColumnType("date");

        builder.Property(ca => ca.AccountStatus)
            .IsRequired();

        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.CustomerAccounts)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // deleting an account also removes its ownership links
        builder.HasOne(ca => ca.Account)
            .WithMany(a => a.CustomerAccounts)
            .HasForeignKey(ca => ca.AccountNumber)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
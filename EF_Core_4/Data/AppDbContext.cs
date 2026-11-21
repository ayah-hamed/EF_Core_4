using EF_Core_4.Entities;
using Microsoft.EntityFrameworkCore;

namespace EF_Core_4.Data;

public class AppDbContext : DbContext
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // use a separate database for the bank assignment
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=NationalBankDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // register all entity configuration classes in this project
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        // add the initial branches
        modelBuilder.Entity<Branch>().HasData(
            new Branch
            {
                Code = "CAI-01",
                Name = "Cairo Main Branch",
                Address = "Downtown, Cairo",
                PhoneNumber = "0223456789"
            },
            new Branch
            {
                Code = "ALX-01",
                Name = "Alexandria Branch",
                Address = "Smouha, Alexandria",
                PhoneNumber = "0334567890"
            }
        );

        // link each manager to a different branch
        // fixed dates keep the seed data consistent between migrations
        modelBuilder.Entity<Manager>().HasData(
            new Manager
            {
                Id = 1,
                FullName = "Mona Ali",
                Email = "mona.ali@nationalbank.example",
                PhoneNumber = "01012345678",
                HireDate = new DateTime(2020, 1, 15),
                BranchCode = "CAI-01"
            },
            new Manager
            {
                Id = 2,
                FullName = "Ahmed Hassan",
                Email = "ahmed.hassan@nationalbank.example",
                PhoneNumber = "01123456789",
                HireDate = new DateTime(2021, 6, 1),
                BranchCode = "ALX-01"
            }
        );
    }
}
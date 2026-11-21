using EF_Core_4.Enums;

namespace EF_Core_4.Entities;

public class Account
{
    public string AccountNumber { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public AccountType AccountType { get; set; }
    public DateTime OpeningDate { get; set; }

    public string BranchCode { get; set; } = null!;
    public Branch Branch { get; set; } = null!;

    // joint accounts can have more than one customer
    public ICollection<CustomerAccount> CustomerAccounts { get; set; }
        = new List<CustomerAccount>();

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}
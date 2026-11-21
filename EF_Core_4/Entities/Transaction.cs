using EF_Core_4.Enums;

namespace EF_Core_4.Entities;

public class Transaction
{
    public string TransactionNumber { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }

    // a transaction can be recorded without an extra note
    public string? Note { get; set; }

    public string AccountNumber { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
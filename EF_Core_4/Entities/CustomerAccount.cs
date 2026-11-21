using EF_Core_4.Enums;

namespace EF_Core_4.Entities;

public class CustomerAccount
{
    // these two properties will form the composite primary key
    public int CustomerId { get; set; }
    public string AccountNumber { get; set; } = null!;

    public OwnershipType OwnershipType { get; set; }
    public DateTime OwnershipStartDate { get; set; }

    // status belongs to this customer's ownership link
    public AccountStatus AccountStatus { get; set; }

    public Customer Customer { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
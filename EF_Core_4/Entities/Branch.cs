namespace EF_Core_4.Entities;

public class Branch
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    // the relationship will be configured using Manager.BranchCode
    public Manager Manager { get; set; } = null!;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
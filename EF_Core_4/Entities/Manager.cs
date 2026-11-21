namespace EF_Core_4.Entities;

public class Manager
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTime HireDate { get; set; }

    // each manager is assigned to one branch
    public string BranchCode { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
namespace BlockiFinAid.Data.Models;

public class BankAccountsHistory : IModel
{
    public Guid Id { get; set; }
    public ICollection<AccountsAddedDetails> BankAccounts { get; set; } = new List<AccountsAddedDetails>();
}

public class AccountsAddedDetails
{
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsActive { get; set; }
    public string? InactivityReason { get; set; }
}
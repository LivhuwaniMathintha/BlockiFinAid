namespace BlockiFinAid.Services.SmartContracts.BankAccount;

public class BankAccountConfirmationDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string UserId { get; set; }
    public bool IsConfirmed { get; set; }
    
}
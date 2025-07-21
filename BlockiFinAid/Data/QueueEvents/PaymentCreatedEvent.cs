namespace BlockiFinAid.Data.QueueEvents;

public class PaymentCreatedEvent
{
    public string Id { get; set; }
    public string StudentEmail { get; set; }
    public string StudentName { get; set; }
    public string UserId { get; set; }
    public string StudentBankAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentType { get; set; }
    public string StudentBankName { get; set; }
    public string StudentBranchCode { get; set; }
    public string OrgBankAccountNumber { get; set; }
    public string OrgBankName { get; set; }
    public string OrgBankBranchCode { get; set; }
    public DateTime InitiatedDate { get; set; } = DateTime.Now;
    public string PaymentStatus { get; set; } = "Initiated";
    public bool IsPaid { get; set; } = false;
}
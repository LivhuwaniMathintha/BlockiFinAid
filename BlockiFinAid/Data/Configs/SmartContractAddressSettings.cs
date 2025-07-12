namespace BlockiFinAid.Data.Configs;

public class SmartContractAddressSettings
{
    public string? AuditTrailAddress { get; set; }
    public  string[] InstitutionRegistryAbi { get; set; }
    public string[] AuditTrailAbi { get; set; }
    public string[] FundingManagerAbi { get; set; }
    public string? FundingManagerAddress { get; set; }
    public string? InstitutionRegistryAddress { get; set; }
    public string? StudentAddress { get; set; }
}



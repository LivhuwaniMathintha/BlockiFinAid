using System.Text.Json.Serialization;
using BlockiFinAid.Services.SmartContracts.Funder;
using Refit;

namespace BlockiFinAid.Services.MachineLearning;

public interface IMachineLearningAPI
{
    [Post("/predict")]
    Task<ApiResponse<MachineLearningResponse>> ProcessMachineLearningRequest([Body] List<MachineLearningRequest> request);

}

public class MachineLearningResponse
{
    [property: JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }
    
    [property: JsonPropertyName("is_fraud_predicted")]
    public bool FraudPredicted { get; set; }
    
    [property: JsonPropertyName("fraud_probability")]
    public decimal FraudProbability { get; set; }
}

public record MachineLearningRequest(
    // Core Identifiers
    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("transaction_id")]
    string TransactionId,

    [property: JsonPropertyName("transaction_group_id")]
    string TransactionGroupId,

    [property: JsonPropertyName("student_id")]
    string StudentId,

    [property: JsonPropertyName("student_number")]
    string StudentNumber,

    [property: JsonPropertyName("institution_service_id")]
    string InstitutionServiceId,

    // Financial/Amount Details
    [property: JsonPropertyName("amount")]
    float Amount,

    [property: JsonPropertyName("funder_base_amount")]
    float FunderBaseAmount,

    [property: JsonPropertyName("balance")]
    float Balance,

    // Date/Time Fields
    [property: JsonPropertyName("initiation_date")]
    string InitiationDate, // Changed to DateTime

    [property: JsonPropertyName("fulfilment_date")]
    string FulfilmentDate, // Changed to DateTime

    [property: JsonPropertyName("updated_at")]
    string UpdatedAt, 

    [property: JsonPropertyName("funder_base_amount_date")]
    string FunderBaseAmountDate, 

    // Bank Details
    [property: JsonPropertyName("account_number")]
    string AccountNumber, 

    [property: JsonPropertyName("bank_name")]
    string BankName,

    [property: JsonPropertyName("branch_code")]
    string BranchCode,

    // Actors/Entities
    [property: JsonPropertyName("funder")]
    string Funder,

    [property: JsonPropertyName("institution")]
    string Institution,

    [property: JsonPropertyName("modified_by")]
    string ModifiedBy, 

    [property: JsonPropertyName("user_id_performing_action")]
    string UserIdPerformingAction, 

    [property: JsonPropertyName("active_base_amount_id")]
    string ActiveBaseAmountId, 

    // Status/Type/Period Details
    [property: JsonPropertyName("payment_type")]
    string PaymentType,

    [property: JsonPropertyName("status")]
    string Status, 

    [property: JsonPropertyName("bulk_payment_reason")]
    string BulkPaymentReason, 

    [property: JsonPropertyName("months_covered")]
    int MonthsCovered, // Changed to non-nullable

    [property: JsonPropertyName("agreement_signed_month")]
    int AgreementSignedMonth, 

    [property: JsonPropertyName("year")]
    int Year, 

    [property: JsonPropertyName("month")]
    int Month,

    // Flag Fields (Booleans)
    [property: JsonPropertyName("is_fraud")]
    bool IsFraud,

    [property: JsonPropertyName("base_amount_is_active")]
    bool BaseAmountIsActive, 

    [property: JsonPropertyName("is_bulk_payment")]
    bool IsBulkPayment,

    [property: JsonPropertyName("once_off")]
    bool OnceOff,

    [property: JsonPropertyName("duplicate_payment_flag")]
    bool DuplicatePaymentFlag,

    [property: JsonPropertyName("multiple_funder_flag")]
    bool MultipleFunderFlag,

    [property: JsonPropertyName("funding_is_active")]
    bool FundingIsActive,

    [property: JsonPropertyName("suspicious_bank_flag")]
    bool SuspiciousBankFlag,

    [property: JsonPropertyName("is_new_data")]
    bool IsNewData 
    );



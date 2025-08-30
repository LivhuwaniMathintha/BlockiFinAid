using Refit;

namespace BlockiFinAid.Services.MachineLearning;

public interface IMachineLearningAPI
{
    [Post("/api/machinelearning/fraud_detection")]
    Task<ApiResponse<MachineLearningResponse>> ProcessMachineLearningRequest([Body] MachineLearningRequest request);
}

public class MachineLearningResponse
{
    public decimal Prediction { get; set; }
    public bool IsSuccess { get; set; }
}


public record MachineLearningRequest(
    decimal Amount,
    string Funder,
    string PaymentType,
    DateTime PaymentDate,
    bool IsDuplicate);

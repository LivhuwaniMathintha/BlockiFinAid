using Refit;

namespace BlockiFinAid.Services.MachineLearning;

public interface IMachineLearningAPI
{
    [Get("/api/machinelearning")]
    Task<MachineLearningResponse> ProcessMachineLearningRequest([Query] MachineLearningRequest request);
}

public record MachineLearningResponse(decimal Prediction, bool IsSuccess);

public record MachineLearningRequest(
    decimal Amount,
    string Funder,
    string PaymentType,
    DateTime PaymentDate,
    bool IsDuplicate);

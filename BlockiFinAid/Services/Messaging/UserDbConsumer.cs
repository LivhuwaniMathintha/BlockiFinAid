using System.Text.Json;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.MachineLearning;
using BlockiFinAid.Services.Observability;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;
using Refit;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace BlockiFinAid.Services.Messaging;

public class UserDbConsumer : IConsumer<UserModel>
{
    private readonly UserContract _userContract;
    private readonly ILogger<UserDbConsumer> _logger;
    private readonly OperationTimingTracker _timingTracker;

    public UserDbConsumer(ILogger<UserDbConsumer> logger, UserContract userContract, OperationTimingTracker timingTracker)
    {
        _userContract = userContract;
        _logger = logger;
        _timingTracker = timingTracker;
    }
    public async Task Consume(ConsumeContext<UserModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(UserModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);

        await Task.Delay(TimeSpan.FromSeconds(6));
        _logger.LogInformation($"[User Consumer] - Consuming the following user {context.Message.StudentNumber} from the queue: ");
        var item = new UserInputDto
        {
            Name = context.Message.Name,
            Email = context.Message.Email,
            StudentNumber = context.Message.StudentNumber,
            BankAccountId = context.Message.BankAccountId.ToString(),
            CourseName = context.Message.CourseName,
            FunderContractId = context.Message.FunderContractId.ToString(),
            IsActive = context.Message.IsActive,
            ModifiedBy = context.Message.ModifiedBy,
            IsChangeConfirmed = context.Message.IsChangeConfirmed,
            InstitutionId = context.Message.InstitutionId.ToString(),
            Role =  context.Message.Role,
            Id = context.Message.Id.ToString(),
        };

        try
        {
            var beforeContract = DateTime.UtcNow;
            var results = await _userContract.AddUserAsync(item);
            var contractElapsedMs = (long)(DateTime.UtcNow - beforeContract).TotalMilliseconds;
            _timingTracker.RecordStep("user_sync", modelName, "smart_contract_write", contractElapsedMs, entityId);
            _logger.LogInformation(
                $"[User Consumer] - User {context.Message.StudentNumber} added to the smart contract for users with status: {results}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[User Consumer] - User for {item.StudentNumber} is not in the smart contract, however, this account is using id: {item.Id} and this is the error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                //Retry Logic
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[User Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var results = await _userContract.AddUserAsync(item);
                        _logger.LogInformation(
                            $"[User Consumer] - Adding data from db to smart contract in real-time for user model complete with status: {results}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[User Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            // If it's the last attempt and it fails, log the final error
                            _logger.LogError($"[User Consumer] - User for {item.StudentNumber} is not in the smart contract, however, this user is using id: {item.Id} and this is the final error after retries: {ex}");
                        }
                    }
                }
            }
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("user_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }
}

public class BankAccountDbConsumer : IConsumer<BankAccountModel>
{
    private readonly ILogger<BankAccountDbConsumer> _logger;
    private readonly BankAccountContract _bankAccountContract;
    private readonly OperationTimingTracker _timingTracker;

    public BankAccountDbConsumer(ILogger<BankAccountDbConsumer> logger, BankAccountContract bankAccountContract, OperationTimingTracker timingTracker)
    {
        _logger = logger;
        _bankAccountContract = bankAccountContract;
        _timingTracker = timingTracker;
    }
    
    public async Task Consume(ConsumeContext<BankAccountModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(BankAccountModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);

        _logger.LogInformation($"[Bank Account Consumer] - BankAccountNumber: {context.Message.BankAccountNumber} has been added to the database and retrieved in real-time. Queue for Smart contract addition");
        await Task.Delay(TimeSpan.FromSeconds(5));
        var item = new BankAccountInputDto()
        {
            BankAccountNumber = context.Message.BankAccountNumber,
            BankBranchCode = context.Message.BankBranchCode,
            BankName = context.Message.BankName,
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            StudentNumber =  context.Message.StudentNumber,
            Id = context.Message.Id.ToString(),
        };

        try
        {
            var beforeContract = DateTime.UtcNow;
            var results = await _bankAccountContract.AddBankAccountAsync(item);
            var contractElapsedMs = (long)(DateTime.UtcNow - beforeContract).TotalMilliseconds;
            _timingTracker.RecordStep("bank_account_sync", modelName, "smart_contract_write", contractElapsedMs, entityId);

            _logger.LogInformation(
                $"[Bank Account Consumer] - Adding data from db to smart contract in real-time for bankAccount model is complete with status: {results}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Bank Account Consumer] - Account for {item.StudentNumber} is not in the smart contract, however, this account is using id: {item.Id} and this is the error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Bank Account Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var beforeRetry = DateTime.UtcNow;
                        var results = await _bankAccountContract.AddBankAccountAsync(item);
                        var retryElapsedMs = (long)(DateTime.UtcNow - beforeRetry).TotalMilliseconds;
                        _timingTracker.RecordStep("bank_account_sync", modelName, "smart_contract_retry", retryElapsedMs, entityId, "retry");
                        _logger.LogInformation(
                            $"[Bank Account Consumer] - Adding data from db to smart contract in real-time for account model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Bank Account Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            _logger.LogError($"[Bank Account Consumer] - Account for {item.StudentNumber} is not in the smart contract, however, this account is using id: {item.Id} and this is the final error after retries: {ex}");
                        }
                    }
                }
            }
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("bank_account_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }
}

public class FundingDbConsumer : IConsumer<FundingModel>
{
    private readonly FundingContract _fundingContract;
    private readonly ILogger<FundingDbConsumer> _logger;
    private readonly OperationTimingTracker _timingTracker;

    public FundingDbConsumer(ILogger<FundingDbConsumer> logger,  FundingContract fundingContract, OperationTimingTracker timingTracker)
    {
        _fundingContract = fundingContract;
        _logger = logger;
        _timingTracker = timingTracker;
    }
    public async Task Consume(ConsumeContext<FundingModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(FundingModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);

        await Task.Delay(TimeSpan.FromSeconds(4));
        _logger.LogInformation($"[Funding Consumer] - Consuming funding model for student {context.Message.StudentId} from the queue: ");
        var item = new FundingInputDto
        {
            StudentId = context.Message.StudentId.ToString(),
            FunderContractConditionId = context.Message.FunderContractConditionId.ToString(),
            DataConfirmedById = context.Message.DataConfirmedById.ToString(),
            SignedOn = context.Message.SignedOn.ToString("G"),
            FoodBalance = context.Message.FoodBalance,
            TuitionBalance = context.Message.TuitionBalance,
            LaptopBalance = context.Message.LaptopBalance,
            AccommodationBalance = context.Message.AccommodationBalance,
            ModifiedBy = context.Message.ModifiedBy,
            FunderId = context.Message.FunderId.ToString()
        };
        try
        {
            var beforeContract = DateTime.UtcNow;
            var results = await _fundingContract.AddFundingAsync(item);
            var contractElapsedMs = (long)(DateTime.UtcNow - beforeContract).TotalMilliseconds;
            _timingTracker.RecordStep("funding_sync", modelName, "smart_contract_write", contractElapsedMs, entityId);
            _logger.LogInformation(
                $"[Funding Consumer]- Adding data from db to smart contract in real-time for funding model complete with status: {results}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Funding Consumer] - Funding for {item.StudentId} is not in the smart contract. Error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Funding Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var beforeRetry = DateTime.UtcNow;
                        var results = await _fundingContract.AddFundingAsync(item);
                        var retryElapsedMs = (long)(DateTime.UtcNow - beforeRetry).TotalMilliseconds;
                        _timingTracker.RecordStep("funding_sync", modelName, "smart_contract_retry", retryElapsedMs, entityId, "retry");
                        _logger.LogInformation(
                            $"[Funding Consumer] - Adding data from db to smart contract in real-time for funding model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Funding Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            _logger.LogError($"[Funding Consumer] - Funding for {item.StudentId} is not in the smart contract. This is the final error after retries: {ex.Message}");
                        }
                    }
                }
            }
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("funding_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }
}

public class FundingConditionsDbConsumer : IConsumer<FundingConditionsModel>
{
    private readonly FundingConditionsContract _fundingConditionsContract;
    private readonly ILogger<FundingConditionsDbConsumer> _logger;
    private readonly OperationTimingTracker _timingTracker;

    public FundingConditionsDbConsumer(FundingConditionsContract fundingConditionsContract, ILogger<FundingConditionsDbConsumer> logger, OperationTimingTracker timingTracker)
    {
        _fundingConditionsContract = fundingConditionsContract;
        _logger = logger;
        _timingTracker = timingTracker;
    }
    public async Task Consume(ConsumeContext<FundingConditionsModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(FundingConditionsModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);

        await Task.Delay(TimeSpan.FromSeconds(3));
        _logger.LogInformation($"[Conditions Consumer] - Consuming funding conditions model with id {context.Message.Id} from the queue: ");
        var item = new FundingConditionInputDto()
        {
            AccommodationAmount = context.Message.AccommodationAmount,
            AccommodationDirectPay = context.Message.AccommodationDirectPay,
            AverageMark = context.Message.AverageMark,
            FoodAmount = context.Message.FoodAmount,
            TuitionAmount = context.Message.TuitionAmount,
            TotalAmount = context.Message.TotalAmount,
            EndDate = context.Message.EndDate.ToString("G"),
            LaptopAmount = context.Message.LaptopAmount,
            IsFullCoverage = context.Message.IsFullCoverage,
            StartDate = context.Message.StartDate.ToString("G"),
            UpdatedAt = context.Message.UpdatedAt.ToString("G"),
            ModifiedBy = context.Message.ModifiedBy,
            Id = context.Message.Id.ToString(),
        };

        try
        {
            var beforeContract = DateTime.UtcNow;
            var results = await _fundingConditionsContract.AddConditionAsync(item);
            var contractElapsedMs = (long)(DateTime.UtcNow - beforeContract).TotalMilliseconds;
            _timingTracker.RecordStep("funding_conditions_sync", modelName, "smart_contract_write", contractElapsedMs, entityId);
            if(results.IsSuccess)
                _logger.LogInformation($"[Conditions Consumer] - Adding data from db to smart contract in real-time for funding conditions model complete with status: {results}");
            else
            {
                _logger.LogError($"[Conditions Consumer] - Adding conditions to the smart contract, failed with error: {results.Data}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Conditions Consumer] - Funding Condition is not in the smart contract, however, this condition is using id: {item.Id} and this is the error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Conditions Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var beforeRetry = DateTime.UtcNow;
                        var results = await _fundingConditionsContract.AddConditionAsync(item);
                        var retryElapsedMs = (long)(DateTime.UtcNow - beforeRetry).TotalMilliseconds;
                        _timingTracker.RecordStep("funding_conditions_sync", modelName, "smart_contract_retry", retryElapsedMs, entityId, "retry");
                        _logger.LogInformation(
                            $"[Conditions Consumer] - Adding data from db to smart contract in real-time for funding condition model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Conditions Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            _logger.LogError($"[Conditions Consumer] - Condition is not in the smart contract, however, this funding condition is using id: {item.Id} and this is the final error after retries: {ex}");
                        }
                    }
                }
            }
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("funding_conditions_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }
}

public class PaymentDbConsumer : IConsumer<PaymentModel>
{
 
    private readonly ILogger<PaymentDbConsumer> _logger;
    private readonly PaymentContract _paymentContract;
    private readonly IBaseRepository<PaymentModel> _paymentRepo;
    private readonly FunderContract _funderContract;
    private readonly FundingContract _fundingContract;
    private readonly UserContract _userContract;
    private readonly IMachineLearningAPI _machineLearningApi;

    private readonly OperationTimingTracker _timingTracker;

    public PaymentDbConsumer(ILogger<PaymentDbConsumer> logger, 
        PaymentContract contract, 
        IBaseRepository<PaymentModel> repository, 
        FunderContract funder, 
        FundingContract fundingContract, 
        UserContract userContract,
        IMachineLearningAPI machineLearningApi,
        OperationTimingTracker timingTracker)
    {
        _paymentContract = contract;
        _logger = logger;
        _paymentRepo = repository;
        _funderContract = funder;
        _fundingContract = fundingContract;
        _userContract = userContract;
        _machineLearningApi = machineLearningApi;
        _timingTracker = timingTracker;
    }

    private float GetPaymentTypeBalance(string paymentType, FundingOutputDto funding)
    {
        switch (paymentType)
        {
            case "Food":
                return funding.FoodBalance;
            case "Laptop":
            case "Laptop/Book":
                return funding.LaptopBalance;
            case "Accommodation":
                return funding.AccommodationBalance;
            default:
                return 0;
        }
    }
    public async Task Consume(ConsumeContext<PaymentModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(PaymentModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);
        var payment = context.Message;

        try
        {
            if (payment.DataSource is DataSources.Demo or DataSources.Seed)
            {
                _logger.LogInformation(
                    "Skipping real payment processing for {DataSource} payment {PaymentId}.",
                    payment.DataSource,
                    payment.Id);
                return;
            }

            if (payment.Status is not (PaymentStatuses.Initiated or PaymentStatuses.FraudCheckFailed or PaymentStatuses.FraudCheckPassed))
            {
                _logger.LogInformation("Skipping payment {PaymentId} because its state is {PaymentStatus}.", payment.Id, payment.Status);
                return;
            }

            var student = await _userContract.GetUserByStudentNumberAsync(payment.StudentNumber);
            var funder = await _funderContract.GetFunderByNameAsync(payment.Funder);
            if (student is null || funder is null)
            {
                throw new InvalidOperationException($"Could not resolve student or funder for payment {payment.Id}.");
            }

            var funding = await _fundingContract.GetFundingByStudentIdAsync(student.Id);
            if (funding is null)
            {
                throw new InvalidOperationException($"Could not resolve funding for payment {payment.Id}.");
            }

            if (payment.Status != PaymentStatuses.FraudCheckPassed)
            {
                var allFunding = await _fundingContract.GetAllFundingsAsync();
                DateTime.TryParse(funding.SignedOn, out var fundingSignedOnDate);
                var mlRequest = new MachineLearningRequest(
                    Guid.NewGuid().ToString(),
                    payment.TransactionId,
                    payment.TransactionGroupId,
                    payment.StudentId.ToString(),
                    payment.StudentNumber,
                    payment.InstitutionServiceId.ToString(),
                    payment.Amount,
                    payment.Amount,
                    GetPaymentTypeBalance(payment.PaymentType, funding),
                    payment.InitiationDate.ToString("o"),
                    payment.FulfilmentDate.ToString("o"),
                    DateTime.UtcNow.ToString("o"),
                    payment.InitiationDate.ToString("o"),
                    payment.AccountNumber,
                    payment.BankName,
                    payment.BranchCode,
                    payment.Funder,
                    payment.Institution,
                    payment.ModifiedBy,
                    payment.UserIdPerformingAction.ToString(),
                    Guid.NewGuid().ToString(),
                    payment.PaymentType,
                    payment.Status,
                    "N/A",
                    1,
                    fundingSignedOnDate.Month,
                    payment.InitiationDate.Year,
                    payment.InitiationDate.Month,
                    false,
                    true,
                    false,
                    false,
                    false,
                    allFunding.Count(x => x.StudentId == payment.StudentId) > 1,
                    funding.IsActive,
                    false,
                    true);

                var mlResponse = await _machineLearningApi.ProcessMachineLearningRequest(new List<MachineLearningRequest> { mlRequest });
                _logger.LogInformation("Fraud-check endpoint returned HTTP {StatusCode} for payment {PaymentId}.", mlResponse.StatusCode, payment.Id);
                if (!mlResponse.IsSuccessStatusCode || mlResponse.Content is null)
                {
                    payment.Status = PaymentStatuses.FraudCheckFailed;
                    await UpdatePaymentAsync(payment);
                    throw new InvalidOperationException($"Fraud check failed for payment {payment.Id} with HTTP {mlResponse.StatusCode}.");
                }

                if (mlResponse.Content.FraudPredicted)
                {
                    payment.Status = PaymentStatuses.FraudRejected;
                    payment.IsFraud = true;
                    await UpdatePaymentAsync(payment);
                    _logger.LogWarning("Payment {PaymentId} was rejected by the fraud check.", payment.Id);
                    return;
                }

                payment.Status = PaymentStatuses.FraudCheckPassed;
                await UpdatePaymentAsync(payment);
            }

            var contractPayments = await _paymentContract.GetAllPaymentsAsync();
            var contractPayment = contractPayments.FirstOrDefault(x => x.Id == payment.Id);
            if (contractPayment is null)
            {
                var beforeContract = DateTime.UtcNow;
                var addResult = await _paymentContract.AddPaymentAsync(new PaymentInputDto
                {
                    Id = payment.Id.ToString(),
                    AccountNumber = payment.AccountNumber,
                    StudentNumber = payment.StudentNumber,
                    BranchCode = payment.BranchCode,
                    BankName = payment.BankName,
                    Amount = payment.Amount,
                    PaymentType = payment.PaymentType,
                    Status = PaymentStatuses.FraudCheckPassed,
                    IsFraud = false,
                    InitiationDate = payment.InitiationDate.ToString("G"),
                    FulfilmentDate = payment.FulfilmentDate.ToString("G"),
                    ModifiedBy = payment.ModifiedBy,
                });
                _timingTracker.RecordStep("payment_sync", modelName, "smart_contract_write", (long)(DateTime.UtcNow - beforeContract).TotalMilliseconds, entityId);
                if (!addResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Smart-contract insertion failed for payment {payment.Id}.");
                }
            }

            if (contractPayment?.Status is not (PaymentStatuses.RecordedOnChain or PaymentStatuses.Fulfilled))
            {
                var updateResult = await _paymentContract.UpdatePaymentByIdAsync(new PaymentUpdateDto
                {
                    Id = payment.Id.ToString(),
                    Status = PaymentStatuses.RecordedOnChain,
                    FulfilmentDate = payment.FulfilmentDate.ToString("G"),
                    ModifiedBy = payment.ModifiedBy,
                });
                if (!updateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"Smart-contract state update failed for payment {payment.Id}.");
                }
            }

            payment.Status = contractPayment?.Status == PaymentStatuses.Fulfilled
                ? PaymentStatuses.Fulfilled
                : PaymentStatuses.RecordedOnChain;
            await UpdatePaymentAsync(payment);
            _logger.LogInformation("Payment {PaymentId} reached state {PaymentStatus}.", payment.Id, payment.Status);
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("payment_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }

    private async Task UpdatePaymentAsync(PaymentModel payment)
    {
        payment.UpdatedAt = DateTime.UtcNow;
        if (!await _paymentRepo.UpdateAsync(payment, "payment-consumer"))
        {
            throw new InvalidOperationException($"Could not persist state {payment.Status} for payment {payment.Id}.");
        }
    }
}

public class FunderDbConsumer : IConsumer<FunderModel>
{
    private readonly FunderContract _funderContract;
    private readonly ILogger<FunderDbConsumer> _logger;
    private readonly OperationTimingTracker _timingTracker;

    public FunderDbConsumer(ILogger<FunderDbConsumer> logger,  FunderContract funderContract, OperationTimingTracker timingTracker)
    {
        _funderContract = funderContract;
        _logger = logger;           
        _timingTracker = timingTracker;
    }
    
    public async Task Consume(ConsumeContext<FunderModel> context)
    {
        var startedAt = DateTime.UtcNow;
        var modelName = nameof(FunderModel);
        var entityId = context.Message.Id.ToString();
        var enqueuedAtUtc = context.SentTime is { } sentTime ? (sentTime.Kind == DateTimeKind.Utc ? sentTime : sentTime.ToUniversalTime()) : startedAt;
        _timingTracker.RecordQueueLatency(modelName, enqueuedAtUtc, startedAt, entityId);

        await Task.Delay(TimeSpan.FromSeconds(1));
        _logger.LogInformation($"Consuming funding model for Funder {context.Message.Name} from the queue: ");
        var item = new FunderInputDto
        {
            Id = context.Message.Id.ToString(),
            IsChangeConfirmed = context.Message.IsChangeConfirmed,
            Name = context.Message.Name,
            ModifiedBy = context.Message.ModifiedBy,
            PaymentDate = context.Message.PaymentDate.ToString("G"),
            FunderContractId = context.Message.FunderContractId.ToString(),
        };

        _logger.LogInformation($"Looking for funder: {item.Name} with id: {item.Id} in the smart contract");
        try
        {
            var beforeLookup = DateTime.UtcNow;
            var isFunderOnContract = await _funderContract.GetFunderByNameAsync(item.Name);
            var lookupElapsedMs = (long)(DateTime.UtcNow - beforeLookup).TotalMilliseconds;
            _timingTracker.RecordStep("funder_sync", modelName, "lookup_contract", lookupElapsedMs, entityId);
            if (isFunderOnContract?.Name == "")
            {
                try
                {
                    var beforeWrite = DateTime.UtcNow;
                    var results = await _funderContract.AddFunderAsync(item);
                    var writeElapsedMs = (long)(DateTime.UtcNow - beforeWrite).TotalMilliseconds;
                    _timingTracker.RecordStep("funder_sync", modelName, "smart_contract_write", writeElapsedMs, entityId);
                    _logger.LogInformation($"Adding data from db to smart contract in real-time for funder model complete with status: {results.BlockHash}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Funder using name {item.Name} is not in the smart contract, however, this funder is using id: {item.Id} and this is the error: {ex}");
                    if (ex.Message.Contains("Nonce"))
                    {
                        int maxRetries = 5;
                        for (int i = 0; i < 5; i++)
                        {
                            try
                            {
                                _logger.LogWarning(
                                    $"Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                                var beforeRetry = DateTime.UtcNow;
                                var results = await _funderContract.AddFunderAsync(item);
                                var retryElapsedMs = (long)(DateTime.UtcNow - beforeRetry).TotalMilliseconds;
                                _timingTracker.RecordStep("funder_sync", modelName, "smart_contract_retry", retryElapsedMs, entityId, "retry");
                                _logger.LogInformation(
                                    $"Adding data from db to smart contract in real-time for funder model complete with status: {results.BlockHash}");
                                break;
                            }
                            catch (Exception retryEx)
                            {
                                _logger.LogError($"Retry attempt failed with error: {retryEx.Message}");
                                if (i == maxRetries - 1)
                                {
                                    _logger.LogError($"Funder using name {item.Name} is not in the smart contract, however, this funder is using id: {item.Id} and this is the final error after retries: {ex}");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.LogError($"Funder {isFunderOnContract?.Name} from the queue already exists in the smart contract - blockchain");
            }
        }
        finally
        {
            var totalElapsedMs = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _timingTracker.RecordStep("funder_sync", modelName, "total_processing", totalElapsedMs, entityId);
        }
    }
}

using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;

namespace BlockiFinAid.Services.Messaging;

public class UserDbConsumer : IConsumer<UserModel>
{
    private readonly UserContract _userContract;
    private readonly ILogger<UserDbConsumer> _logger;

    public UserDbConsumer(ILogger<UserDbConsumer> logger, UserContract userContract)
    {
        _userContract = userContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<UserModel> context)
    {
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
            var results = await _userContract.AddUserAsync(item);
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
    }
}

public class BankAccountDbConsumer : IConsumer<BankAccountModel>
{
    private readonly ILogger<BankAccountDbConsumer> _logger;

    private readonly BankAccountContract _bankAccountContract;


    public BankAccountDbConsumer(ILogger<BankAccountDbConsumer> logger, BankAccountContract bankAccountContract)
    {
        _logger = logger;
        _bankAccountContract = bankAccountContract;
    }
    
    public async Task Consume(ConsumeContext<BankAccountModel> context)
    {
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

            var results = await _bankAccountContract.AddBankAccountAsync(item);

            _logger.LogInformation(
                $"[Bank Account Consumer] - Adding data from db to smart contract in real-time for bankAccount model is complete with status: {results}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Bank Account Consumer] - Account for {item.StudentNumber} is not in the smart contract, however, this account is using id: {item.Id} and this is the error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                //Retry Logic
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Bank Account Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var results = await _bankAccountContract.AddBankAccountAsync(item);
                        _logger.LogInformation(
                            $"[Bank Account Consumer] - Adding data from db to smart contract in real-time for account model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Bank Account Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            // If it's the last attempt and it fails, log the final error
                            _logger.LogError($"[Bank Account Consumer] - Account for {item.StudentNumber} is not in the smart contract, however, this account is using id: {item.Id} and this is the final error after retries: {ex}");
                        }
                    }
                }
            }
        }
    }
}

public class FundingDbConsumer : IConsumer<FundingModel>
{
    private readonly FundingContract _fundingContract;
    private readonly ILogger<FundingDbConsumer> _logger;

    public FundingDbConsumer(ILogger<FundingDbConsumer> logger,  FundingContract fundingContract)
    {
        _fundingContract = fundingContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<FundingModel> context)
    {
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

            var results = await _fundingContract.AddFundingAsync(item);
            _logger.LogInformation(
                $"[Funding Consumer]- Adding data from db to smart contract in real-time for funding model complete with status: {results}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Funding Consumer] - Funding for {item.StudentId} is not in the smart contract. Error: {ex.Message}");
            if (ex.Message.Contains("Nonce"))
            {
                //Retry Logic
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Funding Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var results = await _fundingContract.AddFundingAsync(item);
                        _logger.LogInformation(
                            $"[Funding Consumer] - Adding data from db to smart contract in real-time for funding model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Funding Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            // If it's the last attempt and it fails, log the final error
                            _logger.LogError($"[Funding Consumer] - Funding for {item.StudentId} is not in the smart contract. This is the final error after retries: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}

public class FundingConditionsDbConsumer : IConsumer<FundingConditionsModel>
{
    private readonly FundingConditionsContract _fundingConditionsContract;
    private readonly ILogger<FundingConditionsDbConsumer> _logger;

    public FundingConditionsDbConsumer(FundingConditionsContract fundingConditionsContract, ILogger<FundingConditionsDbConsumer> logger)
    {
        _fundingConditionsContract = fundingConditionsContract;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<FundingConditionsModel> context)
    {
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

            var results = await _fundingConditionsContract.AddConditionAsync(item);
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
                //Retry Logic
                int maxRetries = 5;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        _logger.LogWarning(
                            $"[Conditions Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                        var results = await _fundingConditionsContract.AddConditionAsync(item);
                        _logger.LogInformation(
                            $"[Conditions Consumer] - Adding data from db to smart contract in real-time for funding condition model complete with status: {results.BlockHash}");
                        break;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError($"[Conditions Consumer] - Retry attempt failed with error: {retryEx.Message}");
                        if (i == maxRetries - 1)
                        {
                            // If it's the last attempt and it fails, log the final error
                            _logger.LogError($"[Conditions Consumer] - Condition is not in the smart contract, however, this funding condition is using id: {item.Id} and this is the final error after retries: {ex}");
                        }
                    }
                }
            }
        }
    }
}

public class PaymentDbConsumer : IConsumer<PaymentModel>
{
 
    private readonly ILogger<PaymentDbConsumer> _logger;
    private readonly PaymentContract _paymentContract;
    private readonly IBaseRepository<PaymentModel> _paymentRepo;

    public PaymentDbConsumer(ILogger<PaymentDbConsumer> logger, PaymentContract contract, IBaseRepository<PaymentModel> repository)
    {
        _paymentContract = contract;
        _logger = logger;
        _paymentRepo = repository;
    }
    public async Task Consume(ConsumeContext<PaymentModel> context)
    {
        if (context.Message.Status == "Fulfilled")
        {
            _logger.LogInformation($"[Payment Initiator] - Fulfilled Payment");
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger.LogInformation($"[Payment Consumer] - Consuming payment from the queue ");
            var updatePayment = new PaymentModel
            {
                TransactionGroupId = context.Message.TransactionGroupId,
                TransactionId = context.Message.TransactionId,
                AccountNumber = context.Message.AccountNumber,
                StudentNumber = context.Message.StudentNumber,
                BranchCode = context.Message.BranchCode,
                BankName = context.Message.BankName,
                Amount = context.Message.Amount,
                PaymentType = context.Message.PaymentType,
                Status = "Fulfilled",
                InitiationDate = context.Message.InitiationDate,
                IsFraud = context.Message.IsFraud,
                ModifiedBy = context.Message.ModifiedBy,
                FulfilmentDate = DateTime.UtcNow,
                Funder = context.Message.Funder,
            };

            var item = new PaymentInputDto
            {
                Id = context.Message.Id.ToString(),
                AccountNumber = context.Message.AccountNumber,
                StudentNumber = context.Message.StudentNumber,
                BranchCode = context.Message.BranchCode,
                BankName = context.Message.BankName,
                Amount = context.Message.Amount,
                PaymentType = context.Message.PaymentType,
                Status = updatePayment.Status,
                IsFraud = context.Message.IsFraud,
                InitiationDate = context.Message.InitiationDate.ToString("G"),
                FulfilmentDate = updatePayment.FulfilmentDate.ToString("G"),
                ModifiedBy = context.Message.ModifiedBy,
            };
            try
            {
                await _paymentRepo.CreateAsync(updatePayment, "admin-user");
                var results = await _paymentContract.AddPaymentAsync(item);
                _logger.LogInformation($"[Payment Consumer] - Payment  added to the smart contract with status: {results.BlockHash}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"[Payment Consumer] - Payment for {context.Message.StudentNumber} is not in the smart contract, however, this payment is using id: {context.Message.Id} and this is the error: {ex.Message}");
                if (ex.Message.Contains("Nonce"))
                {
                    //Retry Logic
                    int maxRetries = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            _logger.LogWarning(
                                $"[Payment Consumer] - Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                            var results = await _paymentContract.AddPaymentAsync(item);
                            _logger.LogInformation(
                                $"[Payment Consumer] - Adding data from db to smart contract in real-time for payment model complete with status: {results.BlockHash}");
                            break;
                        }
                        catch (Exception retryEx)
                        {
                            _logger.LogError($"[Payment Consumer] - Retry attempt failed with error: {retryEx.Message}");
                            if (i == maxRetries - 1)
                            {
                                // If it's the last attempt and it fails, log the final error
                                _logger.LogError($"[Payment Consumer] - Payment for {item.StudentNumber} is not in the smart contract, however, this payment is using id: {item.Id} and this is the final error after retries: {ex}");
                            }
                        }
                    }
                }
            }
        }
    }
}

public class FunderDbConsumer : IConsumer<FunderModel>
{
    private readonly FunderContract _funderContract;
    private readonly ILogger<FunderDbConsumer> _logger;

    public FunderDbConsumer(ILogger<FunderDbConsumer> logger,  FunderContract funderContract)
    {
        _funderContract = funderContract;
        _logger = logger;           
    }
    
    public async Task Consume(ConsumeContext<FunderModel> context)
    {
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
        var isFunderOnContract = await _funderContract.GetFunderByNameAsync(item.Name);
        if (isFunderOnContract?.Name == "")
        {
            try
            {
                var results = await _funderContract.AddFunderAsync(item);
                _logger.LogInformation($"Adding data from db to smart contract in real-time for funder model complete with status: {results.BlockHash}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Funder using name {item.Name} is not in the smart contract, however, this funder is using id: {item.Id} and this is the error: {ex}");
                if (ex.Message.Contains("Nonce"))
                {
                    //Retry Logic
                    int maxRetries = 5;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            _logger.LogWarning(
                                $"Nonce error detected, Retrying transaction. Attempt {i + 1} of {maxRetries}");
                            var results = await _funderContract.AddFunderAsync(item);
                            _logger.LogInformation(
                                $"Adding data from db to smart contract in real-time for funder model complete with status: {results.BlockHash}");
                            break;
                        }
                        catch (Exception retryEx)
                        {
                            _logger.LogError($"Retry attempt failed with error: {retryEx.Message}");
                            if (i == maxRetries - 1)
                            {
                                // If it's the last attempt and it fails, log the final error
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
}


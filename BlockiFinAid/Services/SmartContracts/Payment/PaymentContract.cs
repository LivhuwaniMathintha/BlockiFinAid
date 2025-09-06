

using BlockiFinAid.Data.Responses;
using BlockiFinAid.Services.MachineLearning;
using Refit;

namespace BlockiFinAid.Services.SmartContracts.Payment;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Helpers;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

public class PaymentOutputDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; }
    public string StudentNumber { get; set; }
    public string BranchCode { get; set; }
    public string BankName { get; set; }
    public uint Amount { get; set; }
    public string PaymentType { get; set; }
    public string Status { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsFraud { get; set; }
    public string InitiationDate { get; set; }
    public string FulfilmentDate { get; set; }
    public string ModifiedBy { get; set; }
}

public class PaymentInputDto
{
    public string Id { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public uint Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsFraud { get; set; }
    public string InitiationDate { get; set; } = string.Empty;
    public string FulfilmentDate { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
}

public class PaymentUpdateDto
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string FulfilmentDate { get; set; }
    public string ModifiedBy { get; set; }
}



[Function("addPayment")]
public class AddPaymentFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    
    [Parameter("string", "_accountNumber", 2)]
    public string AccountNumber { get; set; }
   
    [Parameter("string", "_studentNumber", 3)]
    public string StudentNumber { get; set; }
    
    [Parameter("string", "_branchCode", 4)]
    public string BranchCode { get; set; }
    [Parameter("string", "_bankName", 5)]
    public string BankName { get; set; }
    
    [Parameter("uint", "_amount", 6)]
    public uint Amount { get; set; }
    
    [Parameter("string", "_paymentType", 7)]
    public string PaymentType { get; set; }
    
    [Parameter("string", "_status", 8)]
    public string Status { get; set; }
    
    [Parameter("string", "_updatedAt", 9)]
    public string UpdatedAt { get; set; }
    
    [Parameter("bool", "_isFraud", 10)]
    public bool IsFraud { get; set; }
    
    [Parameter("string", "_initiationDate", 11)]
    public string InitiationDate { get; set; }
    
    [Parameter("string", "_fulfilmentDate", 12)]
    public string FulfilmentDate { get; set; }
    
    [Parameter("string", "_modifiedBy", 13)]
    public string ModifiedBy { get; set; }
}

[Function("updatePaymentById")]
public class UpdatePaymentByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "_status", 2)]
    public string Status { get; set; }
    [Parameter("string", "_fulfilmentDate", 3)]
    public string FulfilmentDate { get; set; }
    [Parameter("string", "_updatedAt", 4)]
    public string UpdatedAt { get; set; }
    [Parameter("string", "_modifiedBy", 5)]
    public string ModifiedBy { get; set; }
}

[Function("getAllPayments", typeof(GetAllPaymentsOutputDto))]
public class GetAllPaymentsFunction : FunctionMessage { }

[FunctionOutput]
public class GetAllPaymentsOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }
    [Parameter("string[]", "accountNumbers", 2)]
    public List<string> AccountNumbers { get; set; }
    [Parameter("string[]", "studentNumbers", 3)]
    public List<string> StudentNumbers { get; set; }
  
    [Parameter("string[]", "bankNames", 4)]
    public List<string> BankNames { get; set; }
    [Parameter("uint[]", "amounts", 5)]
    public List<uint> Amounts { get; set; }
    [Parameter("string[]", "paymentTypes", 6)]
    public List<string> PaymentTypes { get; set; }
    [Parameter("string[]", "statuses", 7)]
    public List<string> Statuses { get; set; }
    [Parameter("string[]", "updatedAts", 8)]
    public List<string> UpdatedAts { get; set; }
    [Parameter("bool[]", "isFrauds", 9)]
    public List<bool> IsFrauds { get; set; }
    [Parameter("string[]", "initiationDates", 10)]
    public List<string> FulfilmentDates { get; set; }
    [Parameter("string[]", "modifiedBys", 11)]
    public List<string> ModifiedBys { get; set; }
}

[Function("getPaymentByStudentNumber", typeof(GetPaymentByStudentNumberOutputDto))]
public class GetPaymentByStudentNumberFunction : FunctionMessage
{
    [Parameter("string", "_studentNumber", 1)]
    public string StudentNumber { get; set; }
}

[FunctionOutput]
public class GetPaymentByStudentNumberOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "accountNumber", 2)]
    public string AccountNumber { get; set; }
    [Parameter("string", "studentNumber", 3)]
    public string StudentNumber { get; set; }
    [Parameter("string", "branchCode", 4)]
    public string BranchCode { get; set; }
    [Parameter("string", "bankName", 5)]
    public string BankName { get; set; }
    [Parameter("uint", "amount", 6)]
    public uint Amount { get; set; }
    [Parameter("string", "paymentType", 7)]
    public string PaymentType { get; set; }
    [Parameter("string", "status", 8)]
    public string Status { get; set; }
    [Parameter("string", "updatedAt", 9)]
    public string UpdatedAt { get; set; }
    [Parameter("bool", "isFraud", 10)]
    public bool IsFraud { get; set; }
    [Parameter("string", "initiationDate", 11)]
    public string InitiationDate { get; set; }
    [Parameter("string", "fulfilmentDate", 12)]
    public string FulfilmentDate { get; set; }
    [Parameter("string", "modifiedBy", 13)]
    public string ModifiedBy { get; set; }
}

public class PaymentContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;
    private readonly SmartContractByteCodes _byteCodes;
    private readonly IMachineLearningAPI _mlApi;
    private readonly ILogger<PaymentContract> _logger;

    public PaymentContract(IOptions<EthereumNodeSettings> nodeSettings, 
        IOptions<SmartContractAddressSettings> addressSettings, 
        IOptions<SmartContractByteCodes> byteCodes, 
        IMachineLearningAPI mlApi,
        ILogger<PaymentContract> logger)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        var account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(account, _nodeSettings.RpcUrl);
        _byteCodes = byteCodes.Value;
        _mlApi = mlApi;
        _logger = logger;
    }

    public async Task<SmartContractResponse<string>> AddPaymentAsync(PaymentInputDto dto)
    {
        bool isContinue = false, apiCallSucceeded = false;
        try
        {
            var mlRequest = new MachineLearningRequest(dto.Amount, "NSFAS", dto.PaymentType,
                DateTime.Parse(dto.InitiationDate), false);
            
            ApiResponse<MachineLearningResponse> mlResponse = await _mlApi.ProcessMachineLearningRequest(mlRequest);
            _logger.LogInformation($"ML Endpoint has response status code: {mlResponse.StatusCode}");
            if (mlResponse.IsSuccessStatusCode)
            {
                isContinue = true;
                apiCallSucceeded = true;
                
                //Add payment to training data
            }

            {
                isContinue = true;
                apiCallSucceeded = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while trying to access the Machine Learning API: {ex.Message}");
        }

        var function = new AddPaymentFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            AccountNumber = dto.AccountNumber,
            StudentNumber = dto.StudentNumber,
            BranchCode = dto.BranchCode,
            BankName = dto.BankName,
            Amount = dto.Amount,
            PaymentType = dto.PaymentType,
            Status = dto.Status,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            IsFraud = dto.IsFraud,
            InitiationDate = dto.InitiationDate.ToString(),
            FulfilmentDate = dto.FulfilmentDate.ToString(),
            ModifiedBy = dto.ModifiedBy,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddPaymentFunction>();
        if (apiCallSucceeded && isContinue)
        {
            var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.PaymentsContractAddress, function);
            var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Payment");

            if (receipt.HasErrors().Value)
            {
                return new SmartContractResponse<string>();
            }

            return new SmartContractResponse<string>()
            {
                BlockHash = receipt.BlockHash,
                BlockNumber = receipt.BlockNumber,
                ByteCode = _byteCodes.BankAccountByteCode,
                FunctionSigHashes = functionsSigHashes,
                ContractAddress = receipt.ContractAddress,
                Data = receipt.TransactionHash,
                IsSuccess = true
            };
        }
        return new SmartContractResponse<string>();
    }

    public async Task<SmartContractResponse<string>> UpdatePaymentByIdAsync(PaymentUpdateDto dto)
    {
        var function = new UpdatePaymentByIdFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            Status = dto.Status,
            FulfilmentDate = dto.FulfilmentDate,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            ModifiedBy = dto.ModifiedBy,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdatePaymentByIdFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.PaymentsContractAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Funding");

        if (receipt.HasErrors().Value)
        {
            return new SmartContractResponse<string>();
        }
        
        return new SmartContractResponse<string>()
        {
            BlockHash = receipt.BlockHash,
            BlockNumber = receipt.BlockNumber,
            ByteCode = _byteCodes.BankAccountByteCode,
            FunctionSigHashes = functionsSigHashes,
            ContractAddress = receipt.ContractAddress,
            Data = receipt.TransactionHash,
            IsSuccess = true
        };
    }

    public async Task<List<PaymentOutputDto>> GetAllPaymentsAsync()
    {
        
        
        var function = new GetAllPaymentsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.PaymentsContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllPaymentsFunction, GetAllPaymentsOutputDto>(function);
        var payments = new List<PaymentOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            payments.Add(new PaymentOutputDto
            {
                Id = new Guid(result.Ids[i]),
                AccountNumber = result.AccountNumbers[i],
                StudentNumber = result.StudentNumbers[i],
                BankName = result.BankNames[i],
                Amount = result.Amounts[i],
                PaymentType = result.PaymentTypes[i],
                Status = result.Statuses[i],
                UpdatedAt = result.UpdatedAts[i],
                IsFraud = result.IsFrauds[i],
                FulfilmentDate = result.FulfilmentDates[i],
                ModifiedBy = result.ModifiedBys[i]
            });
        }
        return payments;
    }

    public async Task<PaymentOutputDto?> GetPaymentByStudentNumberAsync(string studentNumber)
    {
        var function = new GetPaymentByStudentNumberFunction
        {
            StudentNumber = studentNumber
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.PaymentsContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetPaymentByStudentNumberFunction, GetPaymentByStudentNumberOutputDto>(function);

        if (result is not null)
        {
            return new PaymentOutputDto
            {
                Id = new Guid(result.Id),
                AccountNumber = result.AccountNumber,
                StudentNumber = result.StudentNumber,
                BranchCode = result.BranchCode,
                BankName = result.BankName,
                Amount = result.Amount,
                PaymentType = result.PaymentType,
                Status = result.Status,
                UpdatedAt = result.UpdatedAt,
                IsFraud = result.IsFraud,
                InitiationDate = result.InitiationDate,
                FulfilmentDate = result.FulfilmentDate,
                ModifiedBy = result.ModifiedBy
            };
        }

        return null;
    }
}

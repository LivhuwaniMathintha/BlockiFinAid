using BlockiFinAid.Data.Responses;

namespace BlockiFinAid.Services.SmartContracts.BankAccount;

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

public class BankAccountOutputDto
{
    public Guid Id { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankName { get; set; }
    public string BankBranchCode { get; set; }
    public bool IsConfirmed { get; set; }
    public string StudentNumber { get; set; }
    public Guid DataConfirmedById { get; set; }
    public string UpdatedAt { get; set; }
}

public class BankAccountInputDto
{
    public string Id { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankName { get; set; }
    public string BankBranchCode { get; set; }
    public string StudentNumber { get; set; }
    public string DataConfirmedById { get; set; }
}

public class BankAccountUpdateDto
{
    public string BankAccountNumber { get; set; }
    public string BankName { get; set; }
    public string BankBranchCode { get; set; }
    public bool IsConfirmed { get; set; }
    public string StudentNumber { get; set; }
    public string DataConfirmedById { get; set; }
}

[Function("getByBankAccountNumber", typeof(GetByBankAccountNumberOutputDto))]
public class GetByBankAccountNumberFunction : FunctionMessage
{
    [Parameter("string", "_bankAccountNumber", 1)]
    public string BankAccountNumber { get; set; }
}

[FunctionOutput]
public class GetByBankAccountNumberOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "bankAccountNumber", 2)]
    public string BankAccountNumber { get; set; }
    [Parameter("string", "bankName", 3)]
    public string BankName { get; set; }
    [Parameter("string", "bankBranchCode", 4)]
    public string BankBranchCode { get; set; }
    [Parameter("bool", "isConfirmed", 5)]
    public bool IsConfirmed { get; set; }
    [Parameter("string", "studentNumber", 6)]
    public string StudentNumber { get; set; }
    [Parameter("bytes16", "dataConfirmedById", 7)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("addBankAccount")]
public class AddBankAccountFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "_bankAccountNumber", 2)]
    public string BankAccountNumber { get; set; }
    [Parameter("string", "_bankName", 3)]
    public string BankName { get; set; }
    [Parameter("string", "_bankBranchCode", 4)]
    public string BankBranchCode { get; set; }
    [Parameter("bool", "_isConfirmed", 5)]
    public bool IsConfirmed { get; set; }
    [Parameter("string", "_studentNumber", 6)]
    public string StudentNumber { get; set; }
    [Parameter("bytes16", "_dataConfirmedById", 7)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "_updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("updateBankAccountByNumber")]
public class UpdateBankAccountByNumberFunction : FunctionMessage
{
    [Parameter("string", "_bankAccountNumber", 1)]
    public string BankAccountNumber { get; set; }
    [Parameter("string", "_bankName", 2)]
    public string BankName { get; set; }
    [Parameter("string", "_bankBranchCode", 3)]
    public string BankBranchCode { get; set; }
    [Parameter("bool", "_isConfirmed", 4)]
    public bool IsConfirmed { get; set; }
    [Parameter("string", "_studentNumber", 5)]
    public string StudentNumber { get; set; }
    [Parameter("bytes16", "_dataConfirmedById", 6)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "_updatedAt", 7)]
    public string UpdatedAt { get; set; }
}

[Function("studentDataChangeConfirmation")]
public class StudentDataChangeConfirmationFunction : FunctionMessage
{
    [Parameter("string", "_bankAccountNumber", 1)]
    public string BankAccountNumber { get; set; }
    [Parameter("bool", "_isConfirmed", 2)]
    public bool IsConfirmed { get; set; }
    [Parameter("string", "_updatedAt", 3)]
    public string UpdatedAt { get; set; }
    
    [Parameter("bytes16", "_dataConfirmedById", 4)]
    public byte[] DataConfirmedById { get; set; }
    
}

[Function("getByStudentNumber", typeof(GetByStudentNumberOutputDto))]
public class GetByStudentNumberFunction : FunctionMessage
{
    [Parameter("string", "_studentNumber", 1)]
    public string StudentNumber { get; set; }
}

[FunctionOutput]
public class GetByStudentNumberOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "bankAccountNumber", 2)]
    public string BankAccountNumber { get; set; }
    [Parameter("string", "bankName", 3)]
    public string BankName { get; set; }
    [Parameter("string", "bankBranchCode", 4)]
    public string BankBranchCode { get; set; }
    [Parameter("bool", "isConfirmed", 5)]
    public bool IsConfirmed { get; set; }
    [Parameter("string", "studentNumber", 6)]
    public string StudentNumber { get; set; }
    [Parameter("bytes16", "dataConfirmedById", 7)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("getAll", typeof(GetAllBankAccountsOutputDto))]
public class GetAllBankAccountsFunction : FunctionMessage { }

[FunctionOutput]
public class GetAllBankAccountsOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }
    [Parameter("string[]", "bankAccountNumbers", 2)]
    public List<string> BankAccountNumbers { get; set; }
    [Parameter("string[]", "bankNames", 3)]
    public List<string> BankNames { get; set; }
    [Parameter("string[]", "bankBranchCodes", 4)]
    public List<string> BankBranchCodes { get; set; }
    [Parameter("bool[]", "isConfirmeds", 5)]
    public List<bool> IsConfirmeds { get; set; }
    [Parameter("string[]", "studentNumbers", 6)]
    public List<string> StudentNumbers { get; set; }
    [Parameter("bytes16[]", "dataConfirmedByIds", 7)]
    public List<byte[]> DataConfirmedByIds { get; set; }
    [Parameter("string[]", "updatedAts", 8)]
    public List<string> UpdatedAts { get; set; }
}

public class BankAccountContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;
    private readonly SmartContractByteCodes _byteCodes;

    public BankAccountContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings, IOptions<SmartContractByteCodes> byteCodes)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        var account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(account, _nodeSettings.RpcUrl);
        _byteCodes = byteCodes.Value;
    }

    public async Task<SmartContractResponse<string>> AddBankAccountAsync(BankAccountInputDto dto)
    {
        var function = new AddBankAccountFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            BankAccountNumber = dto.BankAccountNumber,
            BankName = dto.BankName,
            BankBranchCode = dto.BankBranchCode,
            IsConfirmed = false,
            StudentNumber = dto.StudentNumber,
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.DataConfirmedById),
            UpdatedAt = DateTime.UtcNow.ToString("g"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddBankAccountFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.BankAccountAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("BankAccount");
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

    public async Task<BankAccountOutputDto> GetByBankAccountNumberAsync(string bankAccountNumber)
    {
        var function = new GetByBankAccountNumberFunction
        {
            BankAccountNumber = bankAccountNumber
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.BankAccountAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetByBankAccountNumberFunction, GetByBankAccountNumberOutputDto>(function);

        return new BankAccountOutputDto
        {
            Id = new Guid(result.Id),
            BankAccountNumber = result.BankAccountNumber,
            BankName = result.BankName,
            BankBranchCode = result.BankBranchCode,
            IsConfirmed = result.IsConfirmed,
            StudentNumber = result.StudentNumber,
            DataConfirmedById = new Guid(result.DataConfirmedById),
            UpdatedAt = result.UpdatedAt
        };
    }

    public async Task<SmartContractResponse<string>> UpdateBankAccountByNumberAsync(BankAccountUpdateDto dto)
    {
        var function = new UpdateBankAccountByNumberFunction
        {
            BankAccountNumber = dto.BankAccountNumber,
            BankName = dto.BankName,
            BankBranchCode = dto.BankBranchCode,
            IsConfirmed = dto.IsConfirmed,
            StudentNumber = dto.StudentNumber,
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.DataConfirmedById),
            UpdatedAt = DateTime.UtcNow.ToString("g"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateBankAccountByNumberFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.BankAccountAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("BankAccount");

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

    public async Task<string> StudentDataChangeConfirmationAsync(BankAccountConfirmationDto dto)
    {
        var function = new StudentDataChangeConfirmationFunction
        {
            BankAccountNumber = dto.AccountNumber,
            IsConfirmed = dto.IsConfirmed,
            UpdatedAt = DateTime.UtcNow.ToString("g"),
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.UserId),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<StudentDataChangeConfirmationFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.BankAccountAddress, function);
        var txHash = receipt?.TransactionHash;
        if (txHash == null)
        {
            return receipt?.HasErrors()?.ToString();
        }
        return txHash.ToString();
    }

    public async Task<BankAccountOutputDto?> GetByStudentNumberAsync(string studentNumber)
    {
        var function = new GetByStudentNumberFunction
        {
            StudentNumber = studentNumber
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.BankAccountAddress);
        try
        {
            var result =
                await contract.QueryDeserializingToObjectAsync<GetByStudentNumberFunction, GetByStudentNumberOutputDto>(
                    function);



            if (result is not null)
            {
                return new BankAccountOutputDto
                {
                    Id = new Guid(result.Id),
                    BankAccountNumber = result.BankAccountNumber,
                    BankName = result.BankName,
                    BankBranchCode = result.BankBranchCode,
                    IsConfirmed = result.IsConfirmed,
                    StudentNumber = result.StudentNumber,
                    DataConfirmedById = new Guid(result.DataConfirmedById),
                    UpdatedAt = result.UpdatedAt
                };
            }
        }
        catch (Exception e)
        {
            return null;
        }

        return null;
    }

    public async Task<List<BankAccountOutputDto>> GetAllAsync()
    {
        var function = new GetAllBankAccountsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.BankAccountAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllBankAccountsFunction, GetAllBankAccountsOutputDto>(function);
        var accounts = new List<BankAccountOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            accounts.Add(new BankAccountOutputDto
            {
                Id = new Guid(result.Ids[i]),
                BankAccountNumber = result.BankAccountNumbers[i],
                BankName = result.BankNames[i],
                BankBranchCode = result.BankBranchCodes[i],
                IsConfirmed = result.IsConfirmeds[i],
                StudentNumber = result.StudentNumbers[i],
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                UpdatedAt = result.UpdatedAts[i]
            });
        }
        return accounts;
    }
}

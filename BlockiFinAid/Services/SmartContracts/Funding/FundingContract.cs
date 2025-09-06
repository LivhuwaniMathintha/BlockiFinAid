using BlockiFinAid.Data.Responses;

namespace BlockiFinAid.Services.SmartContracts.Funding;

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

public class FundingOutputDto
{
    public Guid Id { get; set; }
    public Guid FunderId { get; set; }
    public Guid StudentId { get; set; }
    public Guid FunderContractConditionId { get; set; }
    public Guid DataConfirmedById { get; set; }
    public string SignedOn { get; set; }
    public bool IsActive { get; set; }
    public uint FoodBalance { get; set; }
    public uint TuitionBalance { get; set; }
    public uint LaptopBalance { get; set; }
    public uint AccommodationBalance { get; set; }
    public string ModifiedBy { get; set; }
    public string UpdatedAt { get; set; }
}

public class FundingWebResponse : FundingOutputDto
{
    public string Funder { get; set; } = string.Empty;
    public string PaymentDate { get; set; } = string.Empty;
    public FundingConditionOutputDto? FundingCondition { get; set; }
    
}

public class FundingInputDto
{
    public string FunderId { get; set; }
    public string StudentId { get; set; }
    public string FunderContractConditionId { get; set; }
    public string DataConfirmedById { get; set; }
    public string SignedOn { get; set; }
    public uint FoodBalance { get; set; }
    public uint TuitionBalance { get; set; }
    public uint LaptopBalance { get; set; }
    public uint AccommodationBalance { get; set; }
    public string ModifiedBy { get; set; }
}

public class FundingUpdateDto
{
    public string Id { get; set; }
    public bool IsActive { get; set; }
    public uint FoodBalance { get; set; }
    public uint TuitionBalance { get; set; }
    public uint LaptopBalance { get; set; }
    public uint AccommodationBalance { get; set; }
    public string ModifiedBy { get; set; }

}

public class FundingDataConfirmedByIdUpdateDto
{
    public string Id { get; set; }
    public string DataConfirmedById { get; set; }
    public string ModifiedBy { get; set; }
}

[Function("addFunding")]
public class AddFundingFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bytes16", "_funderId", 2)]
    public byte[] FunderId { get; set; }
    [Parameter("bytes16", "_studentId", 3)]
    public byte[] StudentId { get; set; }
    [Parameter("bytes16", "_funderContractConditionId", 4)]
    public byte[] FunderContractConditionId { get; set; }
    [Parameter("bytes16", "_dataConfirmedById", 5)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "_signedOn", 6)]
    public string SignedOn { get; set; }
    [Parameter("bool", "_isActive", 7)]
    public bool IsActive { get; set; }
    [Parameter("uint", "_foodBalance", 8)]
    public uint FoodBalance { get; set; }
    [Parameter("uint", "_tuitionBalance", 9)]
    public uint TuitionBalance { get; set; }
    [Parameter("uint", "_laptopBalance", 10)]
    public uint LaptopBalance { get; set; }
    [Parameter("uint", "_accommodationBalance", 11)]
    public uint AccommodationBalance { get; set; }
    [Parameter("string", "_modifiedBy", 12)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 13)]
    public string UpdatedAt { get; set; }
}

[Function("updateFunding")]
public class UpdateFundingFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bool", "_isActive", 2)]
    public bool IsActive { get; set; }
    [Parameter("uint", "_foodBalance", 3)]
    public uint FoodBalance { get; set; }
    [Parameter("uint", "_tuitionBalance", 4)]
    public uint TuitionBalance { get; set; }
    [Parameter("uint", "_laptopBalance", 5)]
    public uint LaptopBalance { get; set; }
    [Parameter("uint", "_accommodationBalance", 6)]
    public uint AccommodationBalance { get; set; }
    [Parameter("string", "_modifiedBy", 7)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("updateFundingDataConfirmedById")]
public class UpdateFundingDataConfirmedByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bytes16", "_dataConfirmedById", 2)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "_modifiedBy", 3)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 4)]
    public string UpdatedAt { get; set; }
}

[Function("getAllFundings", typeof(GetAllFundingsOutputDto))]
public class GetAllFundingsFunction : FunctionMessage { }

[FunctionOutput]
public class GetAllFundingsOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }
    [Parameter("bytes16[]", "funderIds", 2)]
    public List<byte[]> FunderIds { get; set; }
    [Parameter("bytes16[]", "studentIds", 3)]
    public List<byte[]> StudentIds { get; set; }
    [Parameter("bytes16[]", "funderContractConditionIds", 4)]
    public List<byte[]> FunderContractConditionIds { get; set; }
    [Parameter("bytes16[]", "dataConfirmedByIds", 5)]
    public List<byte[]> DataConfirmedByIds { get; set; }
    [Parameter("string[]", "signedOns", 6)]
    public List<string> SignedOns { get; set; }
    [Parameter("bool[]", "isActives", 7)]
    public List<bool> IsActives { get; set; }
    [Parameter("uint[]", "foodBalances", 8)]
    public List<uint> FoodBalances { get; set; }
    [Parameter("uint[]", "tuitionBalances", 9)]
    public List<uint> TuitionBalances { get; set; }
    [Parameter("uint[]", "laptopBalances", 10)]
    public List<uint> LaptopBalances { get; set; }
    [Parameter("uint[]", "accommodationBalances", 11)]
    public List<uint> AccommodationBalances { get; set; }
    [Parameter("string[]", "modifiedBys", 12)]
    public List<string> ModifiedBys { get; set; }
    [Parameter("string[]", "updatedAts", 13)]
    public List<string> UpdatedAts { get; set; }
}

[Function("getFundingByStudentId", typeof(GetFundingByStudentIdOutputDto))]
public class GetFundingByStudentIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_studentId", 1)]
    public byte[] StudentId { get; set; }
}

[FunctionOutput]
public class GetFundingByStudentIdOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bytes16", "funderId", 2)]
    public byte[] FunderId { get; set; }
    [Parameter("bytes16", "studentId", 3)]
    public byte[] StudentId { get; set; }
    [Parameter("bytes16", "funderContractConditionId", 4)]
    public byte[] FunderContractConditionId { get; set; }
    [Parameter("bytes16", "dataConfirmedById", 5)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "signedOn", 6)]
    public string SignedOn { get; set; }
    [Parameter("bool", "isActive", 7)]
    public bool IsActive { get; set; }
    [Parameter("uint", "foodBalance", 8)]
    public uint FoodBalance { get; set; }
    [Parameter("uint", "tuitionBalance", 9)]
    public uint TuitionBalance { get; set; }
    [Parameter("uint", "laptopBalance", 10)]
    public uint LaptopBalance { get; set; }
    [Parameter("uint", "accommodationBalance", 11)]
    public uint AccommodationBalance { get; set; }
    [Parameter("string", "modifiedBy", 12)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "updatedAt", 13)]
    public string UpdatedAt { get; set; }
}

[Function("getAllInactiveFundings", typeof(GetAllFundingsOutputDto))]
public class GetAllInactiveFundingsFunction : FunctionMessage { }

[Function("getInactiveFundingsByStudentId", typeof(GetAllFundingsOutputDto))]
public class GetInactiveFundingsByStudentIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_studentId", 1)]
    public byte[] StudentId { get; set; }
}

public class FundingContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;
    private readonly SmartContractByteCodes _byteCodes;

    public FundingContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings, IOptions<SmartContractByteCodes> byteCodes)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        var account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(account, _nodeSettings.RpcUrl);
        _byteCodes = byteCodes.Value;
    }

    public async Task<SmartContractResponse<FundingOutputDto>> AddFundingAsync(FundingInputDto dto)
    {
        var function = new AddFundingFunction
        {
            Id = Guid.NewGuid().ToByteArray(),
            FunderId = GuidHelper.GuidToBytes16OrZero(dto.FunderId),
            StudentId = GuidHelper.GuidToBytes16OrZero(dto.StudentId),
            FunderContractConditionId = GuidHelper.GuidToBytes16OrZero(dto.FunderContractConditionId),
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.DataConfirmedById),
            SignedOn = dto.SignedOn,
            IsActive = true,
            FoodBalance = dto.FoodBalance,
            TuitionBalance = dto.TuitionBalance,
            LaptopBalance = dto.LaptopBalance,
            AccommodationBalance = dto.AccommodationBalance,
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddFundingFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingContractAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Funding");

        if (receipt.HasErrors().Value)
        {
            return new SmartContractResponse<FundingOutputDto>();
        }
        
        var allFundings = await GetFundingByStudentIdAsync(Guid.Parse(dto.StudentId));
        return new SmartContractResponse<FundingOutputDto>()
        {
            BlockHash = receipt.BlockHash,
            BlockNumber = receipt.BlockNumber,
            ByteCode = _byteCodes.BankAccountByteCode,
            FunctionSigHashes = functionsSigHashes,
            ContractAddress = receipt.ContractAddress,
            Data = null,
            IsSuccess = true
        };
    }

    public async Task<SmartContractResponse<string>> UpdateFundingAsync(FundingUpdateDto dto)
    {
        var function = new UpdateFundingFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            IsActive = dto.IsActive,
            FoodBalance = dto.FoodBalance,
            TuitionBalance = dto.TuitionBalance,
            LaptopBalance = dto.LaptopBalance,
            AccommodationBalance = dto.AccommodationBalance,
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateFundingFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingContractAddress, function);
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

    public async Task<SmartContractResponse<string>> UpdateFundingDataConfirmedByIdAsync(FundingDataConfirmedByIdUpdateDto dto)
    {
        var function = new UpdateFundingDataConfirmedByIdFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.DataConfirmedById),
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateFundingDataConfirmedByIdFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingContractAddress, function);
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

    public async Task<List<FundingOutputDto>> GetAllFundingsAsync()
    {
        var function = new GetAllFundingsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllFundingsFunction, GetAllFundingsOutputDto>(function);
        var fundings = new List<FundingOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            fundings.Add(new FundingOutputDto
            {
                Id = new Guid(result.Ids[i]),
                FunderId = new Guid(result.FunderIds[i]),
                StudentId = new Guid(result.StudentIds[i]),
                FunderContractConditionId = new Guid(result.FunderContractConditionIds[i]),
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                SignedOn = result.SignedOns[i],
                IsActive = result.IsActives[i],
                FoodBalance = result.FoodBalances[i],
                TuitionBalance = result.TuitionBalances[i],
                LaptopBalance = result.LaptopBalances[i],
                AccommodationBalance = result.AccommodationBalances[i],
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i]
            });
        }
        return fundings;
    }

    public async Task<FundingOutputDto?> GetFundingByStudentIdAsync(Guid studentId)
    {
        var function = new GetFundingByStudentIdFunction
        {
            StudentId = studentId.ToByteArray()
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetFundingByStudentIdFunction, GetFundingByStudentIdOutputDto>(function);

        if (result is not null)
        {
            return new FundingOutputDto
            {
                Id = new Guid(result.Id),
                FunderId = new Guid(result.FunderId),
                StudentId = new Guid(result.StudentId),
                FunderContractConditionId = new Guid(result.FunderContractConditionId),
                DataConfirmedById = new Guid(result.DataConfirmedById),
                SignedOn = result.SignedOn,
                IsActive = result.IsActive,
                FoodBalance = result.FoodBalance,
                TuitionBalance = result.TuitionBalance,
                LaptopBalance = result.LaptopBalance,
                AccommodationBalance = result.AccommodationBalance,
                ModifiedBy = result.ModifiedBy,
                UpdatedAt = result.UpdatedAt
            };
        }
        return null;
    }

    public async Task<List<FundingOutputDto>> GetAllInactiveFundingsAsync()
    {
        var function = new GetAllInactiveFundingsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllInactiveFundingsFunction, GetAllFundingsOutputDto>(function);
        var fundings = new List<FundingOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            fundings.Add(new FundingOutputDto
            {
                Id = new Guid(result.Ids[i]),
                FunderId = new Guid(result.FunderIds[i]),
                StudentId = new Guid(result.StudentIds[i]),
                FunderContractConditionId = new Guid(result.FunderContractConditionIds[i]),
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                SignedOn = result.SignedOns[i],
                IsActive = result.IsActives[i],
                FoodBalance = result.FoodBalances[i],
                TuitionBalance = result.TuitionBalances[i],
                LaptopBalance = result.LaptopBalances[i],
                AccommodationBalance = result.AccommodationBalances[i],
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i]
            });
        }
        return fundings;
    }

    public async Task<List<FundingOutputDto>> GetInactiveFundingsByStudentIdAsync(Guid studentId)
    {
        var function = new GetInactiveFundingsByStudentIdFunction
        {
            StudentId = studentId.ToByteArray()
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetInactiveFundingsByStudentIdFunction, GetAllFundingsOutputDto>(function);
        var fundings = new List<FundingOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            fundings.Add(new FundingOutputDto
            {
                Id = new Guid(result.Ids[i]),
                FunderId = new Guid(result.FunderIds[i]),
                StudentId = new Guid(result.StudentIds[i]),
                FunderContractConditionId = new Guid(result.FunderContractConditionIds[i]),
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                SignedOn = result.SignedOns[i],
                IsActive = result.IsActives[i],
                FoodBalance = result.FoodBalances[i],
                TuitionBalance = result.TuitionBalances[i],
                LaptopBalance = result.LaptopBalances[i],
                AccommodationBalance = result.AccommodationBalances[i],
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i]
            });
        }
        return fundings;
    }
}

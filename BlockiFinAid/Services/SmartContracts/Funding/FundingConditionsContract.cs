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

public class FundingConditionOutputDto
{
    public Guid Id { get; set; }
    public bool IsFullCoverage { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public uint TotalAmount { get; set; }
    public uint FoodAmount { get; set; }
    public uint TuitionAmount { get; set; }
    public uint LaptopAmount { get; set; }
    public uint AccommodationAmount { get; set; }
    public bool AccommodationDirectPay { get; set; }
    public Guid DataConfirmedById { get; set; }
    public string ModifiedBy { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public uint AverageMark { get; set; }
}

public class FundingConditionInputDto
{
    public string Id { get; set; }
    public bool IsFullCoverage { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public uint TotalAmount { get; set; }
    public uint FoodAmount { get; set; }
    public uint TuitionAmount { get; set; }
    public uint LaptopAmount { get; set; }
    public uint AccommodationAmount { get; set; }
    public bool AccommodationDirectPay { get; set; }
    public string ModifiedBy { get; set; }
    public string UpdatedAt { get; set; }
    public uint AverageMark { get; set; }
}

public class FundingConditionUpdateDto
{
    public string Id { get; set; }
    public bool IsActive { get; set; }
    public uint AverageMark { get; set; }
    public string ModifiedBy { get; set; }
}

public class FundingConditionDataConfirmedByIdUpdateDto
{
    public string Id { get; set; }
    public string DataConfirmedById { get; set; }
    public string ModifiedBy { get; set; }
    public string UpdatedAt { get; set; }
}

[Function("addCondition")]
public class AddConditionFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bool", "_isFullCoverage", 2)]
    public bool IsFullCoverage { get; set; }
    [Parameter("string", "_startDate", 3)]
    public string StartDate { get; set; }
    [Parameter("string", "_endDate", 4)]
    public string EndDate { get; set; }
    [Parameter("uint", "_totalAmount", 5)]
    public uint TotalAmount { get; set; }
    [Parameter("uint", "_foodAmount", 6)]
    public uint FoodAmount { get; set; }
    [Parameter("uint", "_tuitionAmount", 7)]
    public uint TuitionAmount { get; set; }
    [Parameter("uint", "_laptopAmount", 8)]
    public uint LaptopAmount { get; set; }
    [Parameter("uint", "_accommodationAmount", 9)]
    public uint AccommodationAmount { get; set; }
    [Parameter("bool", "_accommodationDirectPay", 10)]
    public bool AccommodationDirectPay { get; set; }
    [Parameter("bytes16", "_dataConfirmedById", 11)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "_modifiedBy", 12)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 13)]
    public string UpdatedAt { get; set; }
    [Parameter("bool", "_isActive", 14)]
    public bool IsActive { get; set; }
    [Parameter("uint", "_averageMark", 15)]
    public uint AverageMark { get; set; }
}

[Function("updateCondition")]
public class UpdateConditionFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bool", "_isActive", 2)]
    public bool IsActive { get; set; }
    [Parameter("uint", "_averageMark", 3)]
    public uint AverageMark { get; set; }
    [Parameter("string", "_modifiedBy", 4)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 5)]
    public string UpdatedAt { get; set; }
}

[Function("updateDataConfirmedById")]
public class UpdateDataConfirmedByIdFunction : FunctionMessage
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

[Function("getAllConditions", typeof(GetAllConditionsOutputDto))]
public class GetAllConditionsFunction : FunctionMessage { }

[FunctionOutput]
public class GetAllConditionsOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }
    [Parameter("bool[]", "isFullCoverages", 2)]
    public List<bool> IsFullCoverages { get; set; }
    [Parameter("string[]", "startDates", 3)]
    public List<string> StartDates { get; set; }
    [Parameter("string[]", "endDates", 4)]
    public List<string> EndDates { get; set; }
    [Parameter("uint[]", "totalAmounts", 5)]
    public List<uint> TotalAmounts { get; set; }
    [Parameter("uint[]", "foodAmounts", 6)]
    public List<uint> FoodAmounts { get; set; }
    [Parameter("uint[]", "tuitionAmounts", 7)]
    public List<uint> TuitionAmounts { get; set; }
    [Parameter("uint[]", "laptopAmounts", 8)]
    public List<uint> LaptopAmounts { get; set; }
    [Parameter("uint[]", "accommodationAmounts", 9)]
    public List<uint> AccommodationAmounts { get; set; }
    [Parameter("bool[]", "accommodationDirectPays", 10)]
    public List<bool> AccommodationDirectPays { get; set; }
    [Parameter("bytes16[]", "dataConfirmedByIds", 11)]
    public List<byte[]> DataConfirmedByIds { get; set; }
    [Parameter("string[]", "modifiedBys", 12)]
    public List<string> ModifiedBys { get; set; }
    [Parameter("string[]", "updatedAts", 13)]
    public List<string> UpdatedAts { get; set; }
    [Parameter("bool[]", "isActives", 14)]
    public List<bool> IsActives { get; set; }
    [Parameter("uint[]", "averageMarks", 15)]
    public List<uint> AverageMarks { get; set; }
}

[Function("getConditionById", typeof(GetConditionByIdOutputDto))]
public class GetConditionByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
}

[FunctionOutput]
public class GetConditionByIdOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("bool", "isFullCoverage", 2)]
    public bool IsFullCoverage { get; set; }
    [Parameter("string", "startDate", 3)]
    public string StartDate { get; set; }
    [Parameter("string", "endDate", 4)]
    public string EndDate { get; set; }
    [Parameter("uint", "totalAmount", 5)]
    public uint TotalAmount { get; set; }
    [Parameter("uint", "foodAmount", 6)]
    public uint FoodAmount { get; set; }
    [Parameter("uint", "tuitionAmount", 7)]
    public uint TuitionAmount { get; set; }
    [Parameter("uint", "laptopAmount", 8)]
    public uint LaptopAmount { get; set; }
    [Parameter("uint", "accommodationAmount", 9)]
    public uint AccommodationAmount { get; set; }
    [Parameter("bool", "accommodationDirectPay", 10)]
    public bool AccommodationDirectPay { get; set; }
    [Parameter("bytes16", "dataConfirmedById", 11)]
    public byte[] DataConfirmedById { get; set; }
    [Parameter("string", "modifiedBy", 12)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "updatedAt", 13)]
    public string UpdatedAt { get; set; }
    [Parameter("bool", "isActive", 14)]
    public bool IsActive { get; set; }
    [Parameter("uint", "averageMark", 15)]
    public uint AverageMark { get; set; }
}

[Function("getAllInactiveConditions", typeof(GetAllConditionsOutputDto))]
public class GetAllInactiveConditionsFunction : FunctionMessage { }

public class FundingConditionsContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;
    private readonly SmartContractByteCodes _byteCodes;

    public FundingConditionsContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings, IOptions<SmartContractByteCodes> byteCode)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        var account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(account, _nodeSettings.RpcUrl);
        _byteCodes = byteCode.Value;
    }

    public async Task<SmartContractResponse<string>> AddConditionAsync(FundingConditionInputDto dto)
    {
        var function = new AddConditionFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            IsFullCoverage = dto.IsFullCoverage,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalAmount = dto.TotalAmount,
            FoodAmount = dto.FoodAmount,
            TuitionAmount = dto.TuitionAmount,
            LaptopAmount = dto.LaptopAmount,
            AccommodationAmount = dto.AccommodationAmount,
            AccommodationDirectPay = dto.AccommodationDirectPay,
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero("invalid"),
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = dto.UpdatedAt,
            IsActive = true,
            AverageMark = dto.AverageMark,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddConditionFunction>();
        try
        {

            var receipt =
                await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingConditionsContractAddress,
                    function);
            var functionsSigHashes =
                FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Condition");

            
            return new SmartContractResponse<string>()
            {
                BlockHash = receipt.BlockHash,
                BlockNumber = receipt.BlockNumber,
                ByteCode = _byteCodes.BankAccountByteCode,
                FunctionSigHashes = functionsSigHashes,
                ContractAddress = receipt.ContractAddress,
                Data = Guid.Parse(dto.Id).ToString(),
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            var response = new SmartContractResponse<string>();
            response.Data = ex.Message;
            return response;
        }
    }

    public async Task<SmartContractResponse<string>> UpdateConditionAsync(FundingConditionUpdateDto dto)
    {
        var function = new UpdateConditionFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            IsActive = dto.IsActive,
            AverageMark = dto.AverageMark,
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateConditionFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingConditionsContractAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Condition");

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

    public async Task<string> UpdateDataConfirmedByIdAsync(FundingConditionDataConfirmedByIdUpdateDto dto)
    {
        var function = new UpdateDataConfirmedByIdFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            DataConfirmedById = GuidHelper.GuidToBytes16OrZero(dto.DataConfirmedById),
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = dto.UpdatedAt,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateDataConfirmedByIdFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FundingConditionsContractAddress, function);
        var txHash = receipt?.TransactionHash;
        if (txHash == null)
        {
            return receipt?.HasErrors()?.ToString();
        }
        return txHash.ToString();
    }

    public async Task<List<FundingConditionOutputDto>> GetAllConditionsAsync()
    {
        var function = new GetAllConditionsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingConditionsContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllConditionsFunction, GetAllConditionsOutputDto>(function);
        var conditions = new List<FundingConditionOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            conditions.Add(new FundingConditionOutputDto
            {
                Id = new Guid(result.Ids[i]),
                IsFullCoverage = result.IsFullCoverages[i],
                StartDate = result.StartDates[i],
                EndDate = result.EndDates[i],
                TotalAmount = result.TotalAmounts[i],
                FoodAmount = result.FoodAmounts[i],
                TuitionAmount = result.TuitionAmounts[i],
                LaptopAmount = result.LaptopAmounts[i],
                AccommodationAmount = result.AccommodationAmounts[i],
                AccommodationDirectPay = result.AccommodationDirectPays[i],
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i],
                IsActive = result.IsActives[i],
                AverageMark = result.AverageMarks[i]
            });
        }
        return conditions;
    }

    public async Task<FundingConditionOutputDto?> GetConditionByIdAsync(string id)
    {
        var function = new GetConditionByIdFunction
        {
            Id = Guid.Parse(id).ToByteArray()
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingConditionsContractAddress);
        try
        {
            var result =
                await contract.QueryDeserializingToObjectAsync<GetConditionByIdFunction, GetConditionByIdOutputDto>(
                    function);

            if (result.UpdatedAt != "")
            {
                return new FundingConditionOutputDto
                {
                    Id = new Guid(result.Id),
                    IsFullCoverage = result.IsFullCoverage,
                    StartDate = result.StartDate,
                    EndDate = result.EndDate,
                    TotalAmount = result.TotalAmount,
                    FoodAmount = result.FoodAmount,
                    TuitionAmount = result.TuitionAmount,
                    LaptopAmount = result.LaptopAmount,
                    AccommodationAmount = result.AccommodationAmount,
                    AccommodationDirectPay = result.AccommodationDirectPay,
                    DataConfirmedById = new Guid(result.DataConfirmedById),
                    ModifiedBy = result.ModifiedBy,
                    UpdatedAt = result.UpdatedAt,
                    IsActive = result.IsActive,
                    AverageMark = result.AverageMark
                };
            }
        }
        catch (Exception e)
        {
            return null;
        }

        return null;
    }

    public async Task<List<FundingConditionOutputDto>> GetAllInactiveConditionsAsync()
    {
        var function = new GetAllInactiveConditionsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FundingConditionsContractAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllInactiveConditionsFunction, GetAllConditionsOutputDto>(function);
        var conditions = new List<FundingConditionOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            conditions.Add(new FundingConditionOutputDto
            {
                Id = new Guid(result.Ids[i]),
                IsFullCoverage = result.IsFullCoverages[i],
                StartDate = result.StartDates[i],
                EndDate = result.EndDates[i],
                TotalAmount = result.TotalAmounts[i],
                FoodAmount = result.FoodAmounts[i],
                TuitionAmount = result.TuitionAmounts[i],
                LaptopAmount = result.LaptopAmounts[i],
                AccommodationAmount = result.AccommodationAmounts[i],
                AccommodationDirectPay = result.AccommodationDirectPays[i],
                DataConfirmedById = new Guid(result.DataConfirmedByIds[i]),
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i],
                IsActive = result.IsActives[i],
                AverageMark = result.AverageMarks[i]
            });
        }
        return conditions;
    }
}

using BlockiFinAid.Data.Responses;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;

namespace BlockiFinAid.Services.SmartContracts.Funder;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Helpers;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;


public class FunderOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string IsActive { get; set; }
    public Guid FunderContractId { get; set; }
    public bool IsChangeConfirmed { get; set; }
    public string PaymentDate { get; set; }
    public string ModifiedBy { get; set; }
    public string UpdatedAt { get; set; }
}

public class FunderInputDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string FunderContractId { get; set; }
    public bool IsChangeConfirmed { get; set; }
    public string PaymentDate { get; set; }
    public string ModifiedBy { get; set; }
}

public class FunderUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public Guid FunderContractId { get; set; }
    public bool IsChangeConfirmed { get; set; }
    public string PaymentDate { get; set; }
    public string ModifiedBy { get; set; }
  
}

[Function("getAllFunders", "tuple[]")]
public class GetAllFundersFunction : FunctionMessage { }

[Function("getFunderById", "tuple")]
public class GetFunderByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
}

[Function("getFunderByName", "tuple")]
public class GetFunderByNameFunction : FunctionMessage
{
    [Parameter("string", "_name", 1)]
    public string Name { get; set; }
}

[FunctionOutput]
public class GetAllFundersOutputDTO : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }
    [Parameter("string[]", "names", 2)]
    public List<string> Names { get; set; }
    [Parameter("string[]", "isActives", 3)]
    public List<string> IsActives { get; set; }
    [Parameter("bytes16[]", "funderContractIds", 4)]
    public List<byte[]> FunderContractIds { get; set; }
    [Parameter("bool[]", "isChangeConfirmeds", 5)]
    public List<bool> IsChangeConfirmeds { get; set; }
    [Parameter("string[]", "paymentDates", 6)]
    public List<string> PaymentDates { get; set; }
    [Parameter("string[]", "modifiedBys", 7)]
    public List<string> ModifiedBys { get; set; }
    [Parameter("string[]", "updatedAts", 8)]
    public List<string> UpdatedAts { get; set; }
}

[FunctionOutput]
public class GetFunderByIdOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "name", 2)]
    public string Name { get; set; }
    [Parameter("string", "isActive", 3)]
    public string IsActive { get; set; }
    [Parameter("bytes16", "funderContractId", 4)]
    public byte[] FunderContractId { get; set; }
    [Parameter("bool", "isChangeConfirmed", 5)]
    public bool IsChangeConfirmed { get; set; }
    [Parameter("string", "paymentDate", 6)]
    public string PaymentDate { get; set; }
    [Parameter("string", "modifiedBy", 7)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[FunctionOutput]
public class GetFunderByNameOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "name", 2)]
    public string Name { get; set; }
    [Parameter("string", "isActive", 3)]
    public string IsActive { get; set; }
    [Parameter("bytes16", "funderContractId", 4)]
    public byte[] FunderContractId { get; set; }
    [Parameter("bool", "isChangeConfirmed", 5)]
    public bool IsChangeConfirmed { get; set; }
    [Parameter("string", "paymentDate", 6)]
    public string PaymentDate { get; set; }
    [Parameter("string", "modifiedBy", 7)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("addFunder")]
public class AddFunderFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "_name", 2)]
    public string Name { get; set; }
    [Parameter("bool", "_isActive", 3)]
    public bool IsActive { get; set; }
    [Parameter("bytes16", "_funderContractId", 4)]
    public byte[] FunderContractId { get; set; }
    [Parameter("bool", "_isChangeConfirmed", 5)]
    public bool IsChangeConfirmed { get; set; }
    [Parameter("string", "_paymentDate", 6)]
    public string PaymentDate { get; set; }
    [Parameter("string", "_modifiedBy", 7)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

[Function("updateFunderById")]
public class UpdateFunderByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("string", "_name", 2)]
    public string Name { get; set; }
    [Parameter("bool", "_isActive", 3)]
    public bool IsActive { get; set; }
    [Parameter("bytes16", "_funderContractId", 4)]
    public byte[] FunderContractId { get; set; }
    [Parameter("bool", "_isChangeConfirmed", 5)]
    public bool IsChangeConfirmed { get; set; }
    [Parameter("string", "_paymentDate", 6)]
    public string PaymentDate { get; set; }
    [Parameter("string", "_modifiedBy", 7)]
    public string ModifiedBy { get; set; }
    [Parameter("string", "_updatedAt", 8)]
    public string UpdatedAt { get; set; }
}

public class FunderContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;
    private readonly SmartContractByteCodes _byteCodes;
    private readonly InMemoryNonceService _nonceService;
    private readonly Account _account;


    public FunderContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings, IOptions<SmartContractByteCodes> byteCodes)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        _account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(_account, _nodeSettings.RpcUrl);
        _byteCodes = byteCodes.Value;
        _nonceService = new InMemoryNonceService(_addressSettings.FunderAddress, _web3.Client);
    }

    public async Task<SmartContractResponse<FunderOutputDto>> AddFunderAsync(FunderInputDto dto)
    {
        var currentNonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(_account.Address, BlockParameter.CreatePending());
        await NonceManager.InitializeNonce(_web3, _account.Address);
        
        var nextNonce = NonceManager.GetNextNonce();
        
        var function = new AddFunderFunction
        {
            Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
            Name = dto.Name,
            IsActive = dto.IsActive,
            FunderContractId = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId),
            IsChangeConfirmed = dto.IsChangeConfirmed,
            PaymentDate = dto.PaymentDate,
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress,
            Nonce = new HexBigInteger(nextNonce)
        };
        
        var contract = _web3.Eth.GetContractTransactionHandler<AddFunderFunction>();
        // var signedContract = await contract.SignTransactionAsync(_addressSettings.FunderAddress, function);
        //
        // var receipt = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedContract);
        // var block = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(receipt);

        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FunderAddress, function);
    
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Funder");

        if (!receipt.Succeeded())
        {
            return new SmartContractResponse<FunderOutputDto>()
            {
                Data = null
            };
        }

        var funderId = System.Guid.Parse(dto.Id);
        var funder = await GetFunderByIdAsync(funderId);
        
        return new SmartContractResponse<FunderOutputDto>()
        {
            BlockHash = receipt.BlockHash,
            BlockNumber = receipt.BlockNumber,
            ByteCode = _byteCodes.BankAccountByteCode,
            FunctionSigHashes = functionsSigHashes,
            ContractAddress = receipt.ContractAddress,
            Data = funder,
            IsSuccess = true
        };
    }

    public async Task<List<FunderOutputDto>> GetAllFundersAsync()
    {
        var function = new GetAllFundersFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FunderAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllFundersFunction, GetAllFundersOutputDTO>(function);
        var funders = new List<FunderOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            funders.Add(new FunderOutputDto
            {
                Id = new Guid(result.Ids[i]),
                Name = result.Names[i],
                IsActive = result.IsActives[i],
                FunderContractId = new Guid(result.FunderContractIds[i]),
                IsChangeConfirmed = result.IsChangeConfirmeds[i],
                PaymentDate = result.PaymentDates[i],
                ModifiedBy = result.ModifiedBys[i],
                UpdatedAt = result.UpdatedAts[i]
            });
        }
        return funders;
    }

    public async Task<FunderOutputDto?> GetFunderByIdAsync(Guid funderId)
    {
        var function = new GetFunderByIdFunction
        {
            Id = funderId.ToByteArray()
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FunderAddress);
        try
        {
            var result =
                await contract.QueryDeserializingToObjectAsync<GetFunderByIdFunction, GetFunderByIdOutputDto>(function);
            return new FunderOutputDto
            {
                Id = new Guid(result.Id),
                Name = result.Name,
                IsActive = result.IsActive,
                FunderContractId = new Guid(result.FunderContractId),
                IsChangeConfirmed = result.IsChangeConfirmed,
                PaymentDate = result.PaymentDate,
                ModifiedBy = result.ModifiedBy,
                UpdatedAt = result.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<FunderOutputDto?> GetFunderByNameAsync(string name)
    {
        var function = new GetFunderByNameFunction
        {
            Name = name
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.FunderAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetFunderByNameFunction, GetFunderByNameOutputDto>(function);
        return new FunderOutputDto
        {
            Id = new Guid(result.Id),
            Name = result.Name,
            IsActive = result.IsActive,
            FunderContractId = new Guid(result.FunderContractId),
            IsChangeConfirmed = result.IsChangeConfirmed,
            PaymentDate = result.PaymentDate,
            ModifiedBy = result.ModifiedBy,
            UpdatedAt = result.UpdatedAt
        };
    }

    public async Task<SmartContractResponse<string>> UpdateFunderByIdAsync(FunderUpdateDto dto)
    {
        var function = new UpdateFunderByIdFunction
        {
            Id = dto.Id.ToByteArray(),
            Name = dto.Name,
            IsActive = dto.IsActive,
            FunderContractId = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId.ToString()),
            IsChangeConfirmed = dto.IsChangeConfirmed,
            PaymentDate = dto.PaymentDate,
            ModifiedBy = dto.ModifiedBy,
            UpdatedAt = DateTime.UtcNow.ToString("G"),
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateFunderByIdFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.FunderAddress, function);
        var functionsSigHashes = FunctionSignatureCalculator.GetStateChangingFunctionSignaturesByGroup("Funder");

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
}


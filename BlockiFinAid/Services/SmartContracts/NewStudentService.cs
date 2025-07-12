using System.Numerics;
using System.Runtime.InteropServices;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Helpers;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace BlockiFinAid.Services.SmartContracts;

public class StudentOutputDto
{
    public Guid Id { get; set; }
    public uint StudentNumber { get; set; } // Change from BigInteger to uint
    public Guid InstitutionId { get; set; }
    public string EnrollmentStatus { get; set; }
    public string IsActive { get; set; }
    public Guid FunderContractId { get; set; }
}

public class StudentInputDto
{
    public Guid Id { get; set; }
    public uint StudentNumber { get; set; } // Change from BigInteger to uint
    public Guid InstitutionId { get; set; }
    public string EnrollmentStatus { get; set; }
    public string IsActive { get; set; }
    public string FunderContractId { get; set; }
}

[Function("getAllStudents", "tuple[]")]
public class GetAllStudentsFunction : FunctionMessage { }

[Function("getStudentById", "tuple")]
public class GetStudentByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
}

// Output DTOs for contract function results[FunctionOutput]
[FunctionOutput]
public class GetAllStudentsOutputDTO : IFunctionOutputDTO
{
    [Parameter("bytes16[]", "ids", 1)]
    public List<byte[]> Ids { get; set; }

    [Parameter("uint32[]", "studentNumbers", 2)]
    public List<uint> StudentNumbers { get; set; }

    [Parameter("bytes16[]", "institutionIds", 3)]
    public List<byte[]> InstitutionIds { get; set; }

    [Parameter("string[]", "enrollmentStatuses", 4)]
    public List<string> EnrollmentStatuses { get; set; }

    [Parameter("string[]", "isActives", 5)]
    public List<string> IsActives { get; set; }

    [Parameter("bytes16[]", "funderContractIds", 6)]
    public List<byte[]> FunderContractIds { get; set; }
}
[FunctionOutput]
public class GetStudentByIdOutputDto : IFunctionOutputDTO
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("uint32", "studentNumber", 2)]
    public uint StudentNumber { get; set; }
    [Parameter("bytes16", "institutionId", 3)]
    public byte[] InstitutionId { get; set; }
    [Parameter("string", "enrollmentStatus", 4)]
    public string EnrollmentStatus { get; set; }
    [Parameter("string", "isActive", 5)]
    public string IsActive { get; set; }
    [Parameter("bytes16", "funderContractId", 6)]
    public byte[] FunderContractId { get; set; }
}

[Function("addStudent")]
public class AddStudentFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
    [Parameter("uint32", "_studentNumber", 2)]
    public uint StudentNumber { get; set; }
    [Parameter("bytes16", "_institutionId", 3)]
    public byte[] InstitutionId { get; set; }
    [Parameter("string", "_enrollmentStatus", 4)]
    public string EnrollmentStatus { get; set; }
    [Parameter("bool", "_isActive", 5)]
    public bool IsActive { get; set; }
    [Parameter("bytes16", "_funderContractId", 6)]
    public byte[] FunderContractId { get; set; }
}
public class NewStudentService
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;

    public NewStudentService(IOptions<EthereumNodeSettings>  nodeSettings, IOptions<SmartContractAddressSettings> addressSettings)
    {
        _nodeSettings = nodeSettings.Value;
        _addressSettings = addressSettings.Value;
        var account = new Account(_nodeSettings.PrivateKey);
        _web3 = new Web3(account, _nodeSettings.RpcUrl);
        
    }
    public async Task<string> AddStudentAsync(StudentInputDto dto)
    {
        var funderContract = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId.ToString());
        var function = new AddStudentFunction
        {
            Id = Guid.NewGuid().ToByteArray(),
            StudentNumber = dto.StudentNumber,
            InstitutionId = Guid.NewGuid().ToByteArray(),
            EnrollmentStatus = dto.EnrollmentStatus,
            IsActive = dto.IsActive == "true",
            FunderContractId = funderContract,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddStudentFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.StudentAddress, function);
        return receipt.TransactionHash;
    }
    public async Task<List<StudentOutputDto>> GetAllStudentsAsync()
    {
        var function = new GetAllStudentsFunction();
        var contract = _web3.Eth.GetContractHandler(_addressSettings.StudentAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetAllStudentsFunction, GetAllStudentsOutputDTO>(function);
        var students = new List<StudentOutputDto>();
        for (int i = 0; i < result.Ids.Count; i++)
        {
            students.Add(new StudentOutputDto
            {
                Id = new Guid(result.Ids[i]),
                StudentNumber = result.StudentNumbers[i],
                InstitutionId = new Guid(result.InstitutionIds[i]),
                EnrollmentStatus = result.EnrollmentStatuses[i],
                IsActive = result.IsActives[i],
                FunderContractId = new Guid(result.FunderContractIds[i])
            });
        }
        return students;
    }
   
}
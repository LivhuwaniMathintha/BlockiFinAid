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

namespace BlockiFinAid.Services.SmartContracts.Student;

public class StudentOutputDto
{
    public Guid Id { get; set; }
    public uint StudentNumber { get; set; }
    public Guid InstitutionId { get; set; }
    public string EnrollmentStatus { get; set; }
    public string IsActive { get; set; }
    public Guid FunderContractId { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
    public bool IsChangeConfirmed { get; set; }
}

public class StudentInputDto
{
    public Guid Id { get; set; }
    public uint StudentNumber { get; set; }
    public Guid InstitutionId { get; set; }
    public string EnrollmentStatus { get; set; }
    public string IsActive { get; set; }
    public string FunderContractId { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
    public bool IsChangeConfirmed { get; set; }
}

[Function("getAllStudents", "tuple[]")]
public class GetAllStudentsFunction : FunctionMessage { }

[Function("getStudentById", "tuple")]
public class GetStudentByIdFunction : FunctionMessage
{
    [Parameter("bytes16", "_id", 1)]
    public byte[] Id { get; set; }
}

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

    [Parameter("string[]", "createdAts", 7)]
    public List<string> CreatedAts { get; set; }

    [Parameter("string[]", "updatedOns", 8)]
    public List<string> UpdatedOns { get; set; }

    [Parameter("string[]", "createdBys", 9)]
    public List<string> CreatedBys { get; set; }

    [Parameter("string[]", "modifiedBys", 10)]
    public List<string> ModifiedBys { get; set; }

    [Parameter("bool[]", "isChangeConfirmeds", 11)]
    public List<bool> IsChangeConfirmeds { get; set; }
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
    [Parameter("string", "createdAt", 7)]
    public string CreatedAt { get; set; }
    [Parameter("string", "updatedOn", 8)]
    public string UpdatedOn { get; set; }
    [Parameter("string", "createdBy", 9)]
    public string CreatedBy { get; set; }
    [Parameter("string", "modifiedBy", 10)]
    public string ModifiedBy { get; set; }
    [Parameter("bool", "isChangeConfirmed", 11)]
    public bool IsChangeConfirmed { get; set; }
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
    [Parameter("string", "_createdAt", 7)]
    public string CreatedAt { get; set; }
    [Parameter("string", "_updatedOn", 8)]
    public string UpdatedOn { get; set; }
    [Parameter("string", "_createdBy", 9)]
    public string CreatedBy { get; set; }
    [Parameter("string", "_modifiedBy", 10)]
    public string ModifiedBy { get; set; }
    [Parameter("bool", "_isChangeConfirmed", 11)]
    public bool IsChangeConfirmed { get; set; }
}
[FunctionOutput]
public class GetStudentByStudentNumberOutputDto : IFunctionOutputDTO
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
    [Parameter("string", "createdAt", 7)]
    public string CreatedAt { get; set; }
    [Parameter("string", "updatedOn", 8)]
    public string UpdatedOn { get; set; }
    [Parameter("string", "createdBy", 9)]
    public string CreatedBy { get; set; }
    [Parameter("string", "modifiedBy", 10)]
    public string ModifiedBy { get; set; }
    [Parameter("bool", "isChangeConfirmed", 11)]
    public bool IsChangeConfirmed { get; set; }
}
[Function("updateStudentByStudentNumber")]
public class UpdateStudentByStudentNumberFunction : FunctionMessage
{
    [Parameter("uint32", "_studentNumber", 1)]
    public uint StudentNumber { get; set; }
    [Parameter("string", "_enrollmentStatus", 2)]
    public string EnrollmentStatus { get; set; }
    [Parameter("bytes16", "_funderContractId", 3)]
    public byte[] FunderContractId { get; set; }
    [Parameter("string", "_updatedOn", 4)]
    public string UpdatedOn { get; set; }
    [Parameter("string", "_modifiedBy", 5)]
    public string ModifiedBy { get; set; }
    [Parameter("bool", "_isChangeConfirmed", 6)]
    public bool IsChangeConfirmed { get; set; }
    [Parameter("bool", "_isActive", 7)]
    public bool IsActive { get; set; }
}
[Function("getStudentByNumber", "tuple")]
public class GetStudentByStudentNumberFunction : FunctionMessage
{
    [Parameter("uint32", "_studentNumber", 1)]
    public uint StudentNumber { get; set; }
}
public class StudentUpdateDto
{
    public uint StudentNumber { get; set; }
    public string EnrollmentStatus { get; set; }
    public string FunderContractId { get; set; }
    public string ModifiedBy { get; set; }
    public bool IsChangeConfirmed { get; set; }
    public bool IsActive { get; set; }
}
public class StudentContract
{
    private readonly SmartContractAddressSettings _addressSettings;
    private readonly Web3 _web3;
    private readonly EthereumNodeSettings _nodeSettings;

    public StudentContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings)
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
            Id = dto.Id.ToByteArray(),
            StudentNumber = dto.StudentNumber,
            InstitutionId = dto.InstitutionId.ToByteArray(),
            EnrollmentStatus = dto.EnrollmentStatus,
            IsActive = dto.IsActive == "true",
            FunderContractId = funderContract,
            CreatedAt = DateTime.UtcNow.ToString("g"),
            UpdatedOn = DateTime.UtcNow.ToString("g"),
            CreatedBy = dto.CreatedBy,
            ModifiedBy = dto.ModifiedBy,
            IsChangeConfirmed = true,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<AddStudentFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.StudentAddress, function);
        var txHash = receipt?.TransactionHash;
        if (txHash == null)
        {
            return receipt?.HasErrors()?.ToString();
        }
        return txHash.ToString();
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
                FunderContractId = new Guid(result.FunderContractIds[i]),
                CreatedAt = result.CreatedAts[i],
                UpdatedOn = result.UpdatedOns[i],
                CreatedBy = result.CreatedBys[i],
                ModifiedBy = result.ModifiedBys[i],
                IsChangeConfirmed = result.IsChangeConfirmeds[i]
            });
        }
        return students;
    }
    public async Task<StudentOutputDto> GetStudentByIdAsync(Guid studentId)
    {
        var function = new GetStudentByIdFunction
        {
            Id = studentId.ToByteArray()
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.StudentAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetStudentByIdFunction, GetStudentByIdOutputDto>(function);

        return new StudentOutputDto
        {
            Id = new Guid(result.Id),
            StudentNumber = result.StudentNumber,
            InstitutionId = new Guid(result.InstitutionId),
            EnrollmentStatus = result.EnrollmentStatus,
            IsActive = result.IsActive,
            FunderContractId = new Guid(result.FunderContractId),
            CreatedAt = result.CreatedAt,
            UpdatedOn = result.UpdatedOn,
            CreatedBy = result.CreatedBy,
            ModifiedBy = result.ModifiedBy,
            IsChangeConfirmed = result.IsChangeConfirmed
        };
    }
    public async Task<string> UpdateStudentByStudentNumberAsync(StudentUpdateDto dto)
    {
        var funderContract = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId.ToString());
        var function = new UpdateStudentByStudentNumberFunction
        {
            StudentNumber = dto.StudentNumber,
            EnrollmentStatus = dto.EnrollmentStatus,
            FunderContractId = funderContract,
            UpdatedOn = DateTime.UtcNow.ToString("g"),
            ModifiedBy = dto.ModifiedBy,
            IsChangeConfirmed = dto.IsChangeConfirmed,
            IsActive = dto.IsActive,
            FromAddress = _nodeSettings.ContractAddress
        };
        var contract = _web3.Eth.GetContractTransactionHandler<UpdateStudentByStudentNumberFunction>();
        var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.StudentAddress, function);
        return receipt.TransactionHash;
    }
    public async Task<StudentOutputDto> GetStudentByStudentNumberAsync(uint studentNumber)
    {
        var function = new GetStudentByStudentNumberFunction
        {
            StudentNumber = studentNumber
        };
        var contract = _web3.Eth.GetContractHandler(_addressSettings.StudentAddress);
        var result = await contract.QueryDeserializingToObjectAsync<GetStudentByStudentNumberFunction, GetStudentByStudentNumberOutputDto>(function);

        return new StudentOutputDto
        {
            Id = new Guid(result.Id),
            StudentNumber = result.StudentNumber,
            InstitutionId = new Guid(result.InstitutionId),
            EnrollmentStatus = result.EnrollmentStatus,
            IsActive = result.IsActive,
            FunderContractId = new Guid(result.FunderContractId),
            CreatedAt = result.CreatedAt,
            UpdatedOn = result.UpdatedOn,
            CreatedBy = result.CreatedBy,
            ModifiedBy = result.ModifiedBy,
            IsChangeConfirmed = result.IsChangeConfirmed
        };
    }
}
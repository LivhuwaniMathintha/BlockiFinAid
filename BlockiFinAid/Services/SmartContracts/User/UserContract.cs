using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.Responses;
using BlockiFinAid.Helpers;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace BlockiFinAid.Services.SmartContracts.User
{
    public class UserOutputDto
    {
        public Guid Id { get; set; }
        public Guid InstitutionId { get; set; }
        public Guid FunderContractId { get; set; }
        public Guid BankAccountId { get; set; }
        public string StudentNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsChangeConfirmed { get; set; }
        public string ModifiedBy { get; set; }
        public string UpdatedAt { get; set; }
        public string CourseName { get; set; }
        public string Role { get; set; }
    }

    public class UserInputDto
    {
        public string InstitutionId { get; set; }
        public string FunderContractId { get; set; }
        public string BankAccountId { get; set; }
        public string StudentNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsChangeConfirmed { get; set; }
        public string ModifiedBy { get; set; }
        public string CourseName { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }
    }

    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public Guid InstitutionId { get; set; }
        public Guid FunderContractId { get; set; }
        public Guid BankAccountId { get; set; }
        public string StudentNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsChangeConfirmed { get; set; }
        public string ModifiedBy { get; set; }
        public string CourseName { get; set; }
        public string Role { get; set; }
    }

    [Function("getAllUsers", typeof(GetAllUsersOutputDTO))]
    public class GetAllUsersFunction : FunctionMessage { }

    [Function("getUserById", typeof(GetUserByIdOutputDto))]
    public class GetUserByIdFunction : FunctionMessage
    {
        [Parameter("bytes16", "_id", 1)]
        public byte[] Id { get; set; }
    }

    [Function("getUserByStudentNumber", typeof(GetUserByStudentNumberOutputDto))]
    public class GetUserByStudentNumberFunction : FunctionMessage
    {
        [Parameter("string", "_studentNumber", 1)]
        public string StudentNumber { get; set; }
    }
    
    [Function("getUserByName", typeof(GetUserByStudentNumberOutputDto))]
    public class GetUserByNameFunction : FunctionMessage
    {
        [Parameter("string", "_name", 1)]
        public string Name { get; set; }
    }

    [FunctionOutput]
    public class GetAllUsersOutputDTO : IFunctionOutputDTO
    {
        [Parameter("bytes16[]", "ids", 1)]
        public List<byte[]> Ids { get; set; }
        [Parameter("bytes16[]", "institutionIds", 2)]
        public List<byte[]> InstitutionIds { get; set; }
        [Parameter("bytes16[]", "funderContractIds", 3)]
        public List<byte[]> FunderContractIds { get; set; }
        [Parameter("bytes16[]", "bankAccountIds", 4)]
        public List<byte[]> BankAccountIds { get; set; }
        [Parameter("string[]", "studentNumbers", 5)]
        public List<string> StudentNumbers { get; set; }
        [Parameter("string[]", "names", 6)]
        public List<string> Names { get; set; }
        [Parameter("string[]", "emails", 7)]
        public List<string> Emails { get; set; }
        [Parameter("bool[]", "isActives", 8)]
        public List<bool> IsActives { get; set; }
        [Parameter("bool[]", "isChangeConfirmeds", 9)]
        public List<bool> IsChangeConfirmeds { get; set; }
        [Parameter("string[]", "modifiedBys", 10)]
        public List<string> ModifiedBys { get; set; }
        [Parameter("string[]", "updatedAts", 11)]
        public List<string> UpdatedAts { get; set; }
        [Parameter("string[]", "courseNames", 12)]
        public List<string> CourseNames { get; set; }
        [Parameter("string[]", "roles", 13)]
        public List<string> Roles { get; set; }
    }

    [FunctionOutput]
    public class GetUserByIdOutputDto : IFunctionOutputDTO
    {
        [Parameter("bytes16", "id", 1)]
        public byte[] Id { get; set; }
        [Parameter("bytes16", "institutionId", 2)]
        public byte[] InstitutionId { get; set; }
        [Parameter("bytes16", "funderContractId", 3)]
        public byte[] FunderContractId { get; set; }
        [Parameter("bytes16", "bankAccountId", 4)]
        public byte[] BankAccountId { get; set; }
        [Parameter("string", "studentNumber", 5)]
        public string StudentNumber { get; set; }
        [Parameter("string", "name", 6)]
        public string Name { get; set; }
        [Parameter("string", "email", 7)]
        public string Email { get; set; }
        [Parameter("bool", "isActive", 8)]
        public bool IsActive { get; set; }
        [Parameter("bool", "isChangeConfirmed", 9)]
        public bool IsChangeConfirmed { get; set; }
        [Parameter("string", "modifiedBy", 10)]
        public string ModifiedBy { get; set; }
        [Parameter("string", "updatedAt", 11)]
        public string UpdatedAt { get; set; }
        [Parameter("string", "courseName", 12)]
        public string CourseName { get; set; }
        [Parameter("string", "role", 13)]
        public string Role { get; set; }
    }

    [FunctionOutput]
    public class GetUserByNameOutputDto : IFunctionOutputDTO
    {
        [Parameter("bytes16", "id", 1)]
        public byte[] Id { get; set; }
        [Parameter("bytes16", "institutionId", 2)]
        public byte[] InstitutionId { get; set; }
        [Parameter("bytes16", "funderContractId", 3)]
        public byte[] FunderContractId { get; set; }
        [Parameter("bytes16", "bankAccountId", 4)]
        public byte[] BankAccountId { get; set; }
        [Parameter("string", "studentNumber", 5)]
        public string StudentNumber { get; set; }
        [Parameter("string", "name", 6)]
        public string Name { get; set; }
        [Parameter("string", "email", 7)]
        public string Email { get; set; }
        [Parameter("bool", "isActive", 8)]
        public bool IsActive { get; set; }
        [Parameter("bool", "isChangeConfirmed", 9)]
        public bool IsChangeConfirmed { get; set; }
        [Parameter("string", "modifiedBy", 10)]
        public string ModifiedBy { get; set; }
        [Parameter("string", "updatedAt", 11)]
        public string UpdatedAt { get; set; }
        [Parameter("string", "courseName", 12)]
        public string CourseName { get; set; }
        [Parameter("string", "role", 13)]
        public string Role { get; set; }
    }

    [FunctionOutput]
    public class GetUserByStudentNumberOutputDto : IFunctionOutputDTO
    {
        [Parameter("bytes16", "id", 1)]
        public byte[] Id { get; set; }
        [Parameter("bytes16", "institutionId", 2)]
        public byte[] InstitutionId { get; set; }
        [Parameter("bytes16", "funderContractId", 3)]
        public byte[] FunderContractId { get; set; }
        [Parameter("bytes16", "bankAccountId", 4)]
        public byte[] BankAccountId { get; set; }
        [Parameter("string", "studentNumber", 5)]
        public string StudentNumber { get; set; }
        [Parameter("string", "name", 6)]
        public string Name { get; set; }
        [Parameter("string", "email", 7)]
        public string Email { get; set; }
        [Parameter("bool", "isActive", 8)]
        public bool IsActive { get; set; }
        [Parameter("bool", "isChangeConfirmed", 9)]
        public bool IsChangeConfirmed { get; set; }
        [Parameter("string", "modifiedBy", 10)]
        public string ModifiedBy { get; set; }
        [Parameter("string", "updatedAt", 11)]
        public string UpdatedAt { get; set; }
        [Parameter("string", "courseName", 12)]
        public string CourseName { get; set; }
        [Parameter("string", "role", 13)]
        public string Role { get; set; }
    }

    [Function("addUser")]
    public class AddUserFunction : FunctionMessage
    {
        [Parameter("bytes16", "_id", 1)]
        public byte[] Id { get; set; }
        [Parameter("bytes16", "_institutionId", 2)]
        public byte[] InstitutionId { get; set; }
        [Parameter("bytes16", "_funderContractId", 3)]
        public byte[] FunderContractId { get; set; }
        [Parameter("bytes16", "_bankAccountId", 4)]
        public byte[] BankAccountId { get; set; }
        [Parameter("string", "_studentNumber", 5)]
        public string StudentNumber { get; set; }
        [Parameter("string", "_name", 6)]
        public string Name { get; set; }
        [Parameter("string", "_email", 7)]
        public string Email { get; set; }
        [Parameter("bool", "_isActive", 8)]
        public bool IsActive { get; set; }
        [Parameter("bool", "_isChangeConfirmed", 9)]
        public bool IsChangeConfirmed { get; set; }
        [Parameter("string", "_modifiedBy", 10)]
        public string ModifiedBy { get; set; }
        [Parameter("string", "_updatedAt", 11)]
        public string UpdatedAt { get; set; }
        [Parameter("string", "_courseName", 12)]
        public string CourseName { get; set; }
        [Parameter("string", "_role", 13)]
        public string Role { get; set; }
    }

    [Function("updateUser")]
    public class UpdateUserFunction : FunctionMessage
    {
        [Parameter("bytes16", "_id", 1)]
        public byte[] Id { get; set; }
        [Parameter("bytes16", "_institutionId", 2)]
        public byte[] InstitutionId { get; set; }
        [Parameter("bytes16", "_funderContractId", 3)]
        public byte[] FunderContractId { get; set; }
        [Parameter("bytes16", "_bankAccountId", 4)]
        public byte[] BankAccountId { get; set; }
        [Parameter("string", "_studentNumber", 5)]
        public string StudentNumber { get; set; }
        [Parameter("string", "_name", 6)]
        public string Name { get; set; }
        [Parameter("string", "_email", 7)]
        public string Email { get; set; }
        [Parameter("bool", "_isActive", 8)]
        public bool IsActive { get; set; }
        [Parameter("bool", "_isChangeConfirmed", 9)]
        public bool IsChangeConfirmed { get; set; }
        [Parameter("string", "_modifiedBy", 10)]
        public string ModifiedBy { get; set; }
        [Parameter("string", "_updatedAt", 11)]
        public string UpdatedAt { get; set; }
        [Parameter("string", "_courseName", 12)]
        public string CourseName { get; set; }
        [Parameter("string", "_role", 13)]
        public string Role { get; set; }
    }

    public class UserContract
    {
        private readonly SmartContractAddressSettings _addressSettings;
        private readonly Web3 _web3;
        private readonly EthereumNodeSettings _nodeSettings;

        public UserContract(IOptions<EthereumNodeSettings> nodeSettings, IOptions<SmartContractAddressSettings> addressSettings)
        {
            _nodeSettings = nodeSettings.Value;
            _addressSettings = addressSettings.Value;
            var account = new Account(_nodeSettings.PrivateKey);
            _web3 = new Web3(account, _nodeSettings.RpcUrl);
        }

        public async Task<ServiceResponse<string>> AddUserAsync(UserInputDto dto)
        {
            var function = new AddUserFunction
            {
                Id = GuidHelper.GuidToBytes16OrZero(dto.Id),
                InstitutionId = GuidHelper.GuidToBytes16OrZero(dto.InstitutionId),
                FunderContractId = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId),
                BankAccountId = GuidHelper.GuidToBytes16OrZero(dto.BankAccountId),
                StudentNumber = dto.StudentNumber,
                Name = dto.Name,
                Email = dto.Email,
                IsActive = dto.IsActive,
                IsChangeConfirmed = dto.IsChangeConfirmed,
                ModifiedBy = dto.ModifiedBy,
                UpdatedAt = DateTime.UtcNow.ToString("g"),
                CourseName = dto.CourseName,
                Role = dto.Role,
                FromAddress = _nodeSettings.ContractAddress
            };
            var contract = _web3.Eth.GetContractTransactionHandler<AddUserFunction>();
            var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.UsersAddress, function);
            var txHash = receipt?.TransactionHash;
            if (txHash == null)
            {
                return new ServiceResponse<string>
                {
                    Data = null,
                    Error = new List<string?>(){ receipt?.HasErrors()?.ToString()}
                };
            }

            return new ServiceResponse<string>
            {
                IsSuccess = true,
                Data = txHash
            };
        }

        public async Task<List<UserOutputDto>> GetAllUsersAsync()
        {
            var function = new GetAllUsersFunction();
            var contract = _web3.Eth.GetContractHandler(_addressSettings.UsersAddress);
            var result = await contract.QueryDeserializingToObjectAsync<GetAllUsersFunction, GetAllUsersOutputDTO>(function);
            var users = new List<UserOutputDto>();
            for (int i = 0; i < result.Ids.Count; i++)
            {
                users.Add(new UserOutputDto
                {
                    Id = new Guid(result.Ids[i]),
                    InstitutionId = new Guid(result.InstitutionIds[i]),
                    FunderContractId = new Guid(result.FunderContractIds[i]),
                    BankAccountId = new Guid(result.BankAccountIds[i]),
                    StudentNumber = result.StudentNumbers[i],
                    Name = result.Names[i],
                    Email = result.Emails[i],
                    IsActive = result.IsActives[i],
                    IsChangeConfirmed = result.IsChangeConfirmeds[i],
                    ModifiedBy = result.ModifiedBys[i],
                    UpdatedAt = result.UpdatedAts[i],
                    CourseName = result.CourseNames[i],
                    Role = result.Roles[i]
                });
            }
            return users;
        }

        public async Task<UserOutputDto> GetUserByIdAsync(Guid userId)
        {
            var function = new GetUserByIdFunction
            {
                Id = userId.ToByteArray()
            };
            var contract = _web3.Eth.GetContractHandler(_addressSettings.UsersAddress);
            var result = await contract.QueryDeserializingToObjectAsync<GetUserByIdFunction, GetUserByIdOutputDto>(function);

            return new UserOutputDto
            {
                Id = new Guid(result.Id),
                InstitutionId = new Guid(result.InstitutionId),
                FunderContractId = new Guid(result.FunderContractId),
                BankAccountId = new Guid(result.BankAccountId),
                StudentNumber = result.StudentNumber,
                Name = result.Name,
                Email = result.Email,
                IsActive = result.IsActive,
                IsChangeConfirmed = result.IsChangeConfirmed,
                ModifiedBy = result.ModifiedBy,
                UpdatedAt = result.UpdatedAt,
                CourseName = result.CourseName,
                Role = result.Role
            };
        }

        public async Task<UserOutputDto?> GetUserByNameAsync(string name)
        {
            var function = new GetUserByNameFunction
            {
                Name = name
            };
            var contract = _web3.Eth.GetContractHandler(_addressSettings.UsersAddress);
            try
            {
                var result =
                    await contract
                        .QueryDeserializingToObjectAsync<GetUserByNameFunction,
                            GetUserByNameOutputDto>(function);
                return new UserOutputDto
                {
                    Id = new Guid(result.Id),
                    InstitutionId = new Guid(result.InstitutionId),
                    FunderContractId = new Guid(result.FunderContractId),
                    BankAccountId = new Guid(result.BankAccountId),
                    StudentNumber = result.StudentNumber,
                    Name = result.Name,
                    Email = result.Email,
                    IsActive = result.IsActive,
                    IsChangeConfirmed = result.IsChangeConfirmed,
                    ModifiedBy = result.ModifiedBy,
                    UpdatedAt = result.UpdatedAt,
                    CourseName = result.CourseName,
                    Role = result.Role
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<UserOutputDto?> GetUserByStudentNumberAsync(string studentNumber)
        {
            var function = new GetUserByStudentNumberFunction
            {
                StudentNumber = studentNumber
            };
            var contract = _web3.Eth.GetContractHandler(_addressSettings.UsersAddress);
            try
            {
                var result =
                    await contract
                        .QueryDeserializingToObjectAsync<GetUserByStudentNumberFunction,
                            GetUserByStudentNumberOutputDto>(function);
                return new UserOutputDto
                {
                    Id = new Guid(result.Id),
                    InstitutionId = new Guid(result.InstitutionId),
                    FunderContractId = new Guid(result.FunderContractId),
                    BankAccountId = new Guid(result.BankAccountId),
                    StudentNumber = result.StudentNumber,
                    Name = result.Name,
                    Email = result.Email,
                    IsActive = result.IsActive,
                    IsChangeConfirmed = result.IsChangeConfirmed,
                    ModifiedBy = result.ModifiedBy,
                    UpdatedAt = result.UpdatedAt,
                    CourseName = result.CourseName,
                    Role = result.Role
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> UpdateUserAsync(UserUpdateDto dto)
        {
            var function = new UpdateUserFunction
            {
                Id = GuidHelper.GuidToBytes16OrZero(dto.Id.ToString()),
                InstitutionId = GuidHelper.GuidToBytes16OrZero(dto.InstitutionId.ToString()),
                FunderContractId = GuidHelper.GuidToBytes16OrZero(dto.FunderContractId.ToString()),
                BankAccountId = GuidHelper.GuidToBytes16OrZero(dto.BankAccountId.ToString()),
                StudentNumber = dto.StudentNumber,
                Name = dto.Name,
                Email = dto.Email,
                IsActive = dto.IsActive,
                IsChangeConfirmed = dto.IsChangeConfirmed,
                ModifiedBy = dto.ModifiedBy,
                UpdatedAt = DateTime.UtcNow.ToString("g"),
                CourseName = dto.CourseName,
                Role = dto.Role,
                FromAddress = _nodeSettings.ContractAddress
            };
            var contract = _web3.Eth.GetContractTransactionHandler<UpdateUserFunction>();
            var receipt = await contract.SendRequestAndWaitForReceiptAsync(_addressSettings.UsersAddress, function);
            var txHash = receipt?.TransactionHash;
            if (txHash == null)
            {
                return receipt?.HasErrors()?.ToString();
            }
            return txHash.ToString();
        }
    }
}

using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.DTOs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.User;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;


namespace BlockiFinAid.Helpers;

public class DatabaseSeeder
{
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly MongoDbSettings _mongoDbSettings;
    private readonly UserAccessControlService _uam;
    private readonly IBaseRepository<UserModel> _userRepo;
    private readonly IBaseRepository<PaymentModel> _paymentRepo;
    private readonly IBaseRepository<FunderModel> _funders;
    private readonly IBaseRepository<BankAccountModel> _bankAccounts;
    private readonly IBaseRepository<FundingConditionsModel> _fundingConditions;
    private readonly IBaseRepository<FundingModel> _funding;
    private readonly Random _random = new Random();
    public DatabaseSeeder(ILogger<DatabaseSeeder> logger, 
        IWebHostEnvironment env, 
        IOptions<MongoDbSettings> settings, 
        UserAccessControlService userAccessControlService, 
        IBaseRepository<UserModel> user,
        IBaseRepository<PaymentModel> payments, 
        IBaseRepository<FunderModel> funders,
        IBaseRepository<BankAccountModel> bankAccounts,
        IBaseRepository<FundingConditionsModel> fundingConditions,
        IBaseRepository<FundingModel> funding)
    {
        _logger = logger;
        _env = env;
        _mongoDbSettings = settings.Value;
        _uam = userAccessControlService;
        _userRepo = user;
        _paymentRepo = payments;
        _funders = funders;
        _bankAccounts = bankAccounts;
        _fundingConditions = fundingConditions;
        _funding = funding;
    }

    public async Task SeedDataAsync()
    {
        _logger.LogInformation("Seeding users...");
        
        // seed users including admin
        try
        {
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        var existingUser = await _uam.GetUserByEmail(UAMHelper.GetAminEmail());
        if (existingUser != null)
        {
            await SeedFundersAsync(existingUser.Id.ToString());
            //await SeedPaymentsFromJsonAsync();
            await SeedUsersFromJsonAsync(existingUser.Id.ToString());
        }
        else
        {
            _logger.LogError($"There is no admin-user with email {UAMHelper.GetAminEmail()}. Please check your configuration.");
        }
    }

    private async Task SeedAdminUserAsync()
    {
        _logger.LogInformation("Seeding api admin...");
      
        var adminUser = new UserAccessControlDto
        {
            Email = UAMHelper.GetAminEmail(),
            Name = "api-admin",
            Role = "Admin",
            Permissions = new List<string>
            {

                "api:global_access"

            },
            Password = "admin-pass"
        };
        
        var results = await _uam.RegisterUser(adminUser);
        
    }

    private async Task SeedFunderUsersAsync(string userPerformingAction)
    {
        var seedFilePath = Path.Combine(_env.ContentRootPath, _mongoDbSettings.FunderUsersJsonFilePath);

        if (!File.Exists(seedFilePath))
        {
            _logger.LogInformation($"Users seed file not found at {seedFilePath}. Skipping user seeding");
            return;
        }

        try
        {
            _logger.LogInformation($"Reading users from Json file {seedFilePath}");
            var jsonContent = await File.ReadAllTextAsync(seedFilePath);
            var usersToSeed = JsonConvert.DeserializeObject<List<UserAccessControlDto>>(jsonContent);

            if (usersToSeed == null || !usersToSeed.Any())
            {
                _logger.LogInformation($"No users found to seed. Skipping user seeding");
                return;
            }

            _logger.LogInformation($"Found {usersToSeed.Count} users in the JSON file. Starting to seed...");
            foreach (var user in usersToSeed)
            {
                // check if a specific user (e.g by student number) already exists
                // This prevents duplicate data on every startup
                var allUsers = await _userRepo.GetAllAsync();
                var doesUserExists = allUsers.FirstOrDefault(x => x.Name == user.Name);
                if (doesUserExists != null)
                {
                    _logger.LogError($"UserFunder {user.Name} already exists. Skipping user seeding");
                }
                else
                {
                      UserModel? studentNumberExists = null;
                    var studentNumber = "";
                    do
                    {
                        studentNumber = UAMHelper.GenerateStudentNumber();
                        studentNumberExists = await _userRepo.FindByStudentNumberAsync(studentNumber);
                    } while (studentNumberExists is not null || studentNumber =="");
                    
                    var userDetails = new UserModel
                    {
                        StudentNumber = studentNumber,
                        BankAccountId = Guid.Empty,
                        Email = user.Email,
                        Role = user.Role,
                        CourseName = UAMHelper.GenerateRandomDegree(),
                        ModifiedBy = "api-admin",
                        InstitutionId = Guid.Empty,
                        IsActive = true,
                        FunderContractId = Guid.Empty,
                        IsChangeConfirmed = false,
                        Name = user.Name
                    };

                    var bankAccount = UAMHelper.GenerateRandomBankAccount();
                    bankAccount.StudentNumber = userDetails.StudentNumber;
                    bankAccount.IsConfirmed = true;
                    bankAccount.Id = Guid.NewGuid();

                    try
                    {
                        var bankModel = await _bankAccounts.CreateAsync(bankAccount, userPerformingAction);
                      
                        _logger.LogInformation($"UserFunder {user.Name} bank account seeded successfully with studentNumber {userDetails.StudentNumber}");
                       
                        try
                        {
                            if (bankModel is BankAccountModel)
                            {
                                BankAccountModel bankAccountModel = (BankAccountModel)bankModel;
                                userDetails.BankAccountId = bankAccountModel.Id;
                                await _userRepo.CreateAsync(userDetails, userPerformingAction);
                                _logger.LogInformation($"UserFunder {user.Name} seeded with studentNumber {userDetails.StudentNumber}");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message, $"Failed to create funder - user for {user.Name}");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message, "Failed to create bank account");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error while seeding users");
        }
    }
    private async Task SeedFundersAsync(string adminId)
    {
        var seedFilePath = Path.Combine(_env.ContentRootPath, _mongoDbSettings.FundersJsonFilePath);

        if (!File.Exists(seedFilePath))
        {
            _logger.LogInformation($"Funders seed file not found at {seedFilePath}. Skipping funders seeding");
            return;
        }

        try
        {
            _logger.LogInformation($"Reading funders from {seedFilePath}");
            var jsonContent = await File.ReadAllTextAsync(seedFilePath);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new GuidSafeConverter() }
            };

            var itemsToSeed = JsonConvert.DeserializeObject<List<FunderModel>>(jsonContent, settings);
            
            if (itemsToSeed == null || !itemsToSeed.Any())
            {
                _logger.LogInformation($"No funder found to seed. Skipping funder seeding");
                return;
            }

            _logger.LogInformation($"Found {itemsToSeed.Count} items in the JSON file. Starting to seed...");
            foreach (var item in itemsToSeed)
            {
                // check if a specific user (e.g by student number) already exists
                // This prevents duplicate data on every startup

                var existingFunders = await _funders.GetAllAsync();
                var existingFunder = existingFunders.FirstOrDefault(x => x.Name == item.Name || x.Id == item.Id);
                if (existingFunder is null)
                {
                    _logger.LogInformation($"Funder with Id {item.Name} does not exist...");
                    var newFunder = new FunderModel
                    {
                        Id = item.Id,
                        PaymentDate = item.PaymentDate,
                        Name = item.Name,
                        IsChangeConfirmed = true,
                        UserIdPerformingAction = item.UserIdPerformingAction,
                        ModifiedBy = item.ModifiedBy,
                        UpdatedAt = item.UpdatedAt,
                        IsActive = true
                    };

                    _logger.LogInformation(
                        $"Processing Funder {newFunder.Name}...");
                    var results = await _funders.CreateAsync(newFunder, adminId);

                    if (results is not null)
                    {
                        _logger.LogInformation($"Successfully added {newFunder.Name} from seeding data");
                    }
                }
                else
                {
                    _logger.LogInformation($"Funder {item.Name} already exists. Ignoring it.");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        
       
    }
    private async Task SeedPaymentsFromJsonAsync()
    {
        _logger.LogInformation("Seeding payments has started, checking for api-admin user");
        var existingAdminUser = await _uam.GetUserByEmail(UAMHelper.GetAminEmail());

        if (existingAdminUser != null)
        {
            var seedFilePath = Path.Combine(_env.ContentRootPath, _mongoDbSettings.PaymentsJsonFilePath);

            if (!File.Exists(seedFilePath))
            {
                _logger.LogInformation($"Payment seed file not found at {seedFilePath}. Skipping payment seeding");
                return;
            }

            try
            {
                _logger.LogInformation($"Reading users from Json file {seedFilePath}");
                var jsonContent = await File.ReadAllTextAsync(seedFilePath);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter> { new GuidSafeConverter() }
                };

                var itemsToSeed = JsonConvert.DeserializeObject<List<PaymentModel>>(jsonContent, settings);
                
                if (itemsToSeed == null || !itemsToSeed.Any())
                {
                    _logger.LogInformation($"No payment found to seed. Skipping payment seeding");
                    return;
                }

                _logger.LogInformation($"Found {itemsToSeed.Count} payment in the JSON file. Starting to seed...");
                foreach (var item in itemsToSeed)
                {
                    // check if a specific user (e.g by student number) already exists
                    // This prevents duplicate data on every startup

                    var existingPayment = await _paymentRepo.GetByIdAsync(item.Id);

                    if (existingPayment is null)
                    {
                        _logger.LogInformation($"Payment with Id {item.Id} does not exist...");
                        var newPayment = new PaymentModel
                        {
                            Id = Guid.NewGuid(),
                            Amount = item.Amount,
                            AccountNumber = item.AccountNumber,
                            BankName = item.BankName,
                            BranchCode = item.BranchCode,
                            FulfilmentDate = item.FulfilmentDate,
                            Funder = item.Funder,
                            InitiationDate = item.InitiationDate,
                            InstitutionServiceId = Guid.NewGuid(),
                            UserIdPerformingAction = item.UserIdPerformingAction,
                            IsFraud = item.IsFraud,
                            ModifiedBy = item.ModifiedBy,
                            UpdatedAt = item.UpdatedAt,
                            PaymentType = item.PaymentType,
                            Status = item.Status,
                            Institution = item.Institution,
                            StudentNumber = item.StudentNumber
                        };

                        _logger.LogInformation(
                            $"Processing Payment for student {newPayment.StudentNumber} for {newPayment.InitiationDate}...");
                        var results = await _paymentRepo.CreateAsync(newPayment, existingAdminUser.Id.ToString());

                        if (results is not null)
                        {
                            _logger.LogInformation("Successfully initiated payment from seeding data");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        else
        {
            _logger.LogError($"API-ADMIN with email: {UAMHelper.GetAminEmail()} was not found in the system. ensure that you seed users first and try again");
        }
    }    
    private async Task SeedUsersFromJsonAsync(string adminId)
    {
        await SeedFunderUsersAsync(adminId);
        
        var seedFilePath = Path.Combine(_env.ContentRootPath, _mongoDbSettings.UsersJsonFilePath);

        if (!File.Exists(seedFilePath))
        {
            _logger.LogInformation($"[User Seeder] - Users seed file not found at {seedFilePath}. Skipping user seeding");
            return;
        }

        try
        {
            _logger.LogInformation($"[User Seeder] - Reading users from Json file {seedFilePath}");
            var jsonContent = await File.ReadAllTextAsync(seedFilePath);
            var usersToSeed = JsonConvert.DeserializeObject<List<UserAccessControlDto>>(jsonContent);

            if (usersToSeed == null || !usersToSeed.Any())
            {
                _logger.LogInformation($"[User Seeder] - No users found to seed. Skipping user seeding");
                return;
            }

            _logger.LogInformation($"[User Seeder] - Found {usersToSeed.Count} users in the JSON file. Starting to seed...");
            foreach (var user in usersToSeed)
            {
                // check if a specific user (e.g by student number) already exists
                // This prevents duplicate data on every startup

                var existingUser = await _uam.GetUserByEmail(user.Email);
                var allFunders = await _funders.GetAllAsync();
                if (allFunders.Any())
                {
                    var count = allFunders.Count();
               
                    var funderIndex = _random.Next(count);
                    var funderId = allFunders.ElementAt(funderIndex).Id;
                    if (existingUser is null)
                    {
                        _logger.LogInformation($"[User Seeder] - User with email {user.Email} does not exist...");
                        var password  = UAMHelper.PasswordGenerator();
                        var newUser = new UserAccessControlDto
                        {
                            Email = user.Email,
                            Role = user.Role,
                            Permissions = user.Permissions,
                            Name = user.Name,
                            Password = password
                        };
                        
                        _logger.LogInformation($"[User Seeder] - Processing UAM for {user.Email}... with password {password}...");

                        var result = await _uam.RegisterUser(newUser);

                        if (result is not null)
                        {
                            _logger.LogInformation($"[User Seeder] - UAM for {result.Email} has been registered with role {result.Role}");
                            
                            UserModel? studentNumberExists = null;
                            var studentNumber = "";
                            do
                            {
                                studentNumber = UAMHelper.GenerateStudentNumber();
                                studentNumberExists = await _userRepo.FindByStudentNumberAsync(studentNumber);
                            } while (studentNumberExists is not null);
                            
                            var userDetails = new UserModel
                            {
                                StudentNumber = studentNumber,
                                BankAccountId = Guid.Empty,
                                Email = user.Email,
                                Role = user.Role,
                                CourseName = UAMHelper.GenerateRandomDegree(),
                                ModifiedBy = "api-admin",
                                InstitutionId = Guid.Empty,
                                IsActive = true,
                                FunderContractId = funderId,
                                IsChangeConfirmed = false,
                                Name = user.Name,
                                
                            };

                            var bankAccount = UAMHelper.GenerateRandomBankAccount();
                            bankAccount.StudentNumber = userDetails.StudentNumber;
                            bankAccount.IsConfirmed = true;
                            bankAccount.Id = Guid.NewGuid();

                            try
                            {
                                var bankModel = await _bankAccounts.CreateAsync(bankAccount, adminId);
                      
                                _logger.LogInformation($"[User Seeder] - UserFunder {user.Name} bank account seeded successfully with studentNumber {userDetails.StudentNumber}");
                       
                                try
                                {
                                    if (bankModel is BankAccountModel)
                                    {
                                        BankAccountModel bankAccountModel = (BankAccountModel)bankModel;
                                        userDetails.BankAccountId = bankAccountModel.Id;
                                        var userResponse = await _userRepo.CreateAsync(userDetails, adminId);
                                        _logger.LogInformation($"[User Seeder] - User {user.Name} seeded with studentNumber {userDetails.StudentNumber}");
                                        if (userResponse is UserModel){
                                           UserModel userModel = (UserModel)userResponse;
                                           var random = new Random();
                                           
                                           var fundingConditions = new FundingConditionsModel
                                           {
                                               Id = Guid.NewGuid(),
                                               IsFullCoverage = true,
                                               StartDate = DateTime.UtcNow,
                                               EndDate = DateTime.UtcNow.AddYears(1),
                                               TotalAmount = (uint)random.Next(10000, 30000),
                                               FoodAmount = (uint)random.Next(1000, 35000),
                                               TuitionAmount = (uint)random.Next(100, 100000),
                                               LaptopAmount = (uint)random.Next(100, 10000),
                                               AccommodationAmount = (uint)random.Next(100, 40000),
                                               AccommodationDirectPay = true,
                                               ModifiedBy = "api-admin",
                                               UpdatedAt = DateTime.UtcNow,
                                               IsActive = true,
                                               AverageMark = (uint)random.Next(65, 95),
                                           };
                                           
                                           var fundingConditionsResponse = await _fundingConditions.CreateAsync(fundingConditions, adminId);

                                           if (fundingConditionsResponse is FundingConditionsModel)
                                           {
                                               _logger.LogInformation($"[User Seeder] - Funding conditions for {user.Email} has been created.");
                                               var fundingConditionsOutputDto = (FundingConditionsModel)fundingConditionsResponse;
                                               var fundingDto = new FundingModel
                                               {
                                                   StudentNumber = userModel.StudentNumber,
                                                   StudentId = userModel.Id,
                                                   AccommodationBalance = fundingConditions.AccommodationAmount,
                                                   LaptopBalance = fundingConditions.LaptopAmount,
                                                   TuitionBalance = fundingConditions.TuitionAmount,
                                                   FoodBalance = fundingConditions.FoodAmount,
                                                   FunderId = funderId,
                                                   FunderContractConditionId = fundingConditionsOutputDto.Id,
                                                   SignedOn = DateTime.UtcNow,
                                                   ModifiedBy = "api-admin",
                                                   UpdatedAt = DateTime.UtcNow,
                                               };
                                               
                                               await _funding.CreateAsync(fundingDto, adminId);
                                           }
                                        }
                                        _logger.LogInformation($"[User Seeder] - User Model {user.Email} has been created.");
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e.Message, $"[User Seeder] - Failed to create funder - user for {user.Name}");
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e.Message, "[User Seeder] - Failed to create bank account");
                            }
                           
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"[User Seeder] - User with email {user.Email} already exists...");
                    }
                }
                else
                {
                    _logger.LogInformation($"[User Seeder] - There are no funders found, cannot seed user");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[User Seeder] - Error while seeding users");
        }
    }
}
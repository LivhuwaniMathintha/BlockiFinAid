using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.MachineLearning;
using BlockiFinAid.Services.Messaging;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models; // Added for OpenApiInfo
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using Serilog;


try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add Serilog as the logging provider
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
    }); // Use Serilog for logging
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    // builder.Services.AddOpenApi(); // REMOVED: Replaced by AddSwaggerGen

    // Configure Swagger/OpenAPI services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlockiFinAid API", Version = "v1" });
    });
    // 

    // map the appsettings to my classes
    var ethereumNodeSettings = builder.Configuration.GetSection("EthereumNodeSettings");
    builder.Services.Configure<EthereumNodeSettings>(ethereumNodeSettings);
    
    var byteCodeSettings = builder.Configuration.GetSection("SmartContractByteCodes");
    builder.Services.Configure<SmartContractByteCodes>(byteCodeSettings);
    
    var sendGridSettings = builder.Configuration.GetSection("SendGridSettings");
    builder.Services.Configure<SendGridSettings>(sendGridSettings);
    
    var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings");
    builder.Services.Configure<MongoDbSettings>(mongoDbSettings);
    
    //configure databases

    builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        var connectionString = builder.Configuration.GetConnectionString("MongoDB");
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        
        // configure settings for replica set and better reliability
        
        settings.RetryWrites = true;
        settings.RetryReads = true;
        settings.ReadPreference = ReadPreference.Primary;
        settings.WriteConcern = WriteConcern.WMajority;
        
        return new MongoClient(settings);
    });

    // Configure IMongoDatabase

    builder.Services.AddKeyedSingleton<IMongoDatabase>("DB.Funds", (sp, key) =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var dbName = builder.Configuration["MongoDB:Funds"] ?? "FinancialAid_Funds";
        return client.GetDatabase(dbName);
    });
    
    builder.Services.AddKeyedSingleton<IMongoDatabase>("DB.UserAccessControl", (sp, key) =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var dbName = builder.Configuration["MongoDB:Access"] ?? "FinAidDb";
        return client.GetDatabase(dbName);
    });
    
    builder.Services.AddKeyedSingleton<IMongoDatabase>("DB.Institution", (sp, key) =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var dbName = builder.Configuration["MongoDB:Institution"] ?? "FinancialAid_Institution";
        return client.GetDatabase(dbName);
    });

    //Register the generic repository for the models

    builder.Services.AddSingleton<IBaseRepository<BankAccountModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<BankAccountModel>>>();
        return new BaseRepository<BankAccountModel>(db, "bankAccounts", logger);
    });
    
    builder.Services.AddSingleton<IBaseRepository<PaymentCheckerModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<PaymentCheckerModel>>>();
        return new BaseRepository<PaymentCheckerModel>(db, "paymentChecker", logger);
    });

    builder.Services.AddSingleton<IBaseRepository<FundingModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<FundingModel>>>();
        return new BaseRepository<FundingModel>(db, "funding", logger);
    });

    builder.Services.AddSingleton<IBaseRepository<FundingConditionsModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<FundingConditionsModel>>>();
        return new BaseRepository<FundingConditionsModel>(db, "fundingConditions", logger);
    });

    builder.Services.AddSingleton<IBaseRepository<FunderModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<FunderModel>>>();
        return new BaseRepository<FunderModel>(db, "funders",  logger);
    });

    builder.Services.AddSingleton<IBaseRepository<PaymentModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Funds");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<PaymentModel>>>();
        return new BaseRepository<PaymentModel>(db, "payments", logger);
    });

    builder.Services.AddScoped<IBaseRepository<StudentTrackerModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Institution");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<StudentTrackerModel>>>();
        return new BaseRepository<StudentTrackerModel>(db, "studentContractTrackers",  logger);
    });


    builder.Services.AddScoped<IBaseRepository<UserModel>>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.Institution");
        var logger = sp.GetRequiredService<ILogger<BaseRepository<UserModel>>>();
        return new BaseRepository<UserModel>(db, "users",  logger);
    });
    builder.Services.AddSingleton<IPasswordHasher<UserAccessControl>, PasswordHasher<UserAccessControl>>();
    builder.Services.AddScoped<UserAccessControlService>(sp =>
    {
        var db = sp.GetRequiredKeyedService<IMongoDatabase>("DB.UserAccessControl");
        var passwordHasher = sp.GetRequiredService<IPasswordHasher<UserAccessControl>>();
        var logger = sp.GetRequiredService<ILogger<UserAccessControlService>>();
        var userRepository = sp.GetRequiredService<IBaseRepository<UserModel>>();
        return new UserAccessControlService(passwordHasher, db, logger, userRepository);
    });
    
    var smartContractAddressSettings = builder.Configuration.GetSection("SmartContractAddressSettings");
    builder.Services.Configure<SmartContractAddressSettings>(smartContractAddressSettings);
    builder.Services.AddTransient<FunderContract>();
    builder.Services.AddTransient<UserContract>();
    builder.Services.AddTransient<BankAccountContract>();
    builder.Services.AddTransient<FundingContract>();
    builder.Services.AddTransient<FundingConditionsContract>();
    builder.Services.AddTransient<PaymentContract>();
    builder.Services.AddTransient<IPublisher, MassTransitPublisher>();
    
    // //Consumers
    // builder.Services.AddScoped<UserDbConsumer>();
    // builder.Services.AddScoped<BankAccountDbConsumer>();
    // builder.Services.AddScoped<FundingDbConsumer>();
    // builder.Services.AddScoped<FundingConditionsDbConsumer>();
    // builder.Services.AddScoped<PaymentEventConsumer>();
    //
    // builder.Services.AddScoped<BankAccountDbConsumer>(sp =>
    // {
    //     var logger = sp.GetRequiredService<ILogger<BankAccountDbConsumer>>();
    //     var bankAccountContract = sp.GetRequiredService<BankAccountContract>();
    //     return new BankAccountDbConsumer(logger, bankAccountContract);
    // });

   
    // configure mass transit rabbit mq here

    builder.Services.AddMassTransit(busConfigurator =>
    {
        busConfigurator.SetKebabCaseEndpointNameFormatter();
        busConfigurator.AddConsumer<UserDbConsumer>();
        busConfigurator.AddConsumer<BankAccountDbConsumer>();
        busConfigurator.AddConsumer<PaymentEventConsumer>();
        busConfigurator.AddConsumer<FundingDbConsumer>();
        busConfigurator.AddConsumer<FundingConditionsDbConsumer>();
        busConfigurator.AddConsumer<FunderDbConsumer>();
        busConfigurator.AddConsumer<PaymentDbConsumer>();
        
        busConfigurator.AddConfigureEndpointsCallback((name, cfg) =>
        {
            if (cfg is IRabbitMqReceiveEndpointConfigurator rmq)
            {
                rmq.PrefetchCount = 1;
                rmq.UseConcurrencyLimit(1);
            }
        });
        busConfigurator.UsingRabbitMq((context, configurator) =>
        {
            configurator.Host(new Uri(builder.Configuration["RabbitMQ:Host"]!), h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });

            configurator.ConfigureEndpoints(context);
        });
    });

    // Add a background service for mongo db publishing to rabbit mq
    builder.Services.AddScoped<DatabaseSeeder>();
    
    builder.Services.AddHostedService<MongoSeederBackgroundService>();
    builder.Services.AddHostedService<MongoDbBackgroundPublisherService>();
    
   
    builder.Services.AddRefitClient<IMachineLearningAPI>(new RefitSettings
    {
        ContentSerializer = new NewtonsoftJsonContentSerializer(
            new JsonSerializerSettings
            {
                //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            }),
    }).ConfigureHttpClient(configure =>
    {
        configure.BaseAddress = new Uri(builder.Configuration["ApiBaseUrls:MachineLearning"] ?? string.Empty);
    });
    
    builder.Services.AddControllers();
    builder.Services.AddHostedService<PaymentInitiator>();
    var app = builder.Build();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // app.MapOpenApi(); // REMOVED: Since it's replaced by Swagger
        // ADDED: Use Swagger and Swagger UI middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlockiFinAid API V1");
        });
        // END ADDED
    }
    app.UseCors();
    app.UseHttpsRedirection();
   
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
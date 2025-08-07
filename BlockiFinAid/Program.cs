using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services.AccessControl;
using BlockiFinAid.Services.Messaging;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Set the default minimum level
    .WriteTo.Console()
    .CreateBootstrapLogger(); // Write logs to the console

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add Serilog as the logging provider
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
    }); // Use Serilog for logging

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // map the appsettings to my classes
    var ethereumNodeSettings = builder.Configuration.GetSection("EthereumNodeSettings");
    builder.Services.Configure<EthereumNodeSettings>(ethereumNodeSettings);

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
        return new UserAccessControlService(passwordHasher, db, logger);
    });
    
    var smartContractAddressSettings = builder.Configuration.GetSection("SmartContractAddressSettings");
    builder.Services.Configure<SmartContractAddressSettings>(smartContractAddressSettings);
    builder.Services.AddControllers();
    builder.Services.AddScoped<FunderContract>();
    builder.Services.AddScoped<UserContract>();
    builder.Services.AddScoped<BankAccountContract>();
    builder.Services.AddScoped<FundingContract>();
    builder.Services.AddScoped<FundingConditionsContract>();
    builder.Services.AddScoped<IPublisher, MassTransitPublisher>();
    builder.Services.AddScoped<UserDbConsumer>();
    builder.Services.AddScoped<BankAccountDbConsumer>();

    builder.Services.AddScoped<DatabaseSeeder>();
    
    // configure mass transit rabbit mq here

    builder.Services.AddMassTransit(busConfigurator =>
    {
        busConfigurator.SetKebabCaseEndpointNameFormatter();
        busConfigurator.AddConsumer<UserDbConsumer>();
        busConfigurator.AddConsumer<BankAccountDbConsumer>();
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

    builder.Services.AddHostedService<MongoDbBackgroundPublisherService>();
    builder.Services.AddControllers();

    var app = builder.Build();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        // var accessControlService = scope.ServiceProvider.GetRequiredService<UserAccessControlService>();
        // var logger  = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
        // var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        // var settings = scope.ServiceProvider.GetRequiredService<IOptions<MongoDbSettings>>();
        //
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedDataAsync();
    }

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
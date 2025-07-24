using BlockiFinAid.Data.Configs;
using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Messaging;
using BlockiFinAid.Services.Repository;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// map the appsettings to my classes
var ethereumNodeSettings = builder.Configuration.GetSection("EthereumNodeSettings");
builder.Services.Configure<EthereumNodeSettings>(ethereumNodeSettings);

var sendGridSettings = builder.Configuration.GetSection("SendGridSettings");
builder.Services.Configure<SendGridSettings>(sendGridSettings);

//configure databases

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    
    return new MongoClient(connectionString);
});

// Configure IMongoDatabase

builder.Services.AddKeyedSingleton<IMongoDatabase>("Financial_Aid.Funds", (sp, key) =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = "Financial_Aid_Funds";
    return client.GetDatabase(dbName);
});

builder.Services.AddKeyedSingleton<IMongoDatabase>("Financial_Aid.Institution", (sp, key) =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["MongoDB:Institution"] ?? "Financial_Aid.Institution";
    return client.GetDatabase(dbName);
});

//Register the generic repository for the models

builder.Services.AddSingleton<IBaseRepository<BankAccountModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Funds");
    return new BaseRepository<BankAccountModel>(db, "bankAccounts");
});

builder.Services.AddSingleton<IBaseRepository<FundingModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Funds");
    return new BaseRepository<FundingModel>(db, "funding");
});

builder.Services.AddSingleton<IBaseRepository<FundingConditionsModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Funds");
    return new BaseRepository<FundingConditionsModel>(db, "fundingConditions");
});

builder.Services.AddSingleton<IBaseRepository<FunderModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Funds");
    return new BaseRepository<FunderModel>(db, "funders");
});

builder.Services.AddSingleton<IBaseRepository<PaymentModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Funds");
    return new BaseRepository<PaymentModel>(db, "payments");
});

builder.Services.AddScoped<IBaseRepository<StudentTrackerModel>>(sp =>
{
    var db = sp.GetRequiredKeyedService<IMongoDatabase>("Financial_Aid.Institution");
    return new BaseRepository<StudentTrackerModel>(db, "studentContractTrackers");
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
  
//builder.Services.AddHostedService<MongoDbBackgroundPublisherService>();
builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();


app.Run();

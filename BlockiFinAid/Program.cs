using BlockiFinAid.Data.Configs;
using BlockiFinAid.Services.Messaging;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.User;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// map the appsettings to my classes
var ethereumNodeSettings = builder.Configuration.GetSection("EthereumNodeSettings");
builder.Services.Configure<EthereumNodeSettings>(ethereumNodeSettings);

var sendGridSettings = builder.Configuration.GetSection("SendGridSettings");
builder.Services.Configure<SendGridSettings>(sendGridSettings);

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


app.Run();

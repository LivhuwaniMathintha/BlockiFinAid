using BlockiFinAid.Data;
using BlockiFinAid.Data.Configs;
using BlockiFinAid.Helpers;
using BlockiFinAid.Services;
using BlockiFinAid.Services.SmartContracts;
using BlockiFinAid.Services.SmartContracts.Student;

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
builder.Services.AddScoped<NewStudentService>();
builder.Services.AddScoped<StudentContract>();
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

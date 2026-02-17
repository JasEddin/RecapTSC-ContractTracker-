using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Services;

var builder = Host.CreateApplicationBuilder(args);

// configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// DI registrations
builder.Services.AddSingleton<IOpenApiLoader, OpenApiLoader>();
builder.Services.AddSingleton<IContractComparer, OpenApiComparer>();
builder.Services.AddSingleton<IContractComparerService, ContractComparerService>();

var app = builder.Build();

// resolve & run
var comparerService = app.Services
    .GetRequiredService<IContractComparerService>();

await comparerService.RunAsync();
comparerService.GetComparisonResults();


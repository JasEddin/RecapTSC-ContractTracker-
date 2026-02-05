using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();


var compareService = new ContractComparerService(configuration);

compareService.Run();
compareService.GetComparisonResults();


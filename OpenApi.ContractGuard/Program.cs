using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Reporting;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

 var file1 = new OpenApiLoader().Load(configuration["OpenApi:Old"]);
 var file2 = new OpenApiLoader().Load(configuration["OpenApi:New"]);
var comparer = new OpenApi.ContractGuard.Comparison.OpenApiComparer();
var changes = comparer.Compare(file1, file2);
ConsoleReporter.ReportChanges(changes);
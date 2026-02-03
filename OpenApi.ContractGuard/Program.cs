using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

Console.WriteLine(configuration["OpenApi:Old"]);
Console.WriteLine(configuration["OpenApi:New"]);

//var file1 = new OpenApiLoader().Load("C:\\projects\\APIS\\apis\\skandia\\internal\\individkund-risk-och-halsa\\riskbedomning-rest\\v1\\riskbedomning-rest.json");
//var file2 = new OpenApiLoader().Load("");
//var comparer = new OpenApi.ContractGuard.Comparison.OpenApiComparer();
//6var changes = comparer.Compare(file1, file2);
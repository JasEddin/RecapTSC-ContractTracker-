// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var file1 = new OpenApiLoader().Load("C:\\projects\\APIS\\apis\\skandia\\internal\\individkund-risk-och-halsa\\riskbedomning-rest\\v1\\riskbedomning-rest.json");
var file2 = new OpenApiLoader().Load("C:\\projects\\APIS\\apis\\skandia\\internal\\individkund-risk-och-halsa\\riskbedomning-rest\\v2\\riskbedomning-rest.json");
var comparer = new OpenApi.ContractGuard.Comparison.OpenApiComparer();
var changes = comparer.Compare(file1, file2);
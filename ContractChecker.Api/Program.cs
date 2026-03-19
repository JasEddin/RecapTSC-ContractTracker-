using ContractChecker.Core.Comparison;
using ContractChecker.Core.Models;
using ContractChecker.Core.Processor;

var builder = WebApplication.CreateBuilder(args);
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
// 🔹 Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Register your Core service
builder.Services.AddTransient<IOpenApiLoader, OpenApiLoader>();
builder.Services.AddSingleton<IApplicationProvider, ApplicationProvider>();
builder.Services.AddSingleton<IContractFileProvider, ContractFileProvider>();
builder.Services.AddSingleton<IContractComparer, OpenApiComparer>();

builder.Services.AddHttpClient("OpenApiProbe", client =>
{
    client.Timeout = TimeSpan.FromSeconds(4);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("ContractChecker/1.0");
});

builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // React dev server
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// 🔹 Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/api/applications/", async (IApplicationProvider provider, IContractFileProvider contractFileProvider, IContractComparer comparer) =>
{

    IEnumerable<ApplicationInfo> result = await provider.GetApplicationsAsync();
    return Results.Ok(result);
});

app.MapGet("/api/applications/{environment}", async (IApplicationProvider provider, IContractFileProvider contractFileProvider, IContractComparer comparer, string environment = "u3") =>
{

    IEnumerable<ApplicationInfo> result = await provider.GetApplicationsAsync(environment);
    return Results.Ok(result);
});
app.MapGet("/api/application", async (IApplicationProvider provider, IContractFileProvider contractFileProvider, IContractComparer comparer, string name, string environment = "u3") =>
{
    ApplicationDetail result = await provider.GetApplicationAsync(name, environment);
    return Results.Ok(result);
});

app.MapGet("/api/open-with-vscode", (string path) =>
{
    if (!File.Exists(path))
        return Results.NotFound();

    var psi = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "code", // VS Code CLI
        Arguments = $"\"{path}\"",
        UseShellExecute = true
    };

    System.Diagnostics.Process.Start(psi);

    return Results.Ok();
});
app.Run();
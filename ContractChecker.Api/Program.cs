using ContractChecker.Core;

var builder = WebApplication.CreateBuilder(args);
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
// 🔹 Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Register your Core service
builder.Services.AddScoped<ContractDiffService>();
builder.Services.AddSingleton<IApplicationProvider, ApplicationProvider>();

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

// 🔹 Temporary test endpoint
app.MapGet("/ping", () => "Swagger is working");


app.MapGet("/api/applications", async (IApplicationProvider provider) =>
{
    var result = await provider.GetApplicationsAsync();

    return Results.Ok(result);
});


app.MapGet("/api/application/{id}", async (IApplicationProvider provider, string id) =>
{
    var result = await provider.GetApplicationAsync(id);
    return Results.Ok(result);
});

app.Run();
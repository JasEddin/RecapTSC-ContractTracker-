using ContractChecker.Core;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Register your Core service
builder.Services.AddScoped<ContractDiffService>();

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



app.MapGet("/api/applications", () =>
{
    return new[]
    {
        new { id = "app1", name = "RiskBedomning.Rest" },
        new { id = "app2", name = "InsuranceClaims.Api" },
        new { id = "app3", name = "Fason.Api" },
        new { id = "app4", name = "Some.Api" }
    };
});

app.Run();
using ContractChecker.Core;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Register your Core service
builder.Services.AddScoped<ContractDiffService>();

var app = builder.Build();

// 🔹 Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Temporary test endpoint
app.MapGet("/ping", () => "Swagger is working");

app.Run();
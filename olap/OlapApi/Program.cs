using OlapApi.Data;

const string ClientCorsPolicy = "OlapClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ISeniorVisitRepository, SampleSeniorVisitRepository>();

// React origins come from config (Cors:AllowedOrigins) so Docker can override them via env vars.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(ClientCorsPolicy, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().WithMethods("GET")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(ClientCorsPolicy);

app.MapControllers();

app.Run();

// Exposes the top-level Program class to WebApplicationFactory in OlapApi.Tests (needed on .NET 9).
public partial class Program;

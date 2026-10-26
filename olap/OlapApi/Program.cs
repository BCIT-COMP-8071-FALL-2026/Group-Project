using OlapApi.Data;

const string ClientCorsPolicy = "OlapClient";

var builder = WebApplication.CreateBuilder(args);

// ODBC connection string for the OLAP database. Set it with user secrets locally and with the
// ConnectionStrings__DefaultConnection environment variable when deployed (see olap/README.md).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// React origins come from config (Cors:AllowedOrigins) so Docker can override them via env vars.
var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Select(origin => origin.TrimEnd('/'))
    .ToArray();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
    options.AddPolicy(ClientCorsPolicy, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().WithMethods("GET")));
builder.Services.AddSingleton<IReportRepository>(new OdbcReportRepository(connectionString));
builder.Services.AddSingleton<IEtlRepository>(new OdbcEtlRepository(connectionString));

var app = builder.Build();

// Unhandled exceptions (e.g. the database is unreachable) become a logged 500 problem details response.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(ClientCorsPolicy);

app.MapControllers();

app.Run();

// Exposes the top-level Program class to WebApplicationFactory in OlapApi.Tests (needed on .NET 9).
public partial class Program;

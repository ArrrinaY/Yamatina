using InternshipService;
using InternshipService.Middleware;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerConfiguration(builder.Configuration);
}

builder.Services.AddHybridAuthentication(builder.Configuration);
builder.Services.AddHybridAuthorization();
builder.Services.AddAuthHelpers(builder.Configuration);

builder.Services.AddCustomHealthChecks(builder.Configuration);
builder.Services.RegisterControllers();
builder.Services.AddValidators();
builder.Services.AddCorsPolicy();

builder.Services.RegisterServices();
builder.Services.AddMappings();
builder.Services.AddRedisCache(builder.Configuration);

builder.Services.RegisterRepositories();

builder.Services.AddDbContextPoolWithPostgres(builder.Configuration.GetConnectionString("Postgres"));


var app = builder.Build();
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = Init.WriteHealthChecksResponse });
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();


using HealthDataCollector.Data.Control;
using HealthDataCollector.Data.Control.Entities;
using HealthDataCollector.Models;
using HealthDataCollector.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ControlDbContext>(options =>                                  // Register SQLite Control context for settings and data
    options.UseSqlite(builder.Configuration.GetConnectionString("ControlDatabase")));

builder.Services.AddSingleton<IAppStateService, AppStateService>();         // Register AppStateService for App State, Settings, and Cache data.

builder.Services.AddScoped<ISourceService, SourceService>();       // Register Provisioning services.

builder.Services.AddOpenApi();

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ControlDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(db);
}

var appState = app.Services.GetRequiredService<IAppStateService>();

var result = await appState.LoadSettingsAsync();

if (!result.Success)
{
    app.Logger.LogInformation("Failed to load Settings.  {Error} {Message}.",
        result.Error,
        result.Message);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/Admin"));

app.MapPost("/api/weight", async(WeightMeasurementDto dto, HttpContext context, ILogger<Program> logger, ISourceService _ps) =>
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var correlationId = context.TraceIdentifier;

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            return Results.Unauthorized();
        }

        var result = await _ps.ValidateApiKeyAsync(apiKey);

        if (!result.Success)
        {
            logger.LogDebug("API Error: {Error}", result.Message);
        }

        logger.LogDebug("Device Id: {DeviceId}.", result.Data.ToString());

        logger.LogDebug(
                    "Received weight measurement {MeasurementId}: " +
                    "{WeightKg} kg, Impedance={ImpedanceOhm} ohm, " +
                    "MeasuredAt={MeasuredAt}, ReceivedAt={ReceivedAt}, " +
                    "CorrelationId={CorrelationId}",
                    dto.MeasurementId,
                    dto.WeightKg,
                    dto.ImpedanceOhm,
                    dto.MeasuredAt,
                    receivedAt,
                    correlationId
                );

        return Results.Ok(new MeasurementAcceptedDto
        {
            MeasurementId = dto.MeasurementId,
            ReceivedAt = receivedAt,
            CorrelationId = correlationId
        });
    });

app.MapPost("/api/provision", async(ProvisionRequestDto dto, HttpContext context, ILogger < Program> logger, ISourceService _ps) =>
    {
        if (!_ps.IsEnabled())
        {
            return Results.NotFound();
        }

        DateTimeOffset receivedAt = DateTimeOffset.UtcNow;
        string correlationId = context.TraceIdentifier;

        var result = await _ps.ProvisionNewDevice(correlationId, dto);
            
        if (!result.Success)
        {
            logger.LogInformation(
                "Provision Failed: {Name}, DeviceType={DeviceType}. {Error} {Message}.",
                dto.Name,
                dto.DeviceType,
                result.Error.ToString(),
                result.Message
            );

            return Results.StatusCode(500);         // Return Failed
        }

        ApiKeyIssueDto provisioningResult = result.Data;


        // If we get here provisioning was successful
        _ps.Disable();

        logger.LogInformation(
            "Provisioned source {SourceId}: {Name}, DeviceType={DeviceType}",
            provisioningResult.SourceId,
            dto.Name,
            dto.DeviceType
        );

        return Results.Ok(provisioningResult);
    });

app.Run();


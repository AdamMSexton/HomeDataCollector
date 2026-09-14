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
builder.Services.AddScoped<IProvisioningService, ProvisioningService>();       // Register Provisioning gate keeper service.

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/Admin"));

app.MapPost("/api/weight", (WeightMeasurementDto dto, HttpContext context, ILogger<Program> logger) =>
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var correlationId = context.TraceIdentifier;

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

app.MapPost("/api/provision", async(ProvisionRequestDto dto, HttpContext context, ILogger < Program> logger, IProvisioningService provision) =>
    {
        if (!provision.IsEnabled())
        {
            return Results.NotFound();
        }

        DateTimeOffset receivedAt = DateTimeOffset.UtcNow;
        string correlationId = context.TraceIdentifier;

        ProvisionResponseDto provisioningResult = null;

        using (var scope = app.Services.CreateScope())
        {
            var _ps = scope.ServiceProvider.GetRequiredService<IProvisioningService>();

            var result = await _ps.ProvisionNewClient(correlationId, dto);
            
            if (!result.Success)
            {
                return Results.StatusCode(500);         // Return Failed
            }
            
            provisioningResult = result.Data;
        }

        provision.Disable();

        logger.LogInformation(
            "Provisioned source {SourceId}: {Name}, DeviceType={DeviceType}",
            provisioningResult.SourceId,
            dto.Name,
            dto.DeviceType
        );

        return Results.Ok(provisioningResult);
    });

app.Run();


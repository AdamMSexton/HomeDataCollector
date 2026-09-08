using HealthDataCollector.Models;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



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

app.MapPost("/api/provision", (ProvisionRequestDto dto, HttpContext context, ILogger < Program> logger) =>
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var correlationId = context.TraceIdentifier;

        var sourceId = Guid.NewGuid();

        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var apiKey = Convert.ToHexString(keyBytes);

        logger.LogInformation(
            "Provisioned source {SourceId}: {Name}, DeviceType={DeviceType}",
            sourceId,
            dto.Name,
            dto.DeviceType
        );

        var response = new ProvisionResponseDto
        {
            SourceId = sourceId,
            ApiKey = apiKey,
            CorrelationId = correlationId
        };

        return Results.Ok(response);
    });

app.Run();


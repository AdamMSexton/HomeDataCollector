using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDataCollector.Migrations.HealthDb
{
    /// <inheritdoc />
    public partial class HealthDataDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeightMeasurements",
                columns: table => new
                {
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<byte>(type: "smallint", nullable: false),
                    WeightKg = table.Column<double>(type: "double precision", nullable: false),
                    ImpedanceOhm = table.Column<int>(type: "integer", nullable: true),
                    MeasuredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightMeasurements", x => new { x.SourceId, x.MeasurementId });
                    table.CheckConstraint("CK_WeightMeasurements_ImpedanceOhm", "\"ImpedanceOhm\" IS NULL OR \"ImpedanceOhm\" > 0");
                    table.CheckConstraint("CK_WeightMeasurements_WeightKg", "\"WeightKg\" > 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeightMeasurements");
        }
    }
}

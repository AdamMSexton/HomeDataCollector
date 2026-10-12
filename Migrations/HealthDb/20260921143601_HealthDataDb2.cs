using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeDataCollector.Migrations.HealthDb
{
    /// <inheritdoc />
    public partial class HealthDataDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WeightMeasurements_WeightKg",
                table: "WeightMeasurements");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WeightMeasurements_WeightKg",
                table: "WeightMeasurements",
                sql: "\"WeightKg\" > 0 AND \"WeightKg\" <= 700");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WeightMeasurements_WeightKg",
                table: "WeightMeasurements");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WeightMeasurements_WeightKg",
                table: "WeightMeasurements",
                sql: "\"WeightKg\" > 0");
        }
    }
}

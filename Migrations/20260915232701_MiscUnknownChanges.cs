using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDataCollector.Migrations
{
    /// <inheritdoc />
    public partial class MiscUnknownChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "ApiKeys",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ApiKeys",
                newName: "id");
        }
    }
}

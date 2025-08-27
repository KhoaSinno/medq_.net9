using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medq.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clinics",
                columns: new[] { "Id", "Address", "Name" },
                values: new object[,]
                {
                    { 1, "123 Main St, Cityville", "Downtown Health Clinic" },
                    { 2, "456 Elm St, Townsville", "Uptown Medical Center" },
                    { 3, "789 Oak St, Suburbia", "Suburban Family Clinic" }
                });

            migrationBuilder.InsertData(
                table: "Pharmacies",
                columns: new[] { "Id", "Address", "Name", "OpenNow" },
                values: new object[,]
                {
                    { 1, "100 Health St, Cityville", "City Pharmacy", true },
                    { 2, "200 Wellness Ave, Townsville", "Town Drugstore", false },
                    { 3, "300 Care Blvd, Suburbia", "Suburbia Meds", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Pharmacies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pharmacies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Pharmacies",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}

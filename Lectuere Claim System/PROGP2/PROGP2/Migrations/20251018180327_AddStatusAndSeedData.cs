using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PROGP2.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "Claims",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "Name", "Password", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, "John Doe", "password123", "Employee", "john.doe" },
                    { 2, "Jane Smith", "password123", "Employee", "jane.smith" },
                    { 3, "Mike Johnson", "password123", "Manager", "mike.johnson" },
                    { 4, "Sarah Wilson", "password123", "Employee", "sarah.wilson" },
                    { 5, "System Administrator", "admin123", "Admin", "admin" }
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "ID", "Hours", "Rate", "RatePDF", "Status", "UserID" },
                values: new object[,]
                {
                    { 1, 8.5, 150.0, null, "Pending", 1 },
                    { 2, 6.0, 200.0, null, "Approved", 2 },
                    { 3, 10.0, 175.0, null, "Declined", 1 },
                    { 4, 7.5, 180.0, null, "Pending", 3 },
                    { 5, 9.0, 160.0, null, "Approved", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}

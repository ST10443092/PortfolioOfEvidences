using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PROGP2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 5,
                columns: new[] { "Name", "Password", "Role", "UserName" },
                values: new object[] { "Programme Coordinator", "coord123", "Coordinator", "coordinator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 5,
                columns: new[] { "Name", "Password", "Role", "UserName" },
                values: new object[] { "System Administrator", "admin123", "Admin", "admin" });
        }
    }
}

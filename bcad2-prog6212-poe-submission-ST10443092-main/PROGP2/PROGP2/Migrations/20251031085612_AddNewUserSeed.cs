using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROGP2.Migrations
{
    /// <inheritdoc />
    public partial class AddNewUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "Name", "Password", "Role", "UserName" },
                values: new object[] { 6, "James Sutton", "HR234", "HR", "james.sutton" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 6);
        }
    }
}

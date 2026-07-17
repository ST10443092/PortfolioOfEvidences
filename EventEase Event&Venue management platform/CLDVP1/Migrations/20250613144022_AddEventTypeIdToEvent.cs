using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLDVP1.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeIdToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_EventTypeId",
                table: "Booking",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_EventTypes_EventTypeId",
                table: "Booking",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_EventTypes_EventTypeId",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_EventTypeId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Booking");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaHotel.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationToReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Reserva",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_UserId",
                table: "Reserva",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reserva_AspNetUsers_UserId",
                table: "Reserva",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reserva_AspNetUsers_UserId",
                table: "Reserva");

            migrationBuilder.DropIndex(
                name: "IX_Reserva_UserId",
                table: "Reserva");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reserva");
        }
    }
}

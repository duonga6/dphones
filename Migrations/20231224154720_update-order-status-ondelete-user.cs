using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class updateorderstatusondeleteuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatus_Users_UserId",
                table: "OrderStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatus_Users_UserId",
                table: "OrderStatus",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatus_Users_UserId",
                table: "OrderStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatus_Users_UserId",
                table: "OrderStatus",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

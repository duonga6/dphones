using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sold",
                table: "Product");

            migrationBuilder.AddColumn<int>(
                name: "Sold",
                table: "Capacities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Capacities_SellPrice",
                table: "Capacities",
                column: "SellPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Capacities_SellPrice",
                table: "Capacities");

            migrationBuilder.DropColumn(
                name: "Sold",
                table: "Capacities");

            migrationBuilder.AddColumn<int>(
                name: "Sold",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

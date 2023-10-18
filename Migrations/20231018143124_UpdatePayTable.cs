using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TradingCode",
                table: "PayStatuses",
                newName: "CardType");

            migrationBuilder.RenameColumn(
                name: "Money",
                table: "PayStatuses",
                newName: "Amount");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentCode",
                table: "PayStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "PayStatuses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PayStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "PayStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankTranNo",
                table: "PayStatuses",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderInfo",
                table: "PayStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseCode",
                table: "PayStatuses",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionNo",
                table: "PayStatuses",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionStatus",
                table: "PayStatuses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "PayStatuses");

            migrationBuilder.DropColumn(
                name: "BankTranNo",
                table: "PayStatuses");

            migrationBuilder.DropColumn(
                name: "OrderInfo",
                table: "PayStatuses");

            migrationBuilder.DropColumn(
                name: "ResponseCode",
                table: "PayStatuses");

            migrationBuilder.DropColumn(
                name: "TransactionNo",
                table: "PayStatuses");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "PayStatuses");

            migrationBuilder.RenameColumn(
                name: "CardType",
                table: "PayStatuses",
                newName: "TradingCode");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PayStatuses",
                newName: "Money");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentCode",
                table: "PayStatuses",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "PayStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "PayStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnNova.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "WithdrawalRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "WithdrawalRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "WithdrawalRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "WithdrawalRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "WithdrawalRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "WithdrawalRequests");
        }
    }
}

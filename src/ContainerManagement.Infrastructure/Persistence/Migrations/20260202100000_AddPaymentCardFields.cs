using Microsoft.EntityFrameworkCore.Migrations;

namespace ContainerManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardLast4",
                table: "Payments",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardExpMonth",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CardExpYear",
                table: "Payments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardLast4",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardExpMonth",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardExpYear",
                table: "Payments");
        }
    }
}


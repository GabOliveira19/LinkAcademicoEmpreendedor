using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutMercadoPagoPremium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoCheckoutUrl",
                table: "AssinaturasPremium",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPaymentId",
                table: "AssinaturasPremium",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoPreferenceId",
                table: "AssinaturasPremium",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPagamento",
                table: "AssinaturasPremium",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MercadoPagoCheckoutUrl",
                table: "AssinaturasPremium");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPaymentId",
                table: "AssinaturasPremium");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPreferenceId",
                table: "AssinaturasPremium");

            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "AssinaturasPremium");
        }
    }
}

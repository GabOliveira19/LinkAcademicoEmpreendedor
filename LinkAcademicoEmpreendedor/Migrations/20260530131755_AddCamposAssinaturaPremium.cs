using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposAssinaturaPremium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PixQrCodeBase64",
                table: "AssinaturasPremium",
                newName: "MercadoPagoPreferenceId");

            migrationBuilder.RenameColumn(
                name: "PixCopiaECola",
                table: "AssinaturasPremium",
                newName: "MercadoPagoPaymentId");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPago",
                table: "AssinaturasPremium",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorPago",
                table: "AssinaturasPremium");

            migrationBuilder.RenameColumn(
                name: "MercadoPagoPreferenceId",
                table: "AssinaturasPremium",
                newName: "PixQrCodeBase64");

            migrationBuilder.RenameColumn(
                name: "MercadoPagoPaymentId",
                table: "AssinaturasPremium",
                newName: "PixCopiaECola");
        }
    }
}

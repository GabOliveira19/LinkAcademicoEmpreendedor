using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddMoedasEgressoMercadoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustoMoedasCandidatura",
                table: "Oportunidades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "VagaPremiumEgresso",
                table: "Oportunidades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MoedasGastas",
                table: "Candidaturas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaldoMoedas",
                table: "Alunos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ComprasMoedas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    Pacote = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    QuantidadeMoedas = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MercadoPagoPaymentId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PixCopiaECola = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PixQrCodeBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicketUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PagoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasMoedas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasMoedas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 1,
                column: "Beneficios",
                value: "Selo premium no perfil; vagas priorizadas na lista de oportunidades; mais visibilidade para empresas");

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 2,
                column: "Beneficios",
                value: "Tudo do Core; filtros avançados na busca de talentos; acesso ampliado a talentos recentes");

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 3,
                column: "Beneficios",
                value: "Tudo do Advanced; destaque máximo; mais projetos em destaque; visão ampliada de talentos");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasMoedas_AlunoId",
                table: "ComprasMoedas",
                column: "AlunoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprasMoedas");

            migrationBuilder.DropColumn(
                name: "CustoMoedasCandidatura",
                table: "Oportunidades");

            migrationBuilder.DropColumn(
                name: "VagaPremiumEgresso",
                table: "Oportunidades");

            migrationBuilder.DropColumn(
                name: "MoedasGastas",
                table: "Candidaturas");

            migrationBuilder.DropColumn(
                name: "SaldoMoedas",
                table: "Alunos");

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 1,
                column: "Beneficios",
                value: "Selo premium; vagas em destaque; acesso a talentos recentes");

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 2,
                column: "Beneficios",
                value: "Tudo do Core; filtros avançados; mais destaque nas oportunidades");

            migrationBuilder.UpdateData(
                table: "PlanosPremium",
                keyColumn: "Id",
                keyValue: 3,
                column: "Beneficios",
                value: "Tudo do Advanced; destaque máximo; relatórios; suporte prioritário");
        }
    }
}

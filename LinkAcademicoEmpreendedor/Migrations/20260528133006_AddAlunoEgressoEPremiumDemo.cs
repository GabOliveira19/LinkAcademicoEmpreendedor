using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddAlunoEgressoEPremiumDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnoConclusao",
                table: "Alunos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprovanteAcademico",
                table: "Alunos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EhEgresso",
                table: "Alunos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "Alunos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MatriculaValidada",
                table: "Alunos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PlanosPremium",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Beneficios = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosPremium", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssinaturasPremium",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    PlanoPremiumId = table.Column<int>(type: "int", nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PixQrCodeBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PixCopiaECola = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturasPremium", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturasPremium_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssinaturasPremium_PlanosPremium_PlanoPremiumId",
                        column: x => x.PlanoPremiumId,
                        principalTable: "PlanosPremium",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PlanosPremium",
                columns: new[] { "Id", "Beneficios", "Nome", "Ordem", "ValorMensal" },
                values: new object[,]
                {
                    { 1, "Selo premium; vagas em destaque; acesso a talentos recentes", "Core", 1, 59.90m },
                    { 2, "Tudo do Core; filtros avançados; mais destaque nas oportunidades", "Advanced", 2, 119.90m },
                    { 3, "Tudo do Advanced; destaque máximo; relatórios; suporte prioritário", "Advanced Plus", 3, 149.90m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasPremium_EmpresaId",
                table: "AssinaturasPremium",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasPremium_PlanoPremiumId",
                table: "AssinaturasPremium",
                column: "PlanoPremiumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssinaturasPremium");

            migrationBuilder.DropTable(
                name: "PlanosPremium");

            migrationBuilder.DropColumn(
                name: "AnoConclusao",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "ComprovanteAcademico",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "EhEgresso",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "MatriculaValidada",
                table: "Alunos");
        }
    }
}

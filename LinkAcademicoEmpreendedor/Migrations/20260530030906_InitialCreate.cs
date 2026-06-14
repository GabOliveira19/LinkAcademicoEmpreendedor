using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustoCandidatura",
                table: "Oportunidades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CarteirasToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    Saldo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarteirasToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarteirasToken_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PacotesToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantidadeTokens = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacotesToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComprasToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PacoteTokenId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuantidadeTokens = table.Column<int>(type: "int", nullable: false),
                    StatusPagamento = table.Column<int>(type: "int", nullable: false),
                    MercadoPagoPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasToken_Alunos_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprasToken_PacotesToken_PacoteTokenId",
                        column: x => x.PacoteTokenId,
                        principalTable: "PacotesToken",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PacotesToken",
                columns: new[] { "Id", "Ativo", "Nome", "QuantidadeTokens", "Valor" },
                values: new object[,]
                {
                    { 1, true, "Básico", 500, 9.90m },
                    { 2, true, "Intermediário", 1000, 17.90m },
                    { 3, true, "Profissional", 2500, 39.90m },
                    { 4, true, "Premium", 5000, 69.90m }
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
                name: "IX_CarteirasToken_AlunoId",
                table: "CarteirasToken",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasToken_PacoteTokenId",
                table: "ComprasToken",
                column: "PacoteTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasToken_UsuarioId",
                table: "ComprasToken",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarteirasToken");

            migrationBuilder.DropTable(
                name: "ComprasToken");

            migrationBuilder.DropTable(
                name: "MovimentacoesToken");

            migrationBuilder.DropTable(
                name: "PacotesToken");

            migrationBuilder.DropColumn(
                name: "CustoCandidatura",
                table: "Oportunidades");

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

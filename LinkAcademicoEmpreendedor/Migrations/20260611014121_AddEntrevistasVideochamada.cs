using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddEntrevistasVideochamada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entrevistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidaturaId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    OportunidadeId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CodigoSala = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrevistas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entrevistas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Entrevistas_Candidaturas_CandidaturaId",
                        column: x => x.CandidaturaId,
                        principalTable: "Candidaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entrevistas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Entrevistas_Oportunidades_OportunidadeId",
                        column: x => x.OportunidadeId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entrevistas_AlunoId",
                table: "Entrevistas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrevistas_CandidaturaId",
                table: "Entrevistas",
                column: "CandidaturaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entrevistas_EmpresaId",
                table: "Entrevistas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrevistas_OportunidadeId",
                table: "Entrevistas",
                column: "OportunidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entrevistas");
        }
    }
}

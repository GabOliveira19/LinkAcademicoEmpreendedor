using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddRedesSociaisEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RedeSocial_Alunos_AlunoId",
                table: "RedeSocial");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RedeSocial",
                table: "RedeSocial");

            migrationBuilder.RenameTable(
                name: "RedeSocial",
                newName: "RedesSociais");

            migrationBuilder.RenameIndex(
                name: "IX_RedeSocial_AlunoId",
                table: "RedesSociais",
                newName: "IX_RedesSociais_AlunoId");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "RedesSociais",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RedesSociais",
                table: "RedesSociais",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RedesSociais_EmpresaId",
                table: "RedesSociais",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RedesSociais_Alunos_AlunoId",
                table: "RedesSociais",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RedesSociais_Empresas_EmpresaId",
                table: "RedesSociais",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RedesSociais_Alunos_AlunoId",
                table: "RedesSociais");

            migrationBuilder.DropForeignKey(
                name: "FK_RedesSociais_Empresas_EmpresaId",
                table: "RedesSociais");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RedesSociais",
                table: "RedesSociais");

            migrationBuilder.DropIndex(
                name: "IX_RedesSociais_EmpresaId",
                table: "RedesSociais");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "RedesSociais");

            migrationBuilder.RenameTable(
                name: "RedesSociais",
                newName: "RedeSocial");

            migrationBuilder.RenameIndex(
                name: "IX_RedesSociais_AlunoId",
                table: "RedeSocial",
                newName: "IX_RedeSocial_AlunoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RedeSocial",
                table: "RedeSocial",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RedeSocial_Alunos_AlunoId",
                table: "RedeSocial",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id");
        }
    }
}

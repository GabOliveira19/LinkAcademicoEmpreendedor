using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddAreasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvaliadorId = table.Column<int>(type: "int", nullable: false),
                    AvaliadoId = table.Column<int>(type: "int", nullable: false),
                    TipoAvaliador = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoAvaliado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OportunidadeId = table.Column<int>(type: "int", nullable: true),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cnpj = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SituacaoCadastral = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataAbertura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NaturezaJuridica = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Cep = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AreaAtuacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoEmpresa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NomeResponsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinatarioId = table.Column<int>(type: "int", nullable: false),
                    TipoDestinatario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Lida = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Curso = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AnoIngresso = table.Column<int>(type: "int", nullable: true),
                    Instituicao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Sobre = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Habilidades = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FotoPerfil = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkedIn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GitHub = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenRecuperacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Curriculo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alunos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alunos_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alunos_Areas_AreaId1",
                        column: x => x.AreaId1,
                        principalTable: "Areas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    OpcoesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldDefinitions_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oportunidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Requisitos = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Local = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modalidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salario = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DataPublicacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oportunidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oportunidades_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tecnologias = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinkRepositorio = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LinkDemonstracao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ImagemCapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projetos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedeSocial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plataforma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedeSocial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedeSocial_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Candidaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCandidatura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MensagemApresentacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataVisualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataResposta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensagemResposta = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    OportunidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidaturas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Candidaturas_Oportunidades_OportunidadeId",
                        column: x => x.OportunidadeId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataComentario = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    ProjetoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comentarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comentarios_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Curtidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCurtida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    ProjetoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curtidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Curtidas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Curtidas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Curtidas_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetoLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ProjetoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetoLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjetoLinks_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, "Área padrão / multiáreas", "Interdisciplinar" },
                    { 2, "Ciências da Computação, Sistemas, TI", "Tecnologia" },
                    { 3, "Medicina, Enfermagem, Farmácia", "Saúde" },
                    { 4, "Engenharias diversas", "Engenharia" },
                    { 5, "Ciências Jurídicas", "Direito" },
                    { 6, "Administração, Economia", "Negócios" },
                    { 7, "Design, Artes Visuais, Multimídia", "Design/Artes" },
                    { 8, "Pedagogia, Educação", "Educação" },
                    { 9, "Sociologia, Psicologia, Antropologia", "Ciências Sociais" }
                });

            migrationBuilder.InsertData(
                table: "FieldDefinitions",
                columns: new[] { "Id", "AreaId", "Nome", "Obrigatorio", "OpcoesJson", "Ordem", "Tipo" },
                values: new object[,]
                {
                    { 1, 2, "GitHub", false, null, 1, "url" },
                    { 2, 2, "StackUtilizada", true, null, 2, "string" },
                    { 3, 2, "Tecnologias", false, null, 3, "string" },
                    { 4, 2, "Deploy", false, null, 4, "string" },
                    { 5, 2, "DocumentacaoTecnica", false, null, 5, "textarea" },
                    { 10, 3, "AreaPesquisaClinica", true, null, 1, "string" },
                    { 11, 3, "InstituicaoAplicacao", true, null, 2, "string" },
                    { 12, 3, "MetodologiaCientifica", true, null, 3, "textarea" },
                    { 13, 3, "ArtigosReferencias", false, null, 4, "textarea" },
                    { 14, 3, "AprovacaoEtica", false, null, 5, "boolean" },
                    { 15, 3, "DocumentoCientifico", false, null, 6, "file" },
                    { 20, 4, "MateriaisUtilizados", false, null, 1, "textarea" },
                    { 21, 4, "CalculosNormas", true, null, 2, "textarea" },
                    { 22, 4, "SoftwaresUtilizados", false, null, 3, "string" },
                    { 23, 4, "ModelosProjetos", false, null, 4, "file" },
                    { 30, 7, "FerramentasUtilizadas", false, null, 1, "string" },
                    { 31, 7, "BehanceFigma", false, null, 2, "url" },
                    { 32, 7, "Prototipo", false, null, 3, "url" },
                    { 40, 5, "AreaJuridica", true, null, 1, "string" },
                    { 41, 5, "CasoEstudo", false, null, 2, "textarea" },
                    { 42, 5, "DocumentoDrive", false, null, 3, "url" },
                    { 50, 6, "MercadoAlvo", false, null, 1, "string" },
                    { 51, 6, "PlanoNegocios", false, null, 2, "textarea" },
                    { 60, 9, "Metodologia", false, null, 1, "textarea" },
                    { 61, 9, "Referencias", false, null, 2, "textarea" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_AreaId",
                table: "Alunos",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_AreaId1",
                table: "Alunos",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_Email",
                table: "Alunos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidaturas_AlunoId_OportunidadeId",
                table: "Candidaturas",
                columns: new[] { "AlunoId", "OportunidadeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Candidaturas_OportunidadeId",
                table: "Candidaturas",
                column: "OportunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_AlunoId",
                table: "Comentarios",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_EmpresaId",
                table: "Comentarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_ProjetoId",
                table: "Comentarios",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_AlunoId_EmpresaId_ProjetoId",
                table: "Curtidas",
                columns: new[] { "AlunoId", "EmpresaId", "ProjetoId" },
                unique: true,
                filter: "[AlunoId] IS NOT NULL AND [EmpresaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_EmpresaId",
                table: "Curtidas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_ProjetoId",
                table: "Curtidas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Cnpj",
                table: "Empresas",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Email",
                table: "Empresas",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldDefinitions_AreaId",
                table: "FieldDefinitions",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_EmpresaId",
                table: "Oportunidades",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetoLinks_ProjetoId",
                table: "ProjetoLinks",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_AlunoId",
                table: "Projetos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_RedeSocial_AlunoId",
                table: "RedeSocial",
                column: "AlunoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Candidaturas");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "Curtidas");

            migrationBuilder.DropTable(
                name: "FieldDefinitions");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "ProjetoLinks");

            migrationBuilder.DropTable(
                name: "RedeSocial");

            migrationBuilder.DropTable(
                name: "Oportunidades");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Alunos");

            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}

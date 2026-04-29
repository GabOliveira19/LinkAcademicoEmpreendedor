CSharp LinkAcademicoEmpreendedor\Data\SeedData.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Data
{
    public static class SeedData
    {
        public static async Task MigrateAndSeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("SeedData");

            try
            {
                // Aplica migrations pendentes
                logger?.LogInformation("Aplicando migrations...");
                await context.Database.MigrateAsync();

                // Verificar conexão e existência lógica da tabela antes de consultar DbSet
                if (!await context.Database.CanConnectAsync())
                {
                    logger?.LogError("Nao foi possivel conectar ao banco apos Migrate.");
                    return;
                }

                // Verifica se a tabela 'Areas' existe no banco (proteção contra 'Invalid object name')
                bool areasTableExists;
                var connection = context.Database.GetDbConnection();
                await using (connection)
                {
                    await connection.OpenAsync();
                    await using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Areas'";
                    var result = await cmd.ExecuteScalarAsync();
                    areasTableExists = Convert.ToInt32(result) > 0;
                }

                if (!areasTableExists)
                {
                    logger?.LogWarning("Tabela 'Areas' nao encontrada apos aplicar migrations. Verifique migrations e connection string.");
                    return;
                }

                // Garante que exista ao menos a área "Interdisciplinar"
                if (!context.Areas.Any())
                {
                    logger?.LogInformation("Inserindo áreas padrão...");
                    context.Areas.AddRange(
                        new Area { Id = 1, Nome = "Interdisciplinar", Descricao = "Área padrão / multiáreas" },
                        new Area { Nome = "Tecnologia", Descricao = "Ciências da Computação, Sistemas, TI" },
                        new Area { Nome = "Saúde", Descricao = "Medicina, Enfermagem, Farmácia" },
                        new Area { Nome = "Engenharia", Descricao = "Engenharias diversas" },
                        new Area { Nome = "Direito", Descricao = "Ciências Jurídicas" },
                        new Area { Nome = "Negócios", Descricao = "Administração, Economia" },
                        new Area { Nome = "Design/Artes", Descricao = "Design, Artes Visuais, Multimídia" },
                        new Area { Nome = "Educação", Descricao = "Pedagogia, Educação" },
                        new Area { Nome = "Ciências Sociais", Descricao = "Sociologia, Psicologia, Antropologia" }
                    );
                    await context.SaveChangesAsync();
                }

                // Mapear Alunos legados: se AreaId == 0 (valor default) ou área inválida, tentar inferir por Curso
                logger?.LogInformation("Mapeando alunos legados para AreaId...");
                var areas = await context.Areas.ToListAsync();
                var padrao = areas.FirstOrDefault(a => a.Nome == "Interdisciplinar") ?? areas.First();

                var alunosParaAtualizar = await context.Alunos
                    .Where(a => a.AreaId == 0)
                    .ToListAsync();

                foreach (var aluno in alunosParaAtualizar)
                {
                    var atribuido = false;
                    if (!string.IsNullOrWhiteSpace(aluno.Curso))
                    {
                        var curso = aluno.Curso.Trim();
                        // Busca correspondência simples (contains) com nome da área
                        foreach (var area in areas)
                        {
                            if (curso.IndexOf(area.Nome, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                aluno.AreaId = area.Id;
                                atribuido = true;
                                break;
                            }
                        }

                        // Heurísticas simples para tecnologia/engenharia/saude
                        if (!atribuido)
                        {
                            var cursoLower = curso.ToLowerInvariant();
                            if (cursoLower.Contains("comput") || cursoLower.Contains("sistemas") || cursoLower.Contains("inform") || cursoLower.Contains("ti"))
                            {
                                var tech = areas.FirstOrDefault(a => a.Nome == "Tecnologia");
                                if (tech != null) { aluno.AreaId = tech.Id; atribuido = true; }
                            }
                            else if (cursoLower.Contains("engenh") || cursoLower.Contains("eng"))
                            {
                                var eng = areas.FirstOrDefault(a => a.Nome == "Engenharia");
                                if (eng != null) { aluno.AreaId = eng.Id; atribuido = true; }
                            }
                            else if (cursoLower.Contains("med") || cursoLower.Contains("enferm") || cursoLower.Contains("farm"))
                            {
                                var saude = areas.FirstOrDefault(a => a.Nome == "Saúde");
                                if (saude != null) { aluno.AreaId = saude.Id; atribuido = true; }
                            }
                        }
                    }

                    if (!atribuido)
                    {
                        aluno.AreaId = padrao.Id;
                    }
                }

                if (alunosParaAtualizar.Any())
                {
                    logger?.LogInformation("Atualizando {count} alunos com AreaId inferida.", alunosParaAtualizar.Count);
                    await context.SaveChangesAsync();
                }
                else
                {
                    logger?.LogInformation("Nenhum aluno legado para mapear.");
                }

                logger?.LogInformation("Seed/migração completa.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Erro ao aplicar migrations/seed. Verifique migrations e connection string.");
                throw;
            }
        }
    }
}
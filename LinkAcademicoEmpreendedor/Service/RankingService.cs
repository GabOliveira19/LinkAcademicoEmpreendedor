using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.ViewModels;

namespace LinkAcademicoEmpreendedor.Services
{
    public class RankingService
    {
        private readonly ApplicationDbContext _context;

        public RankingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ranking de Alunos
        public async Task<List<RankingAlunoViewModel>> ObterRankingAlunosAsync(int top = 50)
        {
            // Buscar todos os alunos
            var alunos = await _context.Alunos
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.FotoPerfil,
                    a.Curso,
                    a.Instituicao
                })
                .ToListAsync();

            // Contar projetos por aluno
            var projetosPorAluno = await _context.Projetos
                .GroupBy(p => p.AlunoId)
                .Select(g => new { AlunoId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            // Contar comentarios por aluno
            var comentariosPorAluno = await _context.Comentarios
                .GroupBy(c => c.AlunoId)
                .Select(g => new { AlunoId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            // Contar curtidas por aluno
            var curtidasPorAluno = await _context.Curtidas
                .GroupBy(c => c.AlunoId)
                .Select(g => new { AlunoId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            // Montar ranking
            var ranking = alunos.Select(a => new RankingAlunoViewModel
            {
                AlunoId = a.Id,
                Nome = a.Nome,
                FotoPerfil = a.FotoPerfil,
                Curso = a.Curso,
                Instituicao = a.Instituicao,
                TotalProjetos = projetosPorAluno.ContainsKey(a.Id) ? projetosPorAluno[a.Id] : 0,
                TotalComentarios = comentariosPorAluno.ContainsKey(a.Id) ? comentariosPorAluno[a.Id] : 0,
                TotalCurtidas = curtidasPorAluno.ContainsKey(a.Id) ? curtidasPorAluno[a.Id] : 0
            }).ToList();

            // Calcular pontuacao e ordenar
            foreach (var item in ranking)
            {
                item.CalcularPontuacao();
            }

            ranking = ranking
                .OrderByDescending(r => r.PontuacaoTotal)
                .Take(top)
                .ToList();

            // Atribuir posicoes
            for (int i = 0; i < ranking.Count; i++)
            {
                ranking[i].Posicao = i + 1;
            }

            return ranking;
        }

        // Ranking de Empresas
        public async Task<List<RankingEmpresaViewModel>> ObterRankingEmpresasAsync(int top = 50)
        {
            // Buscar todas as empresas com suas avaliacoes
            var empresas = await _context.Empresas
                .Select(e => new
                {
                    e.Id,
                    e.RazaoSocial,
                    e.NomeFantasia,
                    e.LogoEmpresa,
                    e.AreaAtuacao,
                    e.Cidade,
                    e.Estado
                })
                .ToListAsync();

            // Buscar avaliacoes das empresas
            var avaliacoesPorEmpresa = await _context.Avaliacoes
                .Where(a => a.TipoAvaliado == "Empresa")
                .GroupBy(a => a.AvaliadoId)
                .Select(g => new
                {
                    EmpresaId = g.Key,
                    TotalAvaliacoes = g.Count(),
                    MediaAvaliacoes = g.Average(a => a.Nota)
                })
                .ToDictionaryAsync(x => x.EmpresaId, x => new { x.TotalAvaliacoes, x.MediaAvaliacoes });

            // Montar ranking
            var ranking = empresas.Select(e => new RankingEmpresaViewModel
            {
                EmpresaId = e.Id,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                LogoEmpresa = e.LogoEmpresa,
                AreaAtuacao = e.AreaAtuacao,
                Cidade = e.Cidade,
                Estado = e.Estado,
                TotalAvaliacoes = avaliacoesPorEmpresa.ContainsKey(e.Id) ? avaliacoesPorEmpresa[e.Id].TotalAvaliacoes : 0,
                MediaAvaliacoes = avaliacoesPorEmpresa.ContainsKey(e.Id) ? avaliacoesPorEmpresa[e.Id].MediaAvaliacoes : 0
            }).ToList();

            // Ordenar: primeiro por media, depois por quantidade
            ranking = ranking
                .OrderByDescending(r => r.MediaAvaliacoes)
                .ThenByDescending(r => r.TotalAvaliacoes)
                .Take(top)
                .ToList();

            // Atribuir posicoes
            for (int i = 0; i < ranking.Count; i++)
            {
                ranking[i].Posicao = i + 1;
            }

            return ranking;
        }
    }
}
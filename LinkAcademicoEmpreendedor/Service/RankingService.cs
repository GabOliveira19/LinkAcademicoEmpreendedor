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
        public async Task<List<RankingAlunoViewModel>> ObterRankingAlunosAsync(int top = 50, bool somenteNaoContratados = false)
        {
            var alunosQuery = _context.Alunos.AsQueryable();

            if (somenteNaoContratados)
            {
                alunosQuery = alunosQuery.Where(a => !_context.Candidaturas.Any(c =>
                    c.AlunoId == a.Id &&
                    (c.Status == "Aprovada" || c.Status == "Aceita")));
            }

            var alunos = await alunosQuery
                .Select(a => new
                {
                    a.Id,
                    a.Nome,
                    a.FotoPerfil,
                    a.Curso,
                    a.Instituicao
                })
                .ToListAsync();

            var projetosPorAluno = await _context.Projetos
                .Where(p => p.AlunoId != null)
                .GroupBy(p => p.AlunoId)
                .Select(g => new
                {
                    AlunoId = g.Key,
                    Total = g.Count()
                })
                .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            var comentariosRecebidosPorAluno = await _context.Comentarios
    .Include(c => c.Projeto)
    .Where(c =>
        c.Projeto != null &&
        c.Projeto.AlunoId != null &&
        c.AlunoId != null &&
        c.AlunoId != c.Projeto.AlunoId)
    .GroupBy(c => c.Projeto!.AlunoId)
    .Select(g => new
    {
        AlunoId = g.Key,
        Total = g.Select(c => c.AlunoId).Distinct().Count()
    })
    .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            var curtidasRecebidasPorAluno = await _context.Curtidas
                .Include(c => c.Projeto)
                .Where(c => c.Projeto != null && c.Projeto.AlunoId != null)
                .GroupBy(c => c.Projeto!.AlunoId)
                .Select(g => new
                {
                    AlunoId = g.Key,
                    Total = g.Count()
                })
                .ToDictionaryAsync(x => x.AlunoId, x => x.Total);

            var ranking = alunos.Select(a => new RankingAlunoViewModel
            {
                AlunoId = a.Id,
                Nome = a.Nome,
                FotoPerfil = a.FotoPerfil,
                Curso = a.Curso,
                Instituicao = a.Instituicao,
                TotalProjetos = projetosPorAluno.GetValueOrDefault(a.Id, 0),
                TotalComentarios = comentariosRecebidosPorAluno.GetValueOrDefault(a.Id, 0),
                TotalCurtidas = curtidasRecebidasPorAluno.GetValueOrDefault(a.Id, 0)
            }).ToList();

            foreach (var item in ranking)
            {
                item.CalcularPontuacao();
            }

            ranking = ranking
                .OrderByDescending(r => r.PontuacaoTotal)
                .Take(top)
                .ToList();

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

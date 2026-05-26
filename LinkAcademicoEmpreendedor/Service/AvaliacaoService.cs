using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Services
{
    public class AvaliacaoService
    {
        private readonly ApplicationDbContext _context;
        private static readonly TimeSpan PeriodoEntreAvaliacoes = TimeSpan.FromDays(182); // ~6 meses

        public AvaliacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verificar se ja avaliou (retorna a avaliacao mais recente)
        public async Task<Avaliacao?> ObterAvaliacaoExistenteAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado)
        {
            return await _context.Avaliacoes
                .Where(a => a.AvaliadorId == avaliadorId
                    && a.TipoAvaliador == tipoAvaliador
                    && a.AvaliadoId == avaliadoId
                    && a.TipoAvaliado == tipoAvaliado)
                .OrderByDescending(a => a.DataAvaliacao)
                .FirstOrDefaultAsync();
        }

        // Verifica existência de vínculo ativo (candidatura aprovada entre empresa e aluno)
        private async Task<Candidatura?> ObterVinculoAtivoAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado)
        {
            if (tipoAvaliador == "Empresa" && tipoAvaliado == "Aluno")
            {
                return await _context.Candidaturas
                    .Include(c => c.Oportunidade)
                    .Where(c => c.AlunoId == avaliadoId
                                && c.Oportunidade != null
                                && c.Oportunidade.EmpresaId == avaliadorId
                                && (c.Status == "Aceita" || c.Status == "Aprovada"))
                    .OrderByDescending(c => c.DataResposta)
                    .FirstOrDefaultAsync();
            }
            else if (tipoAvaliador == "Aluno" && tipoAvaliado == "Empresa")
            {
                return await _context.Candidaturas
                    .Include(c => c.Oportunidade)
                    .Where(c => c.AlunoId == avaliadorId
                                && c.Oportunidade != null
                                && c.Oportunidade.EmpresaId == avaliadoId
                                && (c.Status == "Aceita" || c.Status == "Aprovada"))
                    .OrderByDescending(c => c.DataResposta)
                    .FirstOrDefaultAsync();
            }

            // Para outros tipos, exigir vínculo não faz sentido no modelo atual
            return null;
        }

        // Criar ou atualizar avaliacao com regras: vinculo ativo, 1 avaliacao a cada 6 meses, bloqueio apos encerramento
        public async Task<Avaliacao> AvaliarAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado, int nota, string? comentario)
        {
            // Verificar autoavaliacao
            if (tipoAvaliador == tipoAvaliado && avaliadorId == avaliadoId)
            {
                throw new InvalidOperationException("Nao e permitido se autoavaliar.");
            }

            // Exigir vínculo ativo entre empresa e aluno quando aplicável
            var vinculo = await ObterVinculoAtivoAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado);
            if (vinculo == null)
            {
                throw new InvalidOperationException("Avaliação somente permitida enquanto existir um vínculo ativo (candidatura aprovada) entre as partes.");
            }

            // Buscar avaliacao mais recente do mesmo par avaliador/avaliado
            var avaliacaoExistente = await ObterAvaliacaoExistenteAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado);

            var agora = DateTime.Now;

            if (avaliacaoExistente != null)
            {
                var diferenca = agora - avaliacaoExistente.DataAvaliacao;

                // Se última avaliação está dentro do período (6 meses), permitimos apenas atualização da mesma entrada
                if (diferenca <= PeriodoEntreAvaliacoes)
                {
                    // Atualizar existente (protege contra duplicidade criando nova entrada)
                    avaliacaoExistente.Nota = nota;
                    avaliacaoExistente.Comentario = comentario;
                    avaliacaoExistente.DataAvaliacao = agora;

                    await _context.SaveChangesAsync();
                    return avaliacaoExistente;
                }
                else
                {
                    // Se passou o período, criar nova avaliação (histórico preservado)
                    var novaAvaliacao = new Avaliacao
                    {
                        AvaliadorId = avaliadorId,
                        TipoAvaliador = tipoAvaliador,
                        AvaliadoId = avaliadoId,
                        TipoAvaliado = tipoAvaliado,
                        Nota = nota,
                        Comentario = comentario,
                        DataAvaliacao = agora,
                        OportunidadeId = vinculo.OportunidadeId
                    };

                    _context.Avaliacoes.Add(novaAvaliacao);
                    await CriarNotificacaoAvaliacaoAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado, nota);
                    await _context.SaveChangesAsync();
                    return novaAvaliacao;
                }
            }
            else
            {
                // Nenhuma avaliacao anterior: criar nova (somente se vinculo ativo)
                var novaAvaliacao = new Avaliacao
                {
                    AvaliadorId = avaliadorId,
                    TipoAvaliador = tipoAvaliador,
                    AvaliadoId = avaliadoId,
                    TipoAvaliado = tipoAvaliado,
                    Nota = nota,
                    Comentario = comentario,
                    DataAvaliacao = agora,
                    OportunidadeId = vinculo.OportunidadeId
                };

                _context.Avaliacoes.Add(novaAvaliacao);
                await CriarNotificacaoAvaliacaoAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado, nota);
                await _context.SaveChangesAsync();
                return novaAvaliacao;
            }
        }

        private async Task CriarNotificacaoAvaliacaoAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado, int nota)
        {
            string nomeAvaliador = "";

            if (tipoAvaliador == "Aluno")
            {
                var aluno = await _context.Alunos.FindAsync(avaliadorId);
                nomeAvaliador = aluno?.Nome ?? "Aluno";
            }
            else
            {
                var empresa = await _context.Empresas.FindAsync(avaliadorId);
                nomeAvaliador = empresa?.RazaoSocial ?? "Empresa";
            }

            var Notificacao = new Notificacao
            {
                DestinatarioId = avaliadoId,
                TipoDestinatario = tipoAvaliado,
                Titulo = "Nova Avaliacao Recebida",
                Mensagem = $"{nomeAvaliador} avaliou voce com {nota} estrela(s).",
                Link = "/Avaliacao/MinhasAvaliacoes",
                Lida = false,
                DataCriacao = DateTime.Now
            };

            _context.Notificacoes.Add(Notificacao);
        }

        // Obter avaliacoes recebidas
        public async Task<List<Avaliacao>> ObterAvaliacoesRecebidasAsync(int avaliadoId, string tipoAvaliado)
        {
            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.AvaliadoId == avaliadoId && a.TipoAvaliado == tipoAvaliado)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync();

            // Carregar nomes dos avaliadores
            foreach (var avaliacao in avaliacoes)
            {
                if (avaliacao.TipoAvaliador == "Aluno")
                {
                    var aluno = await _context.Alunos.FindAsync(avaliacao.AvaliadorId);
                    avaliacao.NomeAvaliador = aluno?.Nome ?? "Aluno";
                    avaliacao.FotoAvaliador = aluno?.FotoPerfil;
                }
                else
                {
                    var empresa = await _context.Empresas.FindAsync(avaliacao.AvaliadorId);
                    avaliacao.NomeAvaliador = empresa?.RazaoSocial ?? "Empresa";
                    avaliacao.FotoAvaliador = empresa?.LogoEmpresa;
                }
            }

            return avaliacoes;
        }

        // Obter avaliacoes feitas
        public async Task<List<Avaliacao>> ObterAvaliacoesFeitasAsync(int avaliadorId, string tipoAvaliador)
        {
            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.AvaliadorId == avaliadorId && a.TipoAvaliador == tipoAvaliador)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync();

            // Carregar nomes dos avaliados
            foreach (var avaliacao in avaliacoes)
            {
                if (avaliacao.TipoAvaliado == "Aluno")
                {
                    var aluno = await _context.Alunos.FindAsync(avaliacao.AvaliadoId);
                    avaliacao.NomeAvaliado = aluno?.Nome ?? "Aluno";
                    avaliacao.FotoAvaliado = aluno?.FotoPerfil;
                }
                else
                {
                    var empresa = await _context.Empresas.FindAsync(avaliacao.AvaliadoId);
                    avaliacao.NomeAvaliado = empresa?.RazaoSocial ?? "Empresa";
                    avaliacao.FotoAvaliado = empresa?.LogoEmpresa;
                }
            }

            return avaliacoes;
        }

        // Obter estatisticas
        public async Task<(int total, double media)> ObterEstatisticasAsync(int avaliadoId, string tipoAvaliado)
        {
            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.AvaliadoId == avaliadoId && a.TipoAvaliado == tipoAvaliado)
                .ToListAsync();

            if (!avaliacoes.Any())
                return (0, 0);

            return (avaliacoes.Count, avaliacoes.Average(a => a.Nota));
        }
    }
}
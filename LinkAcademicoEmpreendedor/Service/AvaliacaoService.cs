using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Services
{
    public class AvaliacaoService
    {
        private readonly ApplicationDbContext _context;

        public AvaliacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verificar se ja avaliou
        public async Task<Avaliacao?> ObterAvaliacaoExistenteAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado)
        {
            return await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.AvaliadorId == avaliadorId
                    && a.TipoAvaliador == tipoAvaliador
                    && a.AvaliadoId == avaliadoId
                    && a.TipoAvaliado == tipoAvaliado);
        }

        // Criar ou atualizar avaliacao
        public async Task<Avaliacao> AvaliarAsync(int avaliadorId, string tipoAvaliador, int avaliadoId, string tipoAvaliado, int nota, string? comentario)
        {
            // Verificar autoavaliacao
            if (tipoAvaliador == tipoAvaliado && avaliadorId == avaliadoId)
            {
                throw new InvalidOperationException("Nao e permitido se autoavaliar.");
            }

            // Verificar se ja existe avaliacao
            var avaliacaoExistente = await ObterAvaliacaoExistenteAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado);

            if (avaliacaoExistente != null)
            {
                // Atualizar avaliacao existente
                avaliacaoExistente.Nota = nota;
                avaliacaoExistente.Comentario = comentario;
                avaliacaoExistente.DataAvaliacao = DateTime.Now;
                await _context.SaveChangesAsync();
                return avaliacaoExistente;
            }
            else
            {
                // Criar nova avaliacao
                var novaAvaliacao = new Avaliacao
                {
                    AvaliadorId = avaliadorId,
                    TipoAvaliador = tipoAvaliador,
                    AvaliadoId = avaliadoId,
                    TipoAvaliado = tipoAvaliado,
                    Nota = nota,
                    Comentario = comentario,
                    DataAvaliacao = DateTime.Now
                };

                _context.Avaliacoes.Add(novaAvaliacao);

                // Criar notificacao para o avaliado
                await CriarNotificacaoAvaliacaoAsync(avaliadorId, tipoAvaliador, avaliadoId, tipoAvaliado, nota);

                await _context.SaveChangesAsync();
                return novaAvaliacao;
            }
        }

        // Dentro do AvaliacaoService, substitua o metodo CriarNotificacaoAvaliacaoAsync:

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

            var notificacao = new Notificacao
            {
                DestinatarioId = avaliadoId,
                TipoDestinatario = tipoAvaliado,
                Titulo = "Nova Avaliacao Recebida",
                Mensagem = $"{nomeAvaliador} avaliou voce com {nota} estrela(s).",
                Link = "/Avaliacao/MinhasAvaliacoes",
                Lida = false,
                DataCriacao = DateTime.Now
            };

            _context.Notificacoes.Add(notificacao);
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
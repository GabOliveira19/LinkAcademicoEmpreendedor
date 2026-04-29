using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Services
{
    public class NotificacaoService
    {
        private readonly ApplicationDbContext _context;

        public NotificacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obter notificacoes do usuario
        public async Task<List<Notificacao>> ObterNotificacoesAsync(int destinatarioId, string tipoDestinatario, int limite = 20)
        {
            return await _context.Notificacoes
                .Where(n => n.DestinatarioId == destinatarioId && n.TipoDestinatario == tipoDestinatario)
                .OrderByDescending(n => n.DataCriacao)
                .Take(limite)
                .ToListAsync();
        }

        // Obter apenas nao lidas
        public async Task<List<Notificacao>> ObterNaoLidasAsync(int destinatarioId, string tipoDestinatario)
        {
            return await _context.Notificacoes
                .Where(n => n.DestinatarioId == destinatarioId && n.TipoDestinatario == tipoDestinatario && !n.Lida)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

        // Contar nao lidas
        public async Task<int> ContarNaoLidasAsync(int destinatarioId, string tipoDestinatario)
        {
            return await _context.Notificacoes
                .CountAsync(n => n.DestinatarioId == destinatarioId && n.TipoDestinatario == tipoDestinatario && !n.Lida);
        }

        // Marcar como lida
        public async Task MarcarComoLidaAsync(int notificacaoId)
        {
            var notificacao = await _context.Notificacoes.FindAsync(notificacaoId);
            if (notificacao != null)
            {
                notificacao.Lida = true;
                await _context.SaveChangesAsync();
            }
        }

        // Marcar todas como lidas
        public async Task MarcarTodasComoLidasAsync(int destinatarioId, string tipoDestinatario)
        {
            var notificacoes = await _context.Notificacoes
                .Where(n => n.DestinatarioId == destinatarioId && n.TipoDestinatario == tipoDestinatario && !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
            }

            await _context.SaveChangesAsync();
        }

        // Criar notificacao
        public async Task CriarAsync(int destinatarioId, string tipoDestinatario, string titulo, string mensagem, string? link = null)
        {
            var notificacao = new Notificacao
            {
                DestinatarioId = destinatarioId,
                TipoDestinatario = tipoDestinatario,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                Lida = false,
                DataCriacao = DateTime.Now
            };

            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();
        }
    }
}
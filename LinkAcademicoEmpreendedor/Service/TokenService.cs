using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkAcademicoEmpreendedor.Services
{
    public class TokenService
    {
        private readonly ApplicationDbContext _context;

        public TokenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> ConsultarSaldo(int alunoId)
        {
            var carteira = await _context.CarteirasToken
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.AlunoId == alunoId);

            return carteira?.Saldo ?? 0;
        }

        public async Task AdicionarTokens(int alunoId, int quantidade, string motivo)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantidade));

            var carteira = await _context.CarteirasToken
                .FirstOrDefaultAsync(c => c.AlunoId == alunoId);

            if (carteira == null)
            {
                carteira = new CarteiraToken
                {
                    AlunoId = alunoId,
                    Saldo = 0
                };

                _context.CarteirasToken.Add(carteira);
            }

            carteira.Saldo += quantidade;

            var movimentacao = new MovimentacaoToken
            {
                AlunoId = alunoId,
                Quantidade = quantidade,
                Tipo = motivo,
                Data = DateTime.Now
            };

            _context.MovimentacoesToken.Add(movimentacao);
            await _context.SaveChangesAsync();
        }

        public async Task DebitarTokens(int usuarioId, int quantidade, string motivo)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantidade));

            var carteira = await _context.CarteirasToken
                .FirstOrDefaultAsync(c => c.AlunoId == usuarioId);

            if (carteira == null)
                throw new InvalidOperationException("Carteira nao encontrada para o usuario.");

            if (carteira.Saldo < quantidade)
                throw new InvalidOperationException("Saldo insuficiente para o debito.");

            carteira.Saldo -= quantidade;

            var movimentacao = new MovimentacaoToken
            {
                AlunoId = usuarioId,
                Quantidade = -quantidade,
                Tipo = motivo,
                Data = DateTime.Now
            };

            _context.MovimentacoesToken.Add(movimentacao);
            await _context.SaveChangesAsync();
        }
    }
}
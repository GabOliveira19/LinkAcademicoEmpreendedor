using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;
using LinkAcademicoEmpreendedor.ViewModels;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class TokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly TokenService _tokenService;

        public TokenController(ApplicationDbContext context, MercadoPagoService mercadoPagoService, TokenService tokenService)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> PacoteToken()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            
            if (userId == null || tipoUsuario != "Aluno")
            {
                TempData["Erro"] = "Você precisa estar logado como Aluno para acessar esta página.";
                return RedirectToAction("Login", "Account");
            }

            var aluno = await _context.Alunos.FindAsync(userId);
            if (aluno == null || !aluno.EhEgresso)
            {
                TempData["Erro"] = "Apenas alunos egressos podem comprar SkillCoins.";
                return RedirectToAction("Dashboard", "Aluno");
            }

            var pacotes = await _context.PacotesToken
                .Where(p => p.Ativo)
                .OrderBy(p => p.QuantidadeTokens)
                .Select(p => new PacoteTokenItemViewModel
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    QuantidadeTokens = p.QuantidadeTokens,
                    Valor = p.Valor
                })
                .ToListAsync();

            var viewModel = new PacotesTokenViewModel
            {
                Pacotes = pacotes
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Historico()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var movimentacoes = await _context.MovimentacoesToken
                .Where(m => m.AlunoId == userId)
                .OrderByDescending(m => m.Data)
                .ToListAsync();

            return View(movimentacoes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarPagamento([FromBody] TokenPaymentRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return StatusCode(401, "Usuário não autenticado.");

            var aluno = await _context.Alunos.FindAsync(userId);
            if (aluno == null || !aluno.EhEgresso)
                return StatusCode(403, "Apenas alunos egressos podem comprar SkillCoins.");

            var pacote = await _context.PacotesToken
                .FirstOrDefaultAsync(p => p.Id == dto.PacoteId && p.Ativo);

            if (pacote == null)
                return NotFound();

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var compra = new CompraToken
                    {
                        UsuarioId = aluno.Id,
                        PacoteTokenId = pacote.Id,
                        Valor = pacote.Valor,
                        QuantidadeTokens = pacote.QuantidadeTokens,
                        StatusPagamento = StatusPagamentoCompraToken.Pendente,
                        DataCriacao = DateTime.Now
                    };

                    _context.ComprasToken.Add(compra);
                    await _context.SaveChangesAsync();

                    var payment = await _mercadoPagoService.ProcessarPagamentoTransparenteAsync(
                        dto,
                        $"COMPRA_TOKEN_{compra.Id}");

                    compra.MercadoPagoPaymentId = payment.Id.ToString();
                    
                    if (payment.Status == "approved")
                    {
                        compra.StatusPagamento = StatusPagamentoCompraToken.Aprovado;
                        
                        var carteira = await _context.CarteirasToken.FirstOrDefaultAsync(c => c.AlunoId == aluno.Id);
                        if (carteira == null)
                        {
                            carteira = new CarteiraToken { AlunoId = aluno.Id, Saldo = 0 };
                            _context.CarteirasToken.Add(carteira);
                        }
                        carteira.Saldo += compra.QuantidadeTokens;

                        var movimentacao = new MovimentacaoToken
                        {
                            AlunoId = aluno.Id,
                            Quantidade = compra.QuantidadeTokens,
                            Data = DateTime.Now,
                            Tipo = "Compra"
                        };
                        _context.MovimentacoesToken.Add(movimentacao);
                    }
                    else if (payment.Status == "rejected")
                    {
                        compra.StatusPagamento = StatusPagamentoCompraToken.Recusado;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var successUrl = Url.Action("PagamentoSucesso", "Token", new { compraTokenId = compra.Id }, Request.Scheme);
                    var pendingUrl = Url.Action("PagamentoPendente", "Token", new { compraTokenId = compra.Id }, Request.Scheme);
                    var failureUrl = Url.Action("PagamentoFalha", "Token", new { compraTokenId = compra.Id }, Request.Scheme);

                    string redirectUrl = pendingUrl;
                    if (payment.Status == "approved") redirectUrl = successUrl;
                    if (payment.Status == "rejected") redirectUrl = failureUrl;

                    return Json(new { 
                        status = payment.Status, 
                        status_detail = payment.StatusDetail, 
                        id = payment.Id,
                        redirectUrl = redirectUrl,
                        qrCode = payment.PointOfInteraction?.TransactionData?.QrCode,
                        qrCodeBase64 = payment.PointOfInteraction?.TransactionData?.QrCodeBase64
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Nao foi possivel iniciar o pagamento. Detalhes: " + ex.Message + " | " + (ex.InnerException?.Message ?? ""));
                }
            });
        }

        public IActionResult PagamentoSucesso(int compraTokenId)
        {
            ViewBag.CompraTokenId = compraTokenId;
            return View();
        }

        public async Task<IActionResult> PagamentoPendente(int compraTokenId)
        {
            var compra = await _context.ComprasToken.FindAsync(compraTokenId);
            if (compra != null)
            {
                // Se ainda está pendente mas temos o ID no Mercado Pago, consultamos a API diretamente
                if (compra.StatusPagamento == StatusPagamentoCompraToken.Pendente && !string.IsNullOrEmpty(compra.MercadoPagoPaymentId))
                {
                    if (long.TryParse(compra.MercadoPagoPaymentId, out var paymentId))
                    {
                        var payment = await _mercadoPagoService.ObterPagamentoAsync(paymentId);
                        if (payment != null)
                        {
                            if (payment.Status == "approved")
                            {
                                compra.StatusPagamento = StatusPagamentoCompraToken.Aprovado;
                                compra.DataPagamento = DateTime.Now;
                                await _tokenService.AdicionarTokens(compra.UsuarioId, compra.QuantidadeTokens, "Compra de SkillCoins (Pedido " + compra.Id + ")");
                                await _context.SaveChangesAsync();
                            }
                            else if (payment.Status == "rejected" || payment.Status == "cancelled")
                            {
                                compra.StatusPagamento = StatusPagamentoCompraToken.Recusado;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                if (compra.StatusPagamento == StatusPagamentoCompraToken.Aprovado)
                    return RedirectToAction("PagamentoSucesso", new { compraTokenId });
                
                if (compra.StatusPagamento == StatusPagamentoCompraToken.Recusado || compra.StatusPagamento == StatusPagamentoCompraToken.Cancelado)
                    return RedirectToAction("PagamentoFalha", new { compraTokenId });
            }

            ViewBag.CompraTokenId = compraTokenId;
            return View();
        }

        public IActionResult PagamentoFalha(int compraTokenId)
        {
            ViewBag.CompraTokenId = compraTokenId;
            return View();
        }

        public async Task<IActionResult> Compras(string? status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var query = _context.ComprasToken
                .Include(c => c.PacoteToken)
                .Where(c => c.UsuarioId == userId)
                .OrderByDescending(c => c.DataCriacao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status.Equals("pendente", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.StatusPagamento == StatusPagamentoCompraToken.Pendente);
                ViewBag.ApenasPendentes = true;
            }

            var compras = await query.ToListAsync();
            return View(compras);
        }
    }
}

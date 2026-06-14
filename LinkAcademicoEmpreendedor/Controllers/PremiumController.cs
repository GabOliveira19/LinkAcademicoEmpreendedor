using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class PremiumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MercadoPagoService _mercadoPagoService;

        public PremiumController(ApplicationDbContext context, MercadoPagoService mercadoPagoService)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
        }

        public async Task<IActionResult> Planos()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int nivelAtual = 0;

            if (userId != null && HttpContext.Session.GetString("TipoUsuario") == "Empresa")
            {
                var empresa = await _context.Empresas
                    .Include(e => e.AssinaturasPremium)
                        .ThenInclude(a => a.PlanoPremium)
                    .FirstOrDefaultAsync(e => e.Id == userId);

                if (empresa != null)
                {
                    nivelAtual = LinkAcademicoEmpreendedor.Helpers.PremiumHelper.ObterNivelPremium(empresa);
                }
            }

            ViewBag.NivelAtual = nivelAtual;
            var planos = await _context.PlanosPremium.OrderBy(p => p.Ordem).ToListAsync();
            return View(planos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assinar([FromBody] LinkAcademicoEmpreendedor.ViewModels.PremiumPaymentRequestDto dto)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            if (empresaId == null || HttpContext.Session.GetString("TipoUsuario") != "Empresa")
                return StatusCode(401, "Usuário não autenticado como Empresa.");

            var plano = await _context.PlanosPremium.FindAsync(dto.PlanoId);
            if (plano == null)
                return NotFound();

            var assinatura = new AssinaturaPremium
            {
                EmpresaId = empresaId.Value,
                PlanoPremiumId = plano.Id,
                Status = "Pendente"
            };

            _context.AssinaturasPremium.Add(assinatura);
            await _context.SaveChangesAsync();

            try
            {
                var payment = await _mercadoPagoService.ProcessarPagamentoTransparenteAsync(
                    dto,
                    $"PREMIUM_{assinatura.Id}");

                assinatura.MercadoPagoPaymentId = payment.Id.ToString();

                if (payment.Status == "approved")
                {
                    assinatura.Status = "Ativo";
                    assinatura.Inicio = DateTime.Now;
                    assinatura.Fim = DateTime.Now.AddDays(30);
                }
                else if (payment.Status == "rejected")
                {
                    assinatura.Status = "Falhou";
                }

                await _context.SaveChangesAsync();

                var successUrl = Url.Action("PagamentoSucesso", "Premium", new { assinaturaId = assinatura.Id }, Request.Scheme);
                var pendingUrl = Url.Action("PagamentoPendente", "Premium", new { assinaturaId = assinatura.Id }, Request.Scheme);
                var failureUrl = Url.Action("PagamentoFalha", "Premium", new { assinaturaId = assinatura.Id }, Request.Scheme);

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
                return StatusCode(500, "Nao foi possivel iniciar o pagamento. Detalhes: " + ex.Message + " | " + (ex.InnerException?.Message ?? ""));
            }
        }

        public IActionResult PagamentoSucesso(int assinaturaId)
        {
            ViewBag.AssinaturaId = assinaturaId;
            return View();
        }

        public async Task<IActionResult> PagamentoPendente(int assinaturaId)
        {
            var assinatura = await _context.AssinaturasPremium.FindAsync(assinaturaId);
            if (assinatura != null)
            {
                if (assinatura.Status == "Pendente" && !string.IsNullOrEmpty(assinatura.MercadoPagoPaymentId))
                {
                    if (long.TryParse(assinatura.MercadoPagoPaymentId, out var paymentId))
                    {
                        var payment = await _mercadoPagoService.ObterPagamentoAsync(paymentId);
                        if (payment != null)
                        {
                            if (payment.Status == "approved")
                            {
                                assinatura.Status = "Ativo";
                                assinatura.Inicio = DateTime.Now;
                                assinatura.Fim = DateTime.Now.AddMonths(1);
                                await _context.SaveChangesAsync();
                            }
                            else if (payment.Status == "rejected" || payment.Status == "cancelled")
                            {
                                assinatura.Status = "Falhou";
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                if (assinatura.Status == "Ativo")
                    return RedirectToAction("PagamentoSucesso", new { assinaturaId });
                
                if (assinatura.Status == "Falhou")
                    return RedirectToAction("PagamentoFalha", new { assinaturaId });
            }

            ViewBag.AssinaturaId = assinaturaId;
            return View();
        }

        public IActionResult PagamentoFalha(int assinaturaId)
        {
            ViewBag.AssinaturaId = assinaturaId;
            return View();
        }
    }
}
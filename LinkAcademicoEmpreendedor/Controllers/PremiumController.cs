using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class PremiumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PremiumController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Planos()
        {
            var planos = await _context.PlanosPremium.OrderBy(p => p.Ordem).ToListAsync();
            return View(planos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assinar(int planoId)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            if (empresaId == null || HttpContext.Session.GetString("TipoUsuario") != "Empresa")
                return RedirectToAction("Login", "Account");

            var plano = await _context.PlanosPremium.FindAsync(planoId);
            if (plano == null)
                return NotFound();

            var pix = $"PIX-DEMONSTRACAO|PLANO={plano.Nome}|VALOR={plano.ValorMensal:N2}|ID={Guid.NewGuid()}";
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(pix, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20);

            var assinatura = new AssinaturaPremium
            {
                EmpresaId = empresaId.Value,
                PlanoPremiumId = plano.Id,
                Inicio = DateTime.Now,
                Fim = DateTime.Now.AddMonths(1),
                Status = "Pendente",
                PixCopiaECola = pix,
                PixQrCodeBase64 = Convert.ToBase64String(qrBytes)
            };

            _context.AssinaturasPremium.Add(assinatura);
            await _context.SaveChangesAsync();

            return RedirectToAction("Pagamento", new { id = assinatura.Id });
        }

        public async Task<IActionResult> Pagamento(int id)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");

            var assinatura = await _context.AssinaturasPremium
                .Include(a => a.PlanoPremium)
                .FirstOrDefaultAsync(a => a.Id == id && a.EmpresaId == empresaId);

            if (assinatura == null)
                return NotFound();

            return View(assinatura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagamento(int id)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");

            var assinatura = await _context.AssinaturasPremium
                .FirstOrDefaultAsync(a => a.Id == id && a.EmpresaId == empresaId);

            if (assinatura == null)
                return NotFound();

            assinatura.Status = "Ativa";
            assinatura.Inicio = DateTime.Now;
            assinatura.Fim = DateTime.Now.AddMonths(1);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Pagamento confirmado em modo demonstração. Plano premium ativado!";
            return RedirectToAction("Dashboard", "Empresa");
        }
    }
}
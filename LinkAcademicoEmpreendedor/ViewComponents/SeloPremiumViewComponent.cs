using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LinkAcademicoEmpreendedor.ViewComponents
{
    public class SeloPremiumViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SeloPremiumViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int empresaId)
        {
            var assinatura = await _context.AssinaturasPremium
                .Include(a => a.PlanoPremium)
                .Where(a => a.EmpresaId == empresaId && a.Status == "Ativo" && a.Fim > DateTime.Now)
                .OrderByDescending(a => a.PlanoPremium!.Ordem)
                .FirstOrDefaultAsync();

            var nivel = assinatura?.PlanoPremium?.Nome switch
            {
                "Core" => 1,
                "Advanced" => 2,
                "Advanced Plus" => 3,
                _ => 0
            };

            return View("~/Views/Shared/_SeloPremium.cshtml", nivel);
        }
    }
}

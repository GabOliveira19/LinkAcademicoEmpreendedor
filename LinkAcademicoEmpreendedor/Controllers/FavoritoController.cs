using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class FavoritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarVaga(int oportunidadeId, string? returnUrl)
        {
            var alunoId = HttpContext.Session.GetInt32("UserId");
            if (alunoId == null || HttpContext.Session.GetString("TipoUsuario") != "Aluno")
                return RedirectToAction("Login", "Account");

            var favorito = await _context.FavoritosVagas
                .FirstOrDefaultAsync(f => f.AlunoId == alunoId.Value && f.OportunidadeId == oportunidadeId);

            if (favorito == null)
            {
                _context.FavoritosVagas.Add(new FavoritoVaga
                {
                    AlunoId = alunoId.Value,
                    OportunidadeId = oportunidadeId
                });
                TempData["Sucesso"] = "Vaga salva nos favoritos.";
            }
            else
            {
                _context.FavoritosVagas.Remove(favorito);
                TempData["Sucesso"] = "Vaga removida dos favoritos.";
            }

            await _context.SaveChangesAsync();
            return RedirecionarSeguro(returnUrl, RedirectToAction("Index", "Oportunidade"));
        }

        public async Task<IActionResult> MinhasVagas()
        {
            var alunoId = HttpContext.Session.GetInt32("UserId");
            if (alunoId == null || HttpContext.Session.GetString("TipoUsuario") != "Aluno")
                return RedirectToAction("Login", "Account");

            var favoritos = await _context.FavoritosVagas
                .Include(f => f.Oportunidade)
                    .ThenInclude(o => o!.Empresa)
                .Where(f => f.AlunoId == alunoId.Value)
                .OrderByDescending(f => f.CriadoEm)
                .ToListAsync();

            return View(favoritos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarTalento(int alunoId, string? returnUrl)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            if (empresaId == null || HttpContext.Session.GetString("TipoUsuario") != "Empresa")
                return RedirectToAction("Login", "Account");

            var favorito = await _context.FavoritosTalentos
                .FirstOrDefaultAsync(f => f.EmpresaId == empresaId.Value && f.AlunoId == alunoId);

            if (favorito == null)
            {
                _context.FavoritosTalentos.Add(new FavoritoTalento
                {
                    EmpresaId = empresaId.Value,
                    AlunoId = alunoId
                });
                TempData["Sucesso"] = "Talento salvo nos favoritos.";
            }
            else
            {
                _context.FavoritosTalentos.Remove(favorito);
                TempData["Sucesso"] = "Talento removido dos favoritos.";
            }

            await _context.SaveChangesAsync();
            return RedirecionarSeguro(returnUrl, RedirectToAction("BuscarTalentos", "Empresa"));
        }

        public async Task<IActionResult> MeusTalentos()
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            if (empresaId == null || HttpContext.Session.GetString("TipoUsuario") != "Empresa")
                return RedirectToAction("Login", "Account");

            var favoritos = await _context.FavoritosTalentos
                .Include(f => f.Aluno)
                    .ThenInclude(a => a!.Projetos)
                .Where(f => f.EmpresaId == empresaId.Value)
                .OrderByDescending(f => f.CriadoEm)
                .ToListAsync();

            return View(favoritos);
        }

        private IActionResult RedirecionarSeguro(string? returnUrl, IActionResult fallback)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return fallback;
        }
    }
}

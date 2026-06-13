using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class OportunidadeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OportunidadeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listar oportunidades
        public async Task<IActionResult> Index(string busca, string tipo, string modalidade)
        {
            var query = _context.Oportunidades
                .Include(o => o.Empresa)
                .Include(o => o.Candidaturas)
                .Where(o => o.Ativa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                query = query.Where(o =>
                    o.Titulo.Contains(busca) ||
                    o.Descricao.Contains(busca) ||
                    o.Empresa.RazaoSocial.Contains(busca));
            }

            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(o => o.Tipo == tipo);
            }

            if (!string.IsNullOrEmpty(modalidade))
            {
                query = query.Where(o => o.Modalidade == modalidade);
            }

            var oportunidades = await query.ToListAsync();

            var empresasPremium = await _context.AssinaturasPremium
                .Include(a => a.PlanoPremium)
                .Where(a => a.Status == "Ativa" && a.Fim >= DateTime.Now)
                .ToListAsync();

            var niveisPremiumPorEmpresa = empresasPremium
                .GroupBy(a => a.EmpresaId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(a => a.PlanoPremium != null ? a.PlanoPremium.Ordem : 0)
                );

            oportunidades = oportunidades
                .OrderByDescending(o => niveisPremiumPorEmpresa.ContainsKey(o.EmpresaId)
                    ? niveisPremiumPorEmpresa[o.EmpresaId]
                    : 0)
                .ThenByDescending(o => o.DataPublicacao)
                .ToList();

            ViewBag.NiveisPremiumPorEmpresa = niveisPremiumPorEmpresa;

            ViewBag.Busca = busca;
            ViewBag.Tipo = tipo;
            ViewBag.Modalidade = modalidade;

            return View(oportunidades);
        }

        // Detalhes da oportunidade
        public async Task<IActionResult> Detalhes(int id)
        {
            var oportunidade = await _context.Oportunidades
                .Include(o => o.Empresa)
                .Include(o => o.Candidaturas)
                    .ThenInclude(c => c.Aluno)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (oportunidade == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            ViewBag.JaCandidatou = false;
            if (userId != null && tipoUsuario == "Aluno")
            {
                ViewBag.JaCandidatou = await _context.Candidaturas
                    .AnyAsync(c => c.OportunidadeId == id && c.AlunoId == userId);
            }

            ViewBag.UserId = userId;
            ViewBag.TipoUsuario = tipoUsuario;

            return View(oportunidade);
        }

        // Criar oportunidade - GET
        public IActionResult Criar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // Criar oportunidade - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Oportunidade oportunidade)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Empresa");
            ModelState.Remove("Candidaturas");

            if (!ModelState.IsValid)
                return View(oportunidade);

            oportunidade.EmpresaId = userId.Value;
            oportunidade.DataPublicacao = DateTime.Now;
            oportunidade.Ativa = true;

            _context.Oportunidades.Add(oportunidade);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Oportunidade publicada com sucesso!";
            return RedirectToAction("Dashboard", "Empresa");
        }

        // Editar oportunidade - GET
        public async Task<IActionResult> Editar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var oportunidade = await _context.Oportunidades
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == userId);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Oportunidade nao encontrada.";
                return RedirectToAction("Dashboard", "Empresa");
            }

            return View(oportunidade);
        }

        // Editar oportunidade - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Oportunidade model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var oportunidade = await _context.Oportunidades
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == userId);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Oportunidade nao encontrada.";
                return RedirectToAction("Dashboard", "Empresa");
            }

            ModelState.Remove("Empresa");
            ModelState.Remove("Candidaturas");

            if (!ModelState.IsValid)
                return View(model);

            oportunidade.Titulo = model.Titulo;
            oportunidade.Descricao = model.Descricao;
            oportunidade.Requisitos = model.Requisitos;
            oportunidade.Tipo = model.Tipo;
            oportunidade.Modalidade = model.Modalidade;
            oportunidade.Salario = model.Salario;
            oportunidade.Ativa = model.Ativa;
            oportunidade.CustoCandidatura = model.CustoCandidatura;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Oportunidade atualizada com sucesso!";
            return RedirectToAction("Dashboard", "Empresa");
        }

        // Excluir oportunidade - GET
        public async Task<IActionResult> Excluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var oportunidade = await _context.Oportunidades
                .Include(o => o.Empresa)
                .Include(o => o.Candidaturas)
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == userId);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Vaga nao encontrada.";
                return RedirectToAction("Dashboard", "Empresa");
            }

            return View(oportunidade);
        }

        // Excluir oportunidade - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarExcluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var oportunidade = await _context.Oportunidades
                .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == userId);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Vaga nao encontrada.";
                return RedirectToAction("Dashboard", "Empresa");
            }

            try
            {
                // Excluir candidaturas da oportunidade
                var candidaturas = await _context.Candidaturas
                    .Where(c => c.OportunidadeId == id)
                    .ToListAsync();
                if (candidaturas.Any())
                    _context.Candidaturas.RemoveRange(candidaturas);

                // Excluir a oportunidade
                _context.Oportunidades.Remove(oportunidade);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Vaga excluida com sucesso!";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir a vaga. Tente novamente.";
            }

            return RedirectToAction("Dashboard", "Empresa");
        }
        // GET: /Oportunidade/MinhasOportunidades
        public async Task<IActionResult> MinhasOportunidades()
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
            {
                return RedirectToAction("Login", "Account");
            }

            var oportunidades = await _context.Oportunidades
                .Include(o => o.Candidaturas)
                .ThenInclude(c => c.Aluno)
                .Where(o => o.EmpresaId == empresaId)
                .OrderByDescending(o => o.DataPublicacao)
                .ToListAsync();

            return View(oportunidades);
        }
    }
}

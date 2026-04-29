using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class AvaliacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AvaliacaoService _avaliacaoService;

        public AvaliacaoController(ApplicationDbContext context)
        {
            _context = context;
            _avaliacaoService = new AvaliacaoService(context);
        }

        // GET: /Avaliacao/AvaliarAluno?alunoId=1
        [HttpGet]
        public async Task<IActionResult> AvaliarAluno(int alunoId)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
            {
                TempData["Erro"] = "Acesso nao autorizado.";
                return RedirectToAction("Login", "Account");
            }

            var aluno = await _context.Alunos.FindAsync(alunoId);
            if (aluno == null)
            {
                TempData["Erro"] = "Aluno nao encontrado.";
                return RedirectToAction("Dashboard", "Empresa");
            }

            // Verificar se ja avaliou
            var avaliacaoExistente = await _avaliacaoService.ObterAvaliacaoExistenteAsync(
                empresaId.Value, "Empresa", alunoId, "Aluno");

            ViewBag.Aluno = aluno;
            ViewBag.AvaliacaoExistente = avaliacaoExistente;
            return View();
        }

        // POST: /Avaliacao/AvaliarAluno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvaliarAluno(int alunoId, int nota, string? comentario)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
            {
                TempData["Erro"] = "Acesso nao autorizado.";
                return RedirectToAction("Login", "Account");
            }

            if (nota < 1 || nota > 5)
            {
                TempData["Erro"] = "Nota invalida. Selecione de 1 a 5 estrelas.";
                return RedirectToAction("AvaliarAluno", new { alunoId });
            }

            try
            {
                await _avaliacaoService.AvaliarAsync(empresaId.Value, "Empresa", alunoId, "Aluno", nota, comentario);
                TempData["Sucesso"] = "Avaliacao enviada com sucesso!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Dashboard", "Empresa");
        }

        // GET: /Avaliacao/AvaliarEmpresa?empresaId=1
        [HttpGet]
        public async Task<IActionResult> AvaliarEmpresa(int empresaId)
        {
            var alunoId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (alunoId == null || tipoUsuario != "Aluno")
            {
                TempData["Erro"] = "Acesso nao autorizado.";
                return RedirectToAction("Login", "Account");
            }

            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                TempData["Erro"] = "Empresa nao encontrada.";
                return RedirectToAction("Dashboard", "Aluno");
            }

            // Verificar se ja avaliou
            var avaliacaoExistente = await _avaliacaoService.ObterAvaliacaoExistenteAsync(
                alunoId.Value, "Aluno", empresaId, "Empresa");

            ViewBag.Empresa = empresa;
            ViewBag.AvaliacaoExistente = avaliacaoExistente;
            return View();
        }

        // POST: /Avaliacao/AvaliarEmpresa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvaliarEmpresa(int empresaId, int nota, string? comentario)
        {
            var alunoId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (alunoId == null || tipoUsuario != "Aluno")
            {
                TempData["Erro"] = "Acesso nao autorizado.";
                return RedirectToAction("Login", "Account");
            }

            if (nota < 1 || nota > 5)
            {
                TempData["Erro"] = "Nota invalida. Selecione de 1 a 5 estrelas.";
                return RedirectToAction("AvaliarEmpresa", new { empresaId });
            }

            try
            {
                await _avaliacaoService.AvaliarAsync(alunoId.Value, "Aluno", empresaId, "Empresa", nota, comentario);
                TempData["Sucesso"] = "Avaliacao enviada com sucesso!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Dashboard", "Aluno");
        }

        // GET: /Avaliacao/MinhasAvaliacoes
        public async Task<IActionResult> MinhasAvaliacoes()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return RedirectToAction("Login", "Account");

            var avaliacoes = await _avaliacaoService.ObterAvaliacoesRecebidasAsync(userId.Value, tipoUsuario);
            var (total, media) = await _avaliacaoService.ObterEstatisticasAsync(userId.Value, tipoUsuario);

            ViewBag.TotalAvaliacoes = total;
            ViewBag.MediaAvaliacoes = media;
            ViewBag.TipoUsuario = tipoUsuario;

            return View(avaliacoes);
        }

        // GET: /Avaliacao/AvaliacoesFeitas
        public async Task<IActionResult> AvaliacoesFeitas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return RedirectToAction("Login", "Account");

            var avaliacoes = await _avaliacaoService.ObterAvaliacoesFeitasAsync(userId.Value, tipoUsuario);
            ViewBag.TipoUsuario = tipoUsuario;

            return View(avaliacoes);
        }
    }
}
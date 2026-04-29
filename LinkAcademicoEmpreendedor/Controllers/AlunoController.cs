using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.ViewModels;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class AlunoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlunoController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Dashboard do Aluno
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var aluno = await _context.Alunos
                .Include(a => a.Projetos)
                    .ThenInclude(p => p.Curtidas)
                .Include(a => a.Projetos)
                    .ThenInclude(p => p.Comentarios)
                .FirstOrDefaultAsync(a => a.Id == userId);

            if (aluno == null)
                return RedirectToAction("Login", "Account");

            var oportunidades = await _context.Oportunidades
                .Include(o => o.Empresa)
                .Where(o => o.Ativa)
                .OrderByDescending(o => o.DataPublicacao)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardAlunoViewModel
            {
                Aluno = aluno,
                MeusProjetos = aluno.Projetos.ToList(),
                OportunidadesRecentes = oportunidades,
                TotalCurtidas = aluno.Projetos.Sum(p => p.Curtidas.Count),
                TotalComentarios = aluno.Projetos.Sum(p => p.Comentarios.Count)
            };

            return View(viewModel);
        }

        // Perfil do Aluno (publico)
        public async Task<IActionResult> Perfil(int id)
        {
            var aluno = await _context.Alunos
                .Include(a => a.Projetos)
                    .ThenInclude(p => p.Curtidas)
                .Include(a => a.Projetos)
                    .ThenInclude(p => p.Comentarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluno == null)
                return NotFound();

            return View(aluno);
        }

        // Editar Perfil
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var aluno = await _context.Alunos.FindAsync(userId);
            if (aluno == null)
                return NotFound();

            return View(aluno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Aluno model, IFormFile? fotoPerfil)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != model.Id)
                return RedirectToAction("Login", "Account");

            var aluno = await _context.Alunos.FindAsync(userId);
            if (aluno == null)
                return NotFound();

            // Upload da foto
            if (fotoPerfil != null && fotoPerfil.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "alunos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Deletar foto antiga se existir
                if (!string.IsNullOrEmpty(aluno.FotoPerfil))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, aluno.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(fotoPerfil.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fotoPerfil.CopyToAsync(stream);
                }

                aluno.FotoPerfil = $"/uploads/alunos/{fileName}";
                HttpContext.Session.SetString("UserFoto", aluno.FotoPerfil);
            }

            aluno.Nome = model.Nome;
            aluno.Curso = model.Curso;
            aluno.Instituicao = model.Instituicao;
            aluno.AnoIngresso = model.AnoIngresso;
            aluno.Sobre = model.Sobre;
            aluno.Habilidades = model.Habilidades;
            aluno.LinkedIn = model.LinkedIn;
            aluno.GitHub = model.GitHub;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", aluno.Nome);
            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Dashboard");
        }

        // Remover foto de perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverFoto()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var aluno = await _context.Alunos.FindAsync(userId);
            if (aluno == null)
                return NotFound();

            if (!string.IsNullOrEmpty(aluno.FotoPerfil))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, aluno.FotoPerfil.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                aluno.FotoPerfil = null;
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserFoto", "");
            }

            TempData["Sucesso"] = "Foto removida com sucesso!";
            return RedirectToAction("EditarPerfil");
        }

        // Listar todos os alunos
        public async Task<IActionResult> Listar(string busca, string habilidade)
        {
            var query = _context.Alunos
                .Include(a => a.Projetos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                query = query.Where(a =>
                    a.Nome.Contains(busca) ||
                    (a.Curso != null && a.Curso.Contains(busca)) ||
                    (a.Instituicao != null && a.Instituicao.Contains(busca)));
            }

            if (!string.IsNullOrEmpty(habilidade))
            {
                query = query.Where(a => a.Habilidades != null && a.Habilidades.Contains(habilidade));
            }

            var alunos = await query.OrderBy(a => a.Nome).ToListAsync();

            ViewBag.Busca = busca;
            ViewBag.Habilidade = habilidade;

            return View(alunos);
        }
    }
}
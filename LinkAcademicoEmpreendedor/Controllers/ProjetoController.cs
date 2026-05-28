using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;
using Microsoft.AspNetCore.Hosting;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class ProjetoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DynamicFormService _dynamicFormService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjetoController(
            ApplicationDbContext context,
            DynamicFormService dynamicFormService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _dynamicFormService = dynamicFormService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string busca, string tipo, string tecnologia, string area)
        {
            var query = _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .Include(p => p.Links)
                .Where(p => p.Ativo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                query = query.Where(p =>
                    p.Titulo.Contains(busca) ||
                    p.Descricao.Contains(busca) ||
                    p.Aluno!.Nome.Contains(busca));
            }

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(p => p.Tipo == tipo);

            if (!string.IsNullOrEmpty(tecnologia))
                query = query.Where(p => p.Tecnologias!.Contains(tecnologia));

            if (!string.IsNullOrEmpty(area))
                query = query.Where(p => p.Area!.Contains(area));

            var projetos = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return View(projetos);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var projeto = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios).ThenInclude(c => c.Aluno)
                .Include(p => p.Comentarios).ThenInclude(c => c.Empresa)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            ViewBag.UserId = userId;
            ViewBag.TipoUsuario = tipoUsuario;

            if (userId != null)
            {
                ViewBag.JaCurtiu = await _context.Curtidas.AnyAsync(c =>
                    c.ProjetoId == id &&
                    ((tipoUsuario == "Aluno" && c.AlunoId == userId) ||
                     (tipoUsuario == "Empresa" && c.EmpresaId == userId)));
            }

            return View(projeto);
        }

        public IActionResult Criar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var aluno = _context.Alunos
                .Include(a => a.Area)
                .FirstOrDefault(a => a.Id == userId);

            var projeto = new Projeto
            {
                Area = aluno?.Area?.Nome
            };

            ViewBag.DynamicFieldsDefinition = GetDynamicFieldDefinitions(projeto.Area);
            ViewBag.DadosDinamicos = new Dictionary<string, string>();

            return View(projeto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Projeto projeto, IFormFile? pdfFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Aluno");
            ModelState.Remove("Curtidas");
            ModelState.Remove("Comentarios");
            ModelState.Remove("Links");

            var aluno = await _context.Alunos
                .Include(a => a.Area)
                .FirstOrDefaultAsync(a => a.Id == userId);

            projeto.AlunoId = userId.Value;
            projeto.Area = aluno?.Area?.Nome;
            projeto.DataCriacao = DateTime.Now;
            projeto.Ativo = true;

            var dynamicFields = new Dictionary<string, string>();

            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                    dynamicFields[key.Replace("campo_", "")] = Request.Form[key]!;
            }

            projeto.DadosDinamicos = dynamicFields;

            if (pdfFile != null && pdfFile.Length > 0)
            {
                var pastaPdf = Path.Combine(_webHostEnvironment.WebRootPath, "pdfs");

                if (!Directory.Exists(pastaPdf))
                    Directory.CreateDirectory(pastaPdf);

                var nomeArquivo = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
                var caminhoCompleto = Path.Combine(pastaPdf, nomeArquivo);

                using var stream = new FileStream(caminhoCompleto, FileMode.Create);
                await pdfFile.CopyToAsync(stream);

                projeto.ArquivoPdf = "/pdfs/" + nomeArquivo;
            }

            var links = projeto.Links?
                .Where(l => !string.IsNullOrWhiteSpace(l.Url))
                .ToList() ?? new();

            projeto.Links.Clear();

            _context.Projetos.Add(projeto);
            await _context.SaveChangesAsync();

            foreach (var link in links)
            {
                link.ProjetoId = projeto.Id;
                _context.ProjetoLinks.Add(link);
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Projeto criado com sucesso!";
            return RedirectToAction("Dashboard", "Aluno");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            ViewBag.DynamicFieldsDefinition = GetDynamicFieldDefinitions(projeto.Area);
            ViewBag.DadosDinamicos = projeto.DadosDinamicos;

            return View(projeto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Projeto model, IFormFile? pdfFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            projeto.Titulo = model.Titulo;
            projeto.Descricao = model.Descricao;
            projeto.Tipo = model.Tipo;

            var dynamicFields = new Dictionary<string, string>();

            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                    dynamicFields[key.Replace("campo_", "")] = Request.Form[key]!;
            }

            projeto.DadosDinamicos = dynamicFields;

            if (pdfFile != null && pdfFile.Length > 0)
            {
                var pastaPdf = Path.Combine(_webHostEnvironment.WebRootPath, "pdfs");

                if (!Directory.Exists(pastaPdf))
                    Directory.CreateDirectory(pastaPdf);

                if (!string.IsNullOrEmpty(projeto.ArquivoPdf))
                {
                    var antigoPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        projeto.ArquivoPdf.TrimStart('/').Replace("/", "\\"));

                    if (System.IO.File.Exists(antigoPath))
                        System.IO.File.Delete(antigoPath);
                }

                var nomeArquivo = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
                var caminhoCompleto = Path.Combine(pastaPdf, nomeArquivo);

                using var stream = new FileStream(caminhoCompleto, FileMode.Create);
                await pdfFile.CopyToAsync(stream);

                projeto.ArquivoPdf = "/pdfs/" + nomeArquivo;
            }

            if (projeto.Links.Any())
                _context.ProjetoLinks.RemoveRange(projeto.Links);

            var novosLinks = model.Links?
                .Where(l => !string.IsNullOrWhiteSpace(l.Url))
                .ToList() ?? new();

            foreach (var link in novosLinks)
            {
                link.ProjetoId = projeto.Id;
                _context.ProjetoLinks.Add(link);
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Projeto atualizado com sucesso!";
            return RedirectToAction("Dashboard", "Aluno");
        }

        // =========================
        // TELA DE EXCLUIR
        // =========================
        public async Task<IActionResult> Excluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            return View(projeto);
        }

        // =========================
        // CONFIRMAR EXCLUSÃO
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarExcluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            // Remove PDF
            if (!string.IsNullOrEmpty(projeto.ArquivoPdf))
            {
                var caminhoPdf = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    projeto.ArquivoPdf.TrimStart('/').Replace("/", "\\"));

                if (System.IO.File.Exists(caminhoPdf))
                    System.IO.File.Delete(caminhoPdf);
            }

            // Remove curtidas
            if (projeto.Curtidas.Any())
                _context.Curtidas.RemoveRange(projeto.Curtidas);

            // Remove comentários
            if (projeto.Comentarios.Any())
                _context.Comentarios.RemoveRange(projeto.Comentarios);

            // Remove links
            if (projeto.Links.Any())
                _context.ProjetoLinks.RemoveRange(projeto.Links);

            // Remove projeto
            _context.Projetos.Remove(projeto);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Projeto excluído com sucesso!";

            return RedirectToAction("Dashboard", "Aluno");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Curtir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null)
                return Json(new { success = false });

            var curtida = await _context.Curtidas.FirstOrDefaultAsync(c =>
                c.ProjetoId == id &&
                ((tipoUsuario == "Aluno" && c.AlunoId == userId) ||
                 (tipoUsuario == "Empresa" && c.EmpresaId == userId)));

            if (curtida != null)
            {
                _context.Curtidas.Remove(curtida);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    curtiu = false,
                    total = await _context.Curtidas.CountAsync(c => c.ProjetoId == id)
                });
            }

            var novaCurtida = new Curtida
            {
                ProjetoId = id,
                DataCurtida = DateTime.Now,
                AlunoId = tipoUsuario == "Aluno" ? userId : null,
                EmpresaId = tipoUsuario == "Empresa" ? userId : null
            };

            _context.Curtidas.Add(novaCurtida);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                curtiu = true,
                total = await _context.Curtidas.CountAsync(c => c.ProjetoId == id)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int projetoId, string texto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || (tipoUsuario != "Aluno" && tipoUsuario != "Empresa"))
            {
                TempData["Erro"] = "Faça login como aluno ou empresa para comentar.";
                return RedirectToAction("Detalhes", new { id = projetoId });
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                TempData["Erro"] = "O comentário não pode estar vazio.";
                return RedirectToAction("Detalhes", new { id = projetoId });
            }

            var comentario = new Comentario
            {
                ProjetoId = projetoId,
                Texto = texto.Trim(),
                DataComentario = DateTime.Now
            };

            if (tipoUsuario == "Aluno")
                comentario.AlunoId = userId.Value;
            else
                comentario.EmpresaId = userId.Value;

            _context.Comentarios.Add(comentario);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Comentário adicionado com sucesso!";
            return RedirectToAction("Detalhes", new { id = projetoId });
        }

        private Dictionary<string, string> GetDynamicFieldDefinitions(string? area)
        {
            return area switch
            {
                "Tecnologia" => new()
                {
                    { "Repositório", "Link do GitHub" },
                    { "Deploy", "Link do Sistema Online" },
                    { "Stack", "Tecnologias Utilizadas" }
                },

                "Saúde" => new()
                {
                    { "TipoEstudo", "Tipo de Estudo" },
                    { "Instituicao", "Instituição" },
                    { "Documento", "Link do Artigo (Drive/PDF)" }
                },

                "Design" => new()
                {
                    { "Ferramenta", "Ferramenta Utilizada" },
                    { "Portfolio", "Portfólio" },
                    { "Preview", "Preview do Projeto" }
                },

                _ => new()
            };
        }
    }
}
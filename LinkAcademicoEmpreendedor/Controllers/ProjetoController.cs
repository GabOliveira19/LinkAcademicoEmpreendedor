using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using System.Text.Json;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class ProjetoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DynamicFormService _dynamicFormService;

        public ProjetoController(ApplicationDbContext context, DynamicFormService dynamicFormService)
        {
            _context = context;
            _dynamicFormService = dynamicFormService;
        }

        // Listar todos os projetos
        public async Task<IActionResult> Index(string busca, string tipo, string tecnologia, string area)
        {
            var query = _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .Include(p => p.Links) // incluir links para exibição
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
            {
                query = query.Where(p => p.Tipo == tipo);
            }

            if (!string.IsNullOrEmpty(tecnologia))
            {
                query = query.Where(p => p.Tecnologias!.Contains(tecnologia));
            }

            if (!string.IsNullOrEmpty(area))
            {
                query = query.Where(p => EF.Property<string>(p, "Area") != null && EF.Property<string>(p, "Area").Contains(area));
            }

            var projetos = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            ViewBag.Busca = busca;
            ViewBag.Tipo = tipo;
            ViewBag.Tecnologia = tecnologia;
            ViewBag.Area = area;

            return View(projetos);
        }

        // Detalhes do projeto
        public async Task<IActionResult> Detalhes(int id)
        {
            var projeto = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                    .ThenInclude(c => c.Aluno)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Aluno)
                .Include(p => p.Curtidas)
                    .ThenInclude(c => c.Empresa)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Empresa)
                .Include(p => p.Links) // incluir links
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            // verifica se aluno OU empresa ja curtiu
            bool jaCurtiu = false;
            if (userId != null && !string.IsNullOrEmpty(tipoUsuario))
            {
                if (tipoUsuario == "Aluno")
                    jaCurtiu = projeto.Curtidas.Any(c => c.AlunoId == userId);
                else if (tipoUsuario == "Empresa")
                    jaCurtiu = projeto.Curtidas.Any(c => c.EmpresaId == userId);
            }

            ViewBag.JaCurtiu = userId != null && (tipoUsuario == "Aluno" || tipoUsuario == "Empresa") && jaCurtiu;
            ViewBag.UserId = userId;
            ViewBag.TipoUsuario = tipoUsuario;

            // disponibiliza definições de campos dinâmicos e valores para a view de detalhes
            ViewBag.DynamicFieldsDefinition = GetDynamicFieldDefinitions(projeto.Area);
            ViewBag.DadosDinamicos = projeto.DadosDinamicos;

            return View(projeto);
        }

        // Criar projeto
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
                Area = aluno?.Area?.Nome // 🔥 AQUI resolve
            };

            ViewBag.AreaNome = aluno?.Area?.Nome;

            ViewBag.DynamicFieldsDefinition =
                GetDynamicFieldDefinitions(aluno?.Area?.Nome);

            return View(projeto); // 👈 MUITO IMPORTANTE
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Projeto projeto, IFormFile? pdfFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Aluno");
            ModelState.Remove("Curtidas");
            ModelState.Remove("Comentarios");
            ModelState.Remove("Links"); // permitir envio flexível de links
            ModelState.Remove("DadosDinamicosJson");
            ModelState.Remove("DadosDinamicos");

            // Captura campos dinâmicos enviados com prefixo "campo_"
            var dynamicFields = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                {
                    var fieldKey = key["campo_".Length..];
                    dynamicFields[fieldKey] = Request.Form[key].ToString() ?? string.Empty;
                }
            }

            // Se a área for Arquitetura, valida campos obrigatórios
            if (string.Equals(projeto.Area, "Arquitetura", StringComparison.OrdinalIgnoreCase))
            {
                var required = new[] { "Escalavel", "Configuravel", "FacilExpansaoFutura" };
                foreach (var r in required)
                {
                    if (!dynamicFields.TryGetValue(r, out var val) || string.IsNullOrWhiteSpace(val))
                        ModelState.AddModelError($"campo_{r}", $"{r} é obrigatório para Arquitetura");
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                ViewBag.TiposProjeto = new List<string> { "Projeto Geral" };
                ViewBag.DynamicFieldsDefinition = GetDynamicFieldDefinitions(projeto.Area);

                return View(projeto);
            }

            projeto.AlunoId = userId.Value;
            projeto.DataCriacao = DateTime.Now;
            projeto.Ativo = true;

            projeto.DadosDinamicos = dynamicFields;

            // Se houver links enviados via modelo, vamos persistir após criar o projeto para garantir FK
            var links = projeto.Links?
    .Where(l => !string.IsNullOrWhiteSpace(l.Url))
    .ToList() ?? new List<ProjetoLink>();
            projeto.Links?.Clear();
            var aluno = await _context.Alunos
    .Include(a => a.Area)
    .FirstOrDefaultAsync(a => a.Id == userId);

            projeto.Area = aluno?.Area?.Nome;
            if (pdfFile != null && pdfFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                projeto.ArquivoPdf = "/pdfs/" + fileName;
            }
            _context.Projetos.Add(projeto);
            await _context.SaveChangesAsync();

            if (links.Any())
            {
                foreach (var l in links)
                {
                    l.ProjetoId = projeto.Id;
                    _context.ProjetoLinks.Add(l);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] = "Projeto criado com sucesso!";
            return RedirectToAction("Dashboard", "Aluno");
        }

        // Editar projeto
        public async Task<IActionResult> Editar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Links)
                .Include(p => p.Aluno)
                    .ThenInclude(a => a!.Area)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            var areaId = projeto.Aluno?.AreaId ?? 1;
            var defs = await _dynamicFormService.GetFieldsByAreaIdAsync(areaId); // correção aplicada

            ViewBag.AreaName = projeto.Aluno?.Area?.Nome ?? "Interdisciplinar";
            ViewBag.DynamicFieldsDefinition = (object)defs;
            ViewBag.DadosDinamicos = projeto.DadosDinamicos ?? new Dictionary<string, string>();

            return View(projeto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Projeto model)
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
            var aluno = await _context.Alunos
    .Include(a => a.Area)
    .FirstOrDefaultAsync(a => a.Id == userId);

            projeto.Area = aluno?.Area?.Nome;
            projeto.Tecnologias = model.Tecnologias;
            projeto.LinkRepositorio = model.LinkRepositorio;
            projeto.LinkDemonstracao = model.LinkDemonstracao;

            // captura campos dinâmicos do formulário
            var novosCampos = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                {
                    var fieldKey = key["campo_".Length..];
                    novosCampos[fieldKey] = Request.Form[key].ToString() ?? string.Empty;
                }
            }

            // valida obrigatórios para Arquitetura
            if (string.Equals(projeto.Area, "Arquitetura", StringComparison.OrdinalIgnoreCase))
            {
                var required = new[] { "Escalavel", "Configuravel", "FacilExpansaoFutura" };
                foreach (var r in required)
                {
                    if (!novosCampos.TryGetValue(r, out var val) || string.IsNullOrWhiteSpace(val))
                    {
                        ModelState.AddModelError($"campo_{r}", $"{r} é obrigatório para Arquitetura");
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Areas = new List<string> { "Tecnologia", "Saúde", "Design", "Engenharia", "Arquitetura" };
                    ViewBag.DynamicFieldsDefinition = GetDynamicFieldDefinitions(projeto.Area);
                    ViewBag.DadosDinamicos = novosCampos;
                    return View(projeto);
                }
            }

            // Substituir links: remover existentes e adicionar os novos (simples, preserva compatibilidade)
            var novosLinks = model.Links?
    .Where(l => !string.IsNullOrWhiteSpace(l.Url))
    .ToList() ?? new List<ProjetoLink>();
            if (projeto.Links.Any())
            {
                _context.ProjetoLinks.RemoveRange(projeto.Links);
            }

            if (novosLinks.Any())
            {
                foreach (var l in novosLinks)
                {
                    l.ProjetoId = projeto.Id;
                    _context.ProjetoLinks.Add(l);
                }
            }

            projeto.DadosDinamicos = novosCampos;

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Projeto atualizado com sucesso!";
            return RedirectToAction("Dashboard", "Aluno");
        }

        // GET: Pagina de confirmacao de exclusao
        public async Task<IActionResult> Excluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id && p.AlunoId == userId);

            if (projeto == null)
            {
                TempData["Erro"] = "Projeto nao encontrado.";
                return RedirectToAction("Dashboard", "Aluno");
            }

            return View(projeto);
        }

        // POST: Confirmar exclusao do projeto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarExcluir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos.FindAsync(id);
            if (projeto == null || projeto.AlunoId != userId)
            {
                TempData["Erro"] = "Projeto nao encontrado.";
                return RedirectToAction("Dashboard", "Aluno");
            }

            try
            {
                // Excluir curtidas do projeto
                var curtidas = await _context.Curtidas
                    .Where(c => c.ProjetoId == id)
                    .ToListAsync();
                if (curtidas.Any())
                    _context.Curtidas.RemoveRange(curtidas);

                // Excluir comentarios do projeto
                var comentarios = await _context.Comentarios
                    .Where(c => c.ProjetoId == id)
                    .ToListAsync();
                if (comentarios.Any())
                    _context.Comentarios.RemoveRange(comentarios);

                // Excluir links do projeto
                var links = await _context.ProjetoLinks
                    .Where(l => l.ProjetoId == id)
                    .ToListAsync();
                if (links.Any())
                    _context.ProjetoLinks.RemoveRange(links);

                // Excluir o projeto
                _context.Projetos.Remove(projeto);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Projeto excluido com sucesso!";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir o projeto. Tente novamente.";
            }

            return RedirectToAction("Dashboard", "Aluno");
        }

        // Curtir projeto - agora aceita Aluno ou Empresa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Curtir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || (tipoUsuario != "Aluno" && tipoUsuario != "Empresa"))
            {
                return Json(new { success = false, message = "Faca login como aluno ou empresa para curtir" });
            }

            Curtida? curtidaExistente = null;
            if (tipoUsuario == "Aluno")
            {
                curtidaExistente = await _context.Curtidas
                    .FirstOrDefaultAsync(c => c.AlunoId == userId && c.ProjetoId == id);
            }
            else // Empresa
            {
                curtidaExistente = await _context.Curtidas
                    .FirstOrDefaultAsync(c => c.EmpresaId == userId && c.ProjetoId == id);
            }

            if (curtidaExistente != null)
            {
                _context.Curtidas.Remove(curtidaExistente);
                await _context.SaveChangesAsync();
                var totalCurtidas = await _context.Curtidas.CountAsync(c => c.ProjetoId == id);
                return Json(new { success = true, curtiu = false, total = totalCurtidas });
            }
            else
            {
                var curtida = new Curtida
                {
                    ProjetoId = id,
                    DataCurtida = DateTime.Now
                };

                if (tipoUsuario == "Aluno")
                    curtida.AlunoId = userId.Value;
                else
                    curtida.EmpresaId = userId.Value;

                _context.Curtidas.Add(curtida);
                await _context.SaveChangesAsync();
                var totalCurtidas = await _context.Curtidas.CountAsync(c => c.ProjetoId == id);
                return Json(new { success = true, curtiu = true, total = totalCurtidas });
            }
        }

        // Comentar projeto - agora aceita Aluno ou Empresa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int projetoId, string texto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || (tipoUsuario != "Aluno" && tipoUsuario != "Empresa"))
            {
                TempData["Erro"] = "Faca login como aluno ou empresa para comentar";
                return RedirectToAction("Detalhes", new { id = projetoId });
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                TempData["Erro"] = "O comentario nao pode estar vazio";
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

            TempData["Sucesso"] = "Comentario adicionado!";
            return RedirectToAction("Detalhes", new { id = projetoId });
        }

        private Dictionary<string, string> GetDynamicFieldDefinitions(string? area)
        {
            if (area == "Tecnologia")
            {
                return new Dictionary<string, string>
        {
            { "Repositorio", "Link do GitHub" },
            { "Deploy", "Link do sistema online" },
            { "Stack", "Tecnologias utilizadas" }
        };
            }

            if (area == "Saúde")
            {
                return new Dictionary<string, string>
        {
            { "TipoEstudo", "Tipo de estudo" },
            { "Instituicao", "Instituição" },
            { "Documento", "Link do artigo (Drive/PDF)" }
        };
            }

            if (area == "Design")
            {
                return new Dictionary<string, string>
        {
            { "Ferramenta", "Ferramenta usada" },
            { "Portfolio", "Link do portfólio" },
            { "Preview", "Link de visualização" }
        };
            }

            return new Dictionary<string, string>();
        }
    }
}
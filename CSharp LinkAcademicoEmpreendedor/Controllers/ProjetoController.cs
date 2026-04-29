using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;
using System.Text.Json;

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
                    .ThenInclude(a => a.Area)
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
            {
                query = query.Where(p => p.Tipo == tipo);
            }

            if (!string.IsNullOrEmpty(tecnologia))
            {
                query = query.Where(p => p.Tecnologias != null && p.Tecnologias.Contains(tecnologia));
            }

            if (!string.IsNullOrEmpty(area))
            {
                // Aceita tanto id numérico quanto nome parcial da área
                if (int.TryParse(area, out var areaId))
                {
                    query = query.Where(p => p.Aluno != null && p.Aluno.AreaId == areaId);
                }
                else
                {
                    query = query.Where(p => p.Aluno != null && p.Aluno.Area != null && p.Aluno.Area.Nome.Contains(area));
                }
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
                    .ThenInclude(a => a.Area)
                .Include(p => p.Curtidas)
                    .ThenInclude(c => c.Aluno)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Aluno)
                .Include(p => p.Curtidas)
                    .ThenInclude(c => c.Empresa)
                .Include(p => p.Comentarios)
                    .ThenInclude(c => c.Empresa)
                .Include(p => p.Links)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

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

            // obter definições dinâmicas pela área do autor do projeto (fallback = Interdisciplinar id=1)
            var areaId = projeto.Aluno?.AreaId ?? 1;
            var defs = await _dynamicFormService.GetFieldsByAreaIdAsync(areaId);
            ViewBag.DynamicFieldsDefinition = defs;
            ViewBag.DadosDinamicos = projeto.DadosDinamicos ?? new Dictionary<string, string>();

            return View(projeto);
        }

        // Criar projeto
        public async Task<IActionResult> Criar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var aluno = await _context.Alunos
                .Include(a => a.Area)
                .FirstOrDefaultAsync(a => a.Id == userId.Value);

            var areaId = aluno?.AreaId ?? 1;
            var defs = await _dynamicFormService.GetFieldsByAreaIdAsync(areaId);

            ViewBag.AreaName = aluno?.Area?.Nome ?? "Interdisciplinar";
            ViewBag.DynamicFieldsDefinition = defs;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Projeto projeto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Aluno");
            ModelState.Remove("Curtidas");
            ModelState.Remove("Comentarios");
            ModelState.Remove("Links");
            ModelState.Remove("DadosDinamicosJson");
            ModelState.Remove("DadosDinamicos");

            var aluno = await _context.Alunos
                .Include(a => a.Area)
                .FirstOrDefaultAsync(a => a.Id == userId.Value);

            var areaId = aluno?.AreaId ?? 1;
            var defs = await _dynamicFormService.GetFieldsByAreaIdAsync(areaId);

            // Captura campos dinâmicos enviados com prefixo "campo_"
            var dynamicFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                {
                    var fieldKey = key["campo_".Length..];
                    dynamicFields[fieldKey] = Request.Form[key].ToString() ?? string.Empty;
                }
            }

            // Validação baseada em metadata (sem if/else por área)
            foreach (var def in defs.Where(d => d.Obrigatorio))
            {
                if (!dynamicFields.TryGetValue(def.Nome, out var val) || string.IsNullOrWhiteSpace(val))
                {
                    ModelState.AddModelError($"campo_{def.Nome}", $"{def.Nome} é obrigatório.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AreaName = aluno?.Area?.Nome ?? "Interdisciplinar";
                ViewBag.DynamicFieldsDefinition = defs;
                ViewBag.DadosDinamicos = dynamicFields;
                return View(projeto);
            }

            projeto.AlunoId = userId.Value;
            projeto.DataCriacao = DateTime.Now;
            projeto.Ativo = true;
            // manter compatibilidade: gravar nome da área no campo string Area
            projeto.Area = aluno?.Area?.Nome ?? "Interdisciplinar";

            projeto.DadosDinamicos = dynamicFields;

            var links = projeto.Links?.ToList() ?? new List<ProjetoLink>();
            projeto.Links.Clear();

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
                    .ThenInclude(a => a.Area)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            var areaId = projeto.Aluno?.AreaId ?? 1;
            var defs = await _dynamicForm_service.GetFieldsByAreaIdAsync(areaId); // note: fixed below

            ViewBag.AreaName = projeto.Aluno?.Area?.Nome ?? "Interdisciplinar";
            ViewBag.DynamicFieldsDefinition = defs;
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
                .Include(p => p.Aluno)
                    .ThenInclude(a => a.Area)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            projeto.Titulo = model.Titulo;
            projeto.Descricao = model.Descricao;
            projeto.Tipo = model.Tipo;
            projeto.Tecnologias = model.Tecnologias;
            projeto.LinkRepositorio = model.LinkRepositorio;
            projeto.LinkDemonstracao = model.LinkDemonstracao;

            var areaId = projeto.Aluno?.AreaId ?? 1;
            var defs = await _dynamicFormService.GetFieldsByAreaIdAsync(areaId);

            // captura campos dinâmicos do formulário
            var novosCampos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("campo_"))
                {
                    var fieldKey = key["campo_".Length..];
                    novosCampos[fieldKey] = Request.Form[key].ToString() ?? string.Empty;
                }
            }

            // valida obrigatórios por metadata
            foreach (var def in defs.Where(d => d.Obrigatorio))
            {
                if (!novosCampos.TryGetValue(def.Nome, out var val) || string.IsNullOrWhiteSpace(val))
                {
                    ModelState.AddModelError($"campo_{def.Nome}", $"{def.Nome} é obrigatório.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AreaName = projeto.Aluno?.Area?.Nome ?? "Interdisciplinar";
                ViewBag.DynamicFieldsDefinition = defs;
                ViewBag.DadosDinamicos = novosCampos;
                return View(projeto);
            }

            var novosLinks = model.Links?.ToList() ?? new List<ProjetoLink>();
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

        // ... restante do controller permanece igual ...
    }
}
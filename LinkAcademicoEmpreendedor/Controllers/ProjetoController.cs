using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class ProjetoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjetoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listar todos os projetos
        public async Task<IActionResult> Index(string busca, string tipo, string tecnologia)
        {
            var query = _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
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

            var projetos = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            ViewBag.Busca = busca;
            ViewBag.Tipo = tipo;
            ViewBag.Tecnologia = tecnologia;

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
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            ViewBag.JaCurtiu = userId != null && tipoUsuario == "Aluno" && projeto.Curtidas.Any(c => c.AlunoId == userId);
            ViewBag.UserId = userId;
            ViewBag.TipoUsuario = tipoUsuario;

            return View(projeto);
        }

        // Criar projeto
        public IActionResult Criar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

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

            if (!ModelState.IsValid)
                return View(projeto);

            projeto.AlunoId = userId.Value;
            projeto.DataCriacao = DateTime.Now;
            projeto.Ativo = true;

            _context.Projetos.Add(projeto);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Projeto criado com sucesso!";
            return RedirectToAction("Dashboard", "Aluno");
        }

        // Editar projeto
        public async Task<IActionResult> Editar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos.FindAsync(id);
            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            return View(projeto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Projeto model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var projeto = await _context.Projetos.FindAsync(id);
            if (projeto == null || projeto.AlunoId != userId)
                return NotFound();

            projeto.Titulo = model.Titulo;
            projeto.Descricao = model.Descricao;
            projeto.Tipo = model.Tipo;
            projeto.Tecnologias = model.Tecnologias;
            projeto.LinkRepositorio = model.LinkRepositorio;
            projeto.LinkDemonstracao = model.LinkDemonstracao;

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

        // Curtir projeto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Curtir(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
            {
                return Json(new { success = false, message = "Faca login como aluno para curtir" });
            }

            var curtidaExistente = await _context.Curtidas
                .FirstOrDefaultAsync(c => c.AlunoId == userId && c.ProjetoId == id);

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
                    AlunoId = userId.Value,
                    ProjetoId = id,
                    DataCurtida = DateTime.Now
                };
                _context.Curtidas.Add(curtida);
                await _context.SaveChangesAsync();
                var totalCurtidas = await _context.Curtidas.CountAsync(c => c.ProjetoId == id);
                return Json(new { success = true, curtiu = true, total = totalCurtidas });
            }
        }

        // Comentar projeto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int projetoId, string texto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
            {
                TempData["Erro"] = "Faca login como aluno para comentar";
                return RedirectToAction("Detalhes", new { id = projetoId });
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                TempData["Erro"] = "O comentario nao pode estar vazio";
                return RedirectToAction("Detalhes", new { id = projetoId });
            }

            var comentario = new Comentario
            {
                AlunoId = userId.Value,
                ProjetoId = projetoId,
                Texto = texto.Trim(),
                DataComentario = DateTime.Now
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Comentario adicionado!";
            return RedirectToAction("Detalhes", new { id = projetoId });
        }
    }
}
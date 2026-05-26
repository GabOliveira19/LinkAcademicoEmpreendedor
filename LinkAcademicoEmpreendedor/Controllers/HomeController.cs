using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using System.Diagnostics;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var projetosDestaque = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Include(p => p.Comentarios)
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.Curtidas.Count)
                .Take(6)
                .ToListAsync();

            var oportunidadesRecentes = await _context.Oportunidades
                .Include(o => o.Empresa)
                .Where(o => o.Ativa)
                .OrderByDescending(o => o.DataPublicacao)
                .Take(4)
                .ToListAsync();

            ViewBag.ProjetosDestaque = projetosDestaque;
            ViewBag.OportunidadesRecentes = oportunidadesRecentes;
            ViewBag.TotalAlunos = await _context.Alunos.CountAsync();
            ViewBag.TotalEmpresas = await _context.Empresas.CountAsync();
            ViewBag.TotalProjetos = await _context.Projetos.CountAsync();

            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Desenvolvedores()
        {
            var nomesDesenvolvedores = new List<string>
    {
        "Gabriel Cézar de Oliveira",
        "Guilherme Oliveira da Silva",
        "Matheus Moura Leite",
        "Gabriel Siqueira Assis",
        "João Gabriel Martins Pereira",
        "João Marcos Corrêa Silveira",
        "Fábio Lopes"
    };

            var desenvolvedores = await _context.Alunos
                .Where(a => nomesDesenvolvedores.Contains(a.Nome))
                .ToListAsync();

            ViewBag.NomesDesenvolvedores = nomesDesenvolvedores;

            return View(desenvolvedores);
        }

    }

}
using Microsoft.AspNetCore.Mvc;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class RankingController : Controller
    {
        private readonly RankingService _rankingService;

        public RankingController(ApplicationDbContext context)
        {
            _rankingService = new RankingService(context);
        }

        // GET: /Ranking/Alunos
        public async Task<IActionResult> Alunos()
        {
            var ranking = await _rankingService.ObterRankingAlunosAsync(50);
            return View(ranking);
        }

        // GET: /Ranking/Empresas
        public async Task<IActionResult> Empresas()
        {
            var ranking = await _rankingService.ObterRankingEmpresasAsync(50);
            return View(ranking);
        }
    }
}
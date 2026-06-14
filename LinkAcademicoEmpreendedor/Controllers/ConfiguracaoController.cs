using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class ConfiguracaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Visual()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (usuarioId == null || string.IsNullOrWhiteSpace(tipoUsuario))
                return Unauthorized();

            var configuracao = await ObterOuCriarConfiguracaoAsync(usuarioId.Value, tipoUsuario);

            return Json(new ConfiguracaoVisualUsuarioViewModel
            {
                Tema = configuracao.Tema,
                TamanhoFonte = configuracao.TamanhoFonte,
                Densidade = configuracao.Densidade,
                ReduzirAnimacoes = configuracao.ReduzirAnimacoes,
                ModoDaltonico = configuracao.ModoDaltonico,
                ReduzirCores = configuracao.ReduzirCores
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarVisual([FromBody] ConfiguracaoVisualUsuarioViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (usuarioId == null || string.IsNullOrWhiteSpace(tipoUsuario))
                return Unauthorized();

            var configuracao = await ObterOuCriarConfiguracaoAsync(usuarioId.Value, tipoUsuario);

            configuracao.Tema = Normalizar(model.Tema, new[] { "Claro", "Escuro" }, "Claro");
            configuracao.TamanhoFonte = Normalizar(model.TamanhoFonte, new[] { "Normal", "Grande" }, "Normal");
            configuracao.Densidade = Normalizar(model.Densidade, new[] { "Confortavel", "Compacta" }, "Confortavel");
            configuracao.ReduzirAnimacoes = model.ReduzirAnimacoes;
            configuracao.ModoDaltonico = model.ModoDaltonico;
            configuracao.ReduzirCores = model.ReduzirCores;
            configuracao.AtualizadoEm = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { sucesso = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcluirTour()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (usuarioId == null || string.IsNullOrWhiteSpace(tipoUsuario))
                return Unauthorized();

            if (tipoUsuario == "Aluno")
            {
                var aluno = await _context.Alunos.FindAsync(usuarioId.Value);
                if (aluno == null)
                    return NotFound();

                aluno.TourInicialConcluido = true;
            }
            else if (tipoUsuario == "Empresa")
            {
                var empresa = await _context.Empresas.FindAsync(usuarioId.Value);
                if (empresa == null)
                    return NotFound();

                empresa.TourInicialConcluido = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { sucesso = true });
        }

        private async Task<ConfiguracaoVisualUsuario> ObterOuCriarConfiguracaoAsync(int usuarioId, string tipoUsuario)
        {
            var configuracao = await _context.ConfiguracoesVisuaisUsuario
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.TipoUsuario == tipoUsuario);

            if (configuracao != null)
                return configuracao;

            configuracao = new ConfiguracaoVisualUsuario
            {
                UsuarioId = usuarioId,
                TipoUsuario = tipoUsuario,
                Tema = "Claro",
                TamanhoFonte = "Normal",
                Densidade = "Confortavel",
                ReduzirAnimacoes = false,
                ModoDaltonico = false,
                ReduzirCores = false,
                AtualizadoEm = DateTime.Now
            };

            _context.ConfiguracoesVisuaisUsuario.Add(configuracao);
            await _context.SaveChangesAsync();

            return configuracao;
        }

        private static string Normalizar(string? valor, string[] permitidos, string padrao)
        {
            return permitidos.Contains(valor) ? valor! : padrao;
        }
    }
}

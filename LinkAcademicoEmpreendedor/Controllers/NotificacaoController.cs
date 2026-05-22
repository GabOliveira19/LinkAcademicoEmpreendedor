using Microsoft.AspNetCore.Mvc;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class NotificacaoController : Controller
    {
        private readonly NotificacaoService _NotificacaoService;

        public NotificacaoController(ApplicationDbContext context)
        {
            _NotificacaoService = new NotificacaoService(context);
        }

        // GET: /Notificacao
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return RedirectToAction("Login", "Account");

            var Notificacoes = await _NotificacaoService.ObterNotificacoesAsync(userId.Value, tipoUsuario, 50);
            return View(Notificacoes);
        }

        // GET: /Notificacao/NaoLidas (API JSON)
        [HttpGet]
        public async Task<IActionResult> NaoLidas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return Json(new { count = 0, Notificacoes = new List<object>() });

            var Notificacoes = await _NotificacaoService.ObterNaoLidasAsync(userId.Value, tipoUsuario);
            var count = Notificacoes.Count;

            return Json(new
            {
                count,
                Notificacoes = Notificacoes.Take(5).Select(n => new
                {
                    n.Id,
                    n.Titulo,
                    n.Mensagem,
                    n.Link,
                    Data = n.DataCriacao.ToString("dd/MM/yyyy HH:mm")
                })
            });
        }

        // POST: /Notificacao/MarcarLida/5
        [HttpPost]
        public async Task<IActionResult> MarcarLida(int id)
        {
            await _NotificacaoService.MarcarComoLidaAsync(id);
            return Ok();
        }

        // POST: /Notificacao/MarcarTodasLidas
        [HttpPost]
        public async Task<IActionResult> MarcarTodasLidas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return Unauthorized();

            await _NotificacaoService.MarcarTodasComoLidasAsync(userId.Value, tipoUsuario);
            return Ok();
        }
    }
}
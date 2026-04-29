using Microsoft.AspNetCore.Mvc;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class NotificacaoController : Controller
    {
        private readonly NotificacaoService _notificacaoService;

        public NotificacaoController(ApplicationDbContext context)
        {
            _notificacaoService = new NotificacaoService(context);
        }

        // GET: /Notificacao
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return RedirectToAction("Login", "Account");

            var notificacoes = await _notificacaoService.ObterNotificacoesAsync(userId.Value, tipoUsuario, 50);
            return View(notificacoes);
        }

        // GET: /Notificacao/NaoLidas (API JSON)
        [HttpGet]
        public async Task<IActionResult> NaoLidas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return Json(new { count = 0, notificacoes = new List<object>() });

            var notificacoes = await _notificacaoService.ObterNaoLidasAsync(userId.Value, tipoUsuario);
            var count = notificacoes.Count;

            return Json(new
            {
                count,
                notificacoes = notificacoes.Take(5).Select(n => new
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
            await _notificacaoService.MarcarComoLidaAsync(id);
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

            await _notificacaoService.MarcarTodasComoLidasAsync(userId.Value, tipoUsuario);
            return Ok();
        }
    }
}
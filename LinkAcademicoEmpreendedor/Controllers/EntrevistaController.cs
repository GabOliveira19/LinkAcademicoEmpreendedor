using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class EntrevistaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntrevistaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Agendar(int candidaturaId)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var candidatura = await ObterCandidaturaDaEmpresaAsync(candidaturaId, empresaId.Value);
            if (candidatura == null)
                return NotFound();

            if (candidatura.Status != "Aprovada")
            {
                TempData["Erro"] = "A entrevista só pode ser agendada após a candidatura ser aprovada.";
                return RedirectToAction("Detalhes", "Candidatura", new { id = candidaturaId });
            }

            var entrevistaExistente = await _context.Entrevistas
                .FirstOrDefaultAsync(e => e.CandidaturaId == candidaturaId);

            if (entrevistaExistente != null)
            {
                ViewBag.Candidatura = candidatura;
                return View(entrevistaExistente);
            }

            var entrevista = new Entrevista
            {
                CandidaturaId = candidatura.Id,
                AlunoId = candidatura.AlunoId,
                EmpresaId = empresaId.Value,
                OportunidadeId = candidatura.OportunidadeId,
                Titulo = $"Entrevista - {candidatura.Oportunidade?.Titulo}",
                DataHora = DateTime.Now.AddDays(1).Date.AddHours(9),
                DuracaoMinutos = 45
            };

            ViewBag.Candidatura = candidatura;
            return View(entrevista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(int candidaturaId, DateTime dataHora, int duracaoMinutos, string? observacoes)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var candidatura = await ObterCandidaturaDaEmpresaAsync(candidaturaId, empresaId.Value);
            if (candidatura == null)
                return NotFound();

            if (candidatura.Status != "Aprovada")
            {
                TempData["Erro"] = "A entrevista só pode ser agendada após a candidatura ser aprovada.";
                return RedirectToAction("Detalhes", "Candidatura", new { id = candidaturaId });
            }

            if (dataHora <= DateTime.Now)
            {
                TempData["Erro"] = "Escolha uma data e horário futuros para a entrevista.";
                ViewBag.Candidatura = candidatura;
                return View(new Entrevista
                {
                    CandidaturaId = candidaturaId,
                    AlunoId = candidatura.AlunoId,
                    EmpresaId = empresaId.Value,
                    OportunidadeId = candidatura.OportunidadeId,
                    Titulo = $"Entrevista - {candidatura.Oportunidade?.Titulo}",
                    DataHora = dataHora,
                    DuracaoMinutos = duracaoMinutos,
                    Observacoes = observacoes
                });
            }

            if (duracaoMinutos < 15 || duracaoMinutos > 180)
                duracaoMinutos = 45;

            var entrevista = await _context.Entrevistas
                .FirstOrDefaultAsync(e => e.CandidaturaId == candidaturaId);

            if (entrevista == null)
            {
                entrevista = new Entrevista
                {
                    CandidaturaId = candidatura.Id,
                    AlunoId = candidatura.AlunoId,
                    EmpresaId = empresaId.Value,
                    OportunidadeId = candidatura.OportunidadeId,
                    Titulo = $"Entrevista - {candidatura.Oportunidade?.Titulo}",
                    CodigoSala = GerarCodigoSala(candidatura),
                    Status = "Agendada"
                };

                _context.Entrevistas.Add(entrevista);
            }

            entrevista.DataHora = dataHora;
            entrevista.DuracaoMinutos = duracaoMinutos;
            entrevista.Observacoes = observacoes?.Trim();
            entrevista.Status = "Agendada";

            await _context.SaveChangesAsync();

            _context.Notificacoes.Add(new Notificacao
            {
                Titulo = "Entrevista agendada",
                Mensagem = $"A empresa agendou uma entrevista para '{candidatura.Oportunidade?.Titulo}' em {entrevista.DataHora:dd/MM/yyyy HH:mm}.",
                Link = $"/Entrevista/Detalhes/{entrevista.Id}",
                TipoDestinatario = "Aluno",
                DestinatarioId = candidatura.AlunoId,
                DataCriacao = DateTime.Now,
                Lida = false
            });

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Entrevista agendada com sucesso! O aluno foi notificado.";
            return RedirectToAction("Detalhes", new { id = entrevista.Id });
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var entrevista = await ObterEntrevistaAutorizadaAsync(id);
            if (entrevista == null)
                return NotFound();

            return View(entrevista);
        }

        public async Task<IActionResult> Sala(int id)
        {
            var entrevista = await ObterEntrevistaAutorizadaAsync(id);
            if (entrevista == null)
                return NotFound();

            if (entrevista.Status == "Cancelada")
            {
                TempData["Erro"] = "Esta entrevista foi cancelada.";
                return RedirectToAction("Detalhes", new { id });
            }

            ViewBag.NomeParticipante = ObterNomeParticipante(entrevista);
            return View(entrevista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var entrevista = await _context.Entrevistas
                .Include(e => e.Oportunidade)
                .FirstOrDefaultAsync(e => e.Id == id && e.EmpresaId == empresaId.Value);

            if (entrevista == null)
                return NotFound();

            entrevista.Status = "Cancelada";

            _context.Notificacoes.Add(new Notificacao
            {
                Titulo = "Entrevista cancelada",
                Mensagem = $"A entrevista da vaga '{entrevista.Oportunidade?.Titulo}' foi cancelada pela empresa.",
                Link = $"/Entrevista/Detalhes/{entrevista.Id}",
                TipoDestinatario = "Aluno",
                DestinatarioId = entrevista.AlunoId,
                DataCriacao = DateTime.Now,
                Lida = false
            });

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Entrevista cancelada com sucesso.";
            return RedirectToAction("Detalhes", new { id });
        }

        private async Task<Candidatura?> ObterCandidaturaDaEmpresaAsync(int candidaturaId, int empresaId)
        {
            return await _context.Candidaturas
                .Include(c => c.Aluno)
                .Include(c => c.Oportunidade)
                    .ThenInclude(o => o!.Empresa)
                .FirstOrDefaultAsync(c => c.Id == candidaturaId && c.Oportunidade!.EmpresaId == empresaId);
        }

        private async Task<Entrevista?> ObterEntrevistaAutorizadaAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || string.IsNullOrEmpty(tipoUsuario))
                return null;

            var entrevista = await _context.Entrevistas
                .Include(e => e.Aluno)
                .Include(e => e.Empresa)
                .Include(e => e.Oportunidade)
                .Include(e => e.Candidatura)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrevista == null)
                return null;

            var autorizado = (tipoUsuario == "Aluno" && entrevista.AlunoId == userId.Value) ||
                             (tipoUsuario == "Empresa" && entrevista.EmpresaId == userId.Value);

            return autorizado ? entrevista : null;
        }

        private string ObterNomeParticipante(Entrevista entrevista)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (tipoUsuario == "Empresa" && entrevista.EmpresaId == userId)
                return entrevista.Empresa?.RazaoSocial ?? "Empresa";

            return entrevista.Aluno?.Nome ?? "Aluno";
        }

        private static string GerarCodigoSala(Candidatura candidatura)
        {
            var sufixo = Guid.NewGuid().ToString("N")[..12];
            return $"skillbridge-entrevista-{candidatura.OportunidadeId}-{candidatura.AlunoId}-{sufixo}";
        }
    }
}

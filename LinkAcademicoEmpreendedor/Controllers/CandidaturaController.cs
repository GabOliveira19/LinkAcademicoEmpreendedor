using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class CandidaturaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CandidaturaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Candidatar-se a uma vaga (Aluno)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Candidatar(int oportunidadeId, string? mensagem)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
            {
                TempData["Erro"] = "Voce precisa estar logado como aluno para se candidatar.";
                return RedirectToAction("Detalhes", "Oportunidade", new { id = oportunidadeId });
            }

            // Verificar se ja se candidatou
            var candidaturaExistente = await _context.Candidaturas
                .FirstOrDefaultAsync(c => c.AlunoId == userId && c.OportunidadeId == oportunidadeId);

            if (candidaturaExistente != null)
            {
                TempData["Erro"] = "Voce ja se candidatou a esta vaga.";
                return RedirectToAction("Detalhes", "Oportunidade", new { id = oportunidadeId });
            }

            // Verificar se a oportunidade existe e esta ativa
            var oportunidade = await _context.Oportunidades
                .Include(o => o.Empresa)
                .FirstOrDefaultAsync(o => o.Id == oportunidadeId && o.Ativa);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Oportunidade nao encontrada ou nao esta mais disponivel.";
                return RedirectToAction("Index", "Oportunidade");
            }

            var aluno = await _context.Alunos.FindAsync(userId);

            // Criar candidatura
            var candidatura = new Candidatura
            {
                AlunoId = userId.Value,
                OportunidadeId = oportunidadeId,
                MensagemApresentacao = mensagem?.Trim(),
                DataCandidatura = DateTime.Now,
                Status = "Pendente"
            };

            _context.Candidaturas.Add(candidatura);

            // Criar Notificacao para a empresa
            var Notificacao = new Notificacao
            {
                Titulo = "Nova Candidatura Recebida",
                Mensagem = $"{aluno?.Nome} se candidatou para a vaga: {oportunidade.Titulo}",
                Link = $"/Candidatura/Detalhes/{candidatura.Id}",
                TipoDestinatario = "Empresa",
                DestinatarioId = oportunidade.EmpresaId,
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notificacoes.Add(Notificacao);
            await _context.SaveChangesAsync();

            // Atualizar o link da Notificacao com o ID correto
            Notificacao.Link = $"/Candidatura/Detalhes/{candidatura.Id}";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Candidatura enviada com sucesso! A empresa sera notificada.";
            return RedirectToAction("Detalhes", "Oportunidade", new { id = oportunidadeId });
        }

        // Minhas Candidaturas (Aluno)
        public async Task<IActionResult> MinhasCandidaturas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var candidaturas = await _context.Candidaturas
                .Include(c => c.Oportunidade)
                    .ThenInclude(o => o!.Empresa)
                .Where(c => c.AlunoId == userId)
                .OrderByDescending(c => c.DataCandidatura)
                .ToListAsync();

            return View(candidaturas);
        }

        // GET: /Candidatura/Candidatos?oportunidadeId=1
        public async Task<IActionResult> Candidatos(int oportunidadeId)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscar a Oportunidade com as Candidaturas incluidas
            var oportunidade = await _context.Oportunidades
                .Include(o => o.Candidaturas)
                    .ThenInclude(c => c.Aluno)
                        .ThenInclude(a => a.Projetos)
                .FirstOrDefaultAsync(o => o.Id == oportunidadeId && o.EmpresaId == empresaId);

            if (oportunidade == null)
            {
                TempData["Erro"] = "Oportunidade nao encontrada.";
                return RedirectToAction("MinhasOportunidades", "Oportunidade");
            }

            // Retornar a Oportunidade (nao a lista de candidaturas)
            return View(oportunidade);
        }
        // Detalhes da candidatura
        public async Task<IActionResult> Detalhes(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var candidatura = await _context.Candidaturas
                .Include(c => c.Aluno)
                    .ThenInclude(a => a!.Projetos)
                        .ThenInclude(p => p.Curtidas)
                .Include(c => c.Oportunidade)
                    .ThenInclude(o => o!.Empresa)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null)
                return NotFound();

            // Verificar permissao
            if (tipoUsuario == "Aluno" && candidatura.AlunoId != userId)
                return Forbid();

            if (tipoUsuario == "Empresa" && candidatura.Oportunidade?.EmpresaId != userId)
                return Forbid();

            // Se empresa esta vendo, marcar como visualizada
            if (tipoUsuario == "Empresa" && candidatura.Status == "Pendente")
            {
                candidatura.Status = "Visualizada";
                candidatura.DataVisualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return View(candidatura);
        }

        // Responder candidatura (Empresa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(int candidaturaId, string status, string? mensagemResposta)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var candidatura = await _context.Candidaturas
                .Include(c => c.Oportunidade)
                .Include(c => c.Aluno)
                .FirstOrDefaultAsync(c => c.Id == candidaturaId);

            if (candidatura == null || candidatura.Oportunidade?.EmpresaId != userId)
                return NotFound();

            candidatura.Status = status;
            candidatura.MensagemResposta = mensagemResposta?.Trim();
            candidatura.DataResposta = DateTime.Now;

            // Criar Notificacao para o aluno
            var statusTexto = status == "Aprovada" ? "aprovada" : "atualizada";
            var Notificacao = new Notificacao
            {
                Titulo = $"Candidatura {statusTexto}",
                Mensagem = $"Sua candidatura para '{candidatura.Oportunidade.Titulo}' foi {statusTexto}.",
                Link = $"/Candidatura/Detalhes/{candidatura.Id}",
                TipoDestinatario = "Aluno",
                DestinatarioId = candidatura.AlunoId,
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notificacoes.Add(Notificacao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Resposta enviada com sucesso!";
            return RedirectToAction("Candidatos", new { oportunidadeId = candidatura.OportunidadeId });
        }

        // Cancelar candidatura (Aluno)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Aluno")
                return RedirectToAction("Login", "Account");

            var candidatura = await _context.Candidaturas
                .FirstOrDefaultAsync(c => c.Id == id && c.AlunoId == userId);

            if (candidatura == null)
                return NotFound();

            _context.Candidaturas.Remove(candidatura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Candidatura cancelada com sucesso!";
            return RedirectToAction("MinhasCandidaturas");
        }
        // ACEITAR CANDIDATURA
        public async Task<IActionResult> Aceitar(int id)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var candidatura = await _context.Candidaturas
                .Include(c => c.Oportunidade)
                .Include(c => c.Aluno)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null || candidatura.Oportunidade.EmpresaId != empresaId)
                return NotFound();

            candidatura.Status = "Aprovada";
            candidatura.DataResposta = DateTime.Now;

            _context.Notificacoes.Add(new Notificacao
            {
                Titulo = "Candidatura Aceita",
                Mensagem = $"Sua candidatura para '{candidatura.Oportunidade.Titulo}' foi aceita!",
                Link = $"/Candidatura/Detalhes/{candidatura.Id}",
                TipoDestinatario = "Aluno",
                DestinatarioId = candidatura.AlunoId,
                DataCriacao = DateTime.Now,
                Lida = false
            });

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Candidato aceito com sucesso!";
            return RedirectToAction("Candidatos", new { oportunidadeId = candidatura.OportunidadeId });
        }


        // REJEITAR CANDIDATURA
        public async Task<IActionResult> Rejeitar(int id)
        {
            var empresaId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (empresaId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var candidatura = await _context.Candidaturas
                .Include(c => c.Oportunidade)
                .Include(c => c.Aluno)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidatura == null || candidatura.Oportunidade.EmpresaId != empresaId)
                return NotFound();

            candidatura.Status = "Rejeitada";
            candidatura.DataResposta = DateTime.Now;

            _context.Notificacoes.Add(new Notificacao
            {
                Titulo = "Candidatura Rejeitada",
                Mensagem = $"Sua candidatura para '{candidatura.Oportunidade.Titulo}' foi rejeitada.",
                Link = $"/Candidatura/Detalhes/{candidatura.Id}",
                TipoDestinatario = "Aluno",
                DestinatarioId = candidatura.AlunoId,
                DataCriacao = DateTime.Now,
                Lida = false
            });

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Candidato rejeitado com sucesso!";
            return RedirectToAction("Candidatos", new { oportunidadeId = candidatura.OportunidadeId });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.ViewModels;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmpresaController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas
                .Include(e => e.Oportunidades)
                    .ThenInclude(o => o.Candidaturas)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (empresa == null)
                return RedirectToAction("Login", "Account");

            var assinaturaPremium = await ObterAssinaturaPremiumAtivaAsync(userId.Value);
            var nivelPremium = ObterNivelPremium(assinaturaPremium);

            var limiteTalentosRecentes = nivelPremium switch
            {
                3 => 20,
                2 => 12,
                1 => 8,
                _ => 5
            };

            var alunosRecentes = await _context.Alunos
                .Include(a => a.Projetos)
                .OrderByDescending(a => a.DataCadastro)
                .Take(limiteTalentosRecentes)
                .ToListAsync();

            var limiteProjetosDestaque = nivelPremium == 3 ? 12 : 6;

            var projetosDestaque = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.Curtidas.Count)
                .Take(limiteProjetosDestaque)
                .ToListAsync();

            var viewModel = new DashboardEmpresaViewModel
            {
                Empresa = empresa,
                MinhasOportunidades = empresa.Oportunidades?.ToList() ?? new List<Oportunidade>(),
                AlunosRecentes = alunosRecentes,
                ProjetosDestaque = projetosDestaque,
                TotalCandidaturas = empresa.Oportunidades?.Sum(o => o.Candidaturas?.Count ?? 0) ?? 0,
                AssinaturaPremium = assinaturaPremium,
                PodeRenovarPremium = assinaturaPremium != null && assinaturaPremium.Fim <= DateTime.Now.AddDays(7)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Perfil(int id)
        {
            var empresa = await _context.Empresas
                .Include(e => e.RedesSociais)
                .Include(e => e.Oportunidades.Where(o => o.Ativa))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empresa == null)
                return NotFound();

            ViewBag.AssinaturaPremium = await ObterAssinaturaPremiumAtivaAsync(id);

            return View(empresa);
        }

        public async Task<IActionResult> EditarPerfil()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas
                .Include(e => e.RedesSociais)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (empresa == null)
                return NotFound();

            var viewModel = new EditarPerfilEmpresaViewModel
            {
                Id = empresa.Id,
                Cnpj = empresa.Cnpj,
                RazaoSocial = empresa.RazaoSocial,
                NomeFantasia = empresa.NomeFantasia,
                SituacaoCadastral = empresa.SituacaoCadastral,
                DataAbertura = empresa.DataAbertura,
                NaturezaJuridica = empresa.NaturezaJuridica,
                Endereco = empresa.Endereco,
                Numero = empresa.Numero,
                Bairro = empresa.Bairro,
                Cidade = empresa.Cidade,
                Estado = empresa.Estado,
                Cep = empresa.Cep,
                Email = empresa.Email,
                Telefone = empresa.Telefone,
                Descricao = empresa.Descricao,
                AreaAtuacao = empresa.AreaAtuacao,
                LogoEmpresa = empresa.LogoEmpresa,
                NomeResponsavel = empresa.NomeResponsavel,
                RedesSociais = empresa.RedesSociais
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(
            EditarPerfilEmpresaViewModel model,
            IFormFile? logoEmpresaFile,
            List<string>? plataformas,
            List<string>? urls)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != model.Id)
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas
                .Include(e => e.RedesSociais)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (empresa == null)
                return NotFound();

            ModelState.Remove("logoEmpresaFile");
            ModelState.Remove("plataformas");
            ModelState.Remove("urls");
            ModelState.Remove("RedesSociais");

            if (!ModelState.IsValid)
            {
                model.RedesSociais = empresa.RedesSociais;
                return View(model);
            }

            if (logoEmpresaFile != null && logoEmpresaFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "empresas");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(empresa.LogoEmpresa))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, empresa.LogoEmpresa.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(logoEmpresaFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoEmpresaFile.CopyToAsync(stream);
                }

                empresa.LogoEmpresa = $"/uploads/empresas/{fileName}";
                HttpContext.Session.SetString("UserFoto", empresa.LogoEmpresa);
            }

            empresa.RazaoSocial = model.RazaoSocial;
            empresa.NomeFantasia = model.NomeFantasia;
            empresa.NaturezaJuridica = model.NaturezaJuridica;
            empresa.Endereco = model.Endereco;
            empresa.Numero = model.Numero;
            empresa.Bairro = model.Bairro;
            empresa.Cidade = model.Cidade;
            empresa.Estado = model.Estado;
            empresa.Cep = model.Cep;
            empresa.Telefone = model.Telefone;
            empresa.Descricao = model.Descricao;
            empresa.AreaAtuacao = model.AreaAtuacao;
            empresa.NomeResponsavel = model.NomeResponsavel;

            _context.RedesSociais.RemoveRange(empresa.RedesSociais);

            if (plataformas != null && urls != null)
            {
                for (int i = 0; i < plataformas.Count && i < urls.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(plataformas[i]) || string.IsNullOrWhiteSpace(urls[i]))
                        continue;

                    _context.RedesSociais.Add(new RedeSocial
                    {
                        Plataforma = plataformas[i],
                        Url = NormalizarUrlRedeSocial(plataformas[i], urls[i]),
                        EmpresaId = empresa.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", empresa.RazaoSocial);
            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverLogo()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas.FindAsync(userId);
            if (empresa == null)
                return NotFound();

            if (!string.IsNullOrEmpty(empresa.LogoEmpresa))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, empresa.LogoEmpresa.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                empresa.LogoEmpresa = null;
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserFoto", "");
            }

            TempData["Sucesso"] = "Logo removido com sucesso!";
            return RedirectToAction("EditarPerfil");
        }

        public async Task<IActionResult> BuscarTalentos(string busca, string habilidade, string curso)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null || tipoUsuario != "Empresa")
                return RedirectToAction("Login", "Account");

            var assinaturaPremium = await ObterAssinaturaPremiumAtivaAsync(userId.Value);
            var nivelPremium = ObterNivelPremium(assinaturaPremium);
            var possuiFiltrosAvancados = nivelPremium >= 2;

            var query = _context.Alunos
                .Include(a => a.Projetos)
                    .ThenInclude(p => p.Curtidas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                query = query.Where(a =>
                    a.Nome.Contains(busca) ||
                    (a.Instituicao != null && a.Instituicao.Contains(busca)));
            }

            if (possuiFiltrosAvancados && !string.IsNullOrEmpty(habilidade))
            {
                query = query.Where(a =>
                    a.Habilidades != null &&
                    a.Habilidades.Contains(habilidade));
            }

            if (possuiFiltrosAvancados && !string.IsNullOrEmpty(curso))
            {
                query = query.Where(a =>
                    a.Curso != null &&
                    a.Curso.Contains(curso));
            }

            var limiteResultados = nivelPremium switch
            {
                3 => 100,
                2 => 50,
                1 => 20,
                _ => 10
            };

            var alunos = query
                .AsEnumerable()
                .OrderByDescending(a => a.Projetos.Sum(p => p.Curtidas.Count))
                .Take(limiteResultados)
                .ToList();

            ViewBag.Busca = busca;
            ViewBag.Habilidade = habilidade;
            ViewBag.Curso = curso;
            ViewBag.AssinaturaPremium = assinaturaPremium;
            ViewBag.PossuiFiltrosAvancados = possuiFiltrosAvancados;
            ViewBag.LimiteResultados = limiteResultados;

            return View(alunos);
        }

        private string NormalizarUrlRedeSocial(string plataforma, string url)
        {
            url = url.Trim();

            if (plataforma.Equals("E-mail", StringComparison.OrdinalIgnoreCase))
                return url.StartsWith("mailto:") ? url : $"mailto:{url}";

            if (plataforma.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                    return url;

                var numeros = new string(url.Where(char.IsDigit).ToArray());

                if (!numeros.StartsWith("55"))
                    numeros = "55" + numeros;

                return $"https://wa.me/{numeros}";
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                return $"https://{url}";

            return url;
        }

        private async Task<AssinaturaPremium?> ObterAssinaturaPremiumAtivaAsync(int empresaId)
        {
            return await _context.AssinaturasPremium
                .Include(a => a.PlanoPremium)
                .Where(a => a.EmpresaId == empresaId && a.Status == "Ativa" && a.Fim >= DateTime.Now)
                .OrderByDescending(a => a.PlanoPremium!.Ordem)
                .ThenByDescending(a => a.Fim)
                .FirstOrDefaultAsync();
        }

        private static int ObterNivelPremium(AssinaturaPremium? assinatura)
        {
            return assinatura?.PlanoPremium?.Nome switch
            {
                "Core" => 1,
                "Advanced" => 2,
                "Advanced Plus" => 3,
                _ => 0
            };
        }
    }
}

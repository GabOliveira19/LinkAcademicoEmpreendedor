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

        // Dashboard da Empresa
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

            var alunosRecentes = await _context.Alunos
                .Include(a => a.Projetos)
                .OrderByDescending(a => a.DataCadastro)
                .Take(5)
                .ToListAsync();

            var projetosDestaque = await _context.Projetos
                .Include(p => p.Aluno)
                .Include(p => p.Curtidas)
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.Curtidas.Count)
                .Take(6)
                .ToListAsync();

            var viewModel = new DashboardEmpresaViewModel
            {
                Empresa = empresa,
                MinhasOportunidades = empresa.Oportunidades.ToList(),
                AlunosRecentes = alunosRecentes,
                ProjetosDestaque = projetosDestaque,
                TotalCandidaturas = empresa.Oportunidades.Sum(o => o.Candidaturas?.Count ?? 0)
            };

            return View(viewModel);
        }

        // Perfil da Empresa (publico)
        public async Task<IActionResult> Perfil(int id)
        {
            var empresa = await _context.Empresas
                .Include(e => e.Oportunidades.Where(o => o.Ativa))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        // Editar Perfil
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas.FindAsync(userId);
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
                NomeResponsavel = empresa.NomeResponsavel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilEmpresaViewModel model, IFormFile? logoEmpresaFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != model.Id)
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresas.FindAsync(userId);
            if (empresa == null)
                return NotFound();

            ModelState.Remove("logoEmpresaFile");

            if (!ModelState.IsValid)
                return View(model);

            // Upload do logo
            if (logoEmpresaFile != null && logoEmpresaFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "empresas");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Deletar logo antigo se existir
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

            // Atualiza todos os campos
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

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", empresa.RazaoSocial);
            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Dashboard");
        }

        // Remover logo
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

        // Buscar Talentos
        public async Task<IActionResult> BuscarTalentos(string busca, string habilidade, string curso)
        {
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

            if (!string.IsNullOrEmpty(habilidade))
            {
                query = query.Where(a =>
                    a.Habilidades != null &&
                    a.Habilidades.Contains(habilidade));
            }

            if (!string.IsNullOrEmpty(curso))
            {
                query = query.Where(a =>
                    a.Curso != null &&
                    a.Curso.Contains(curso));
            }

            var alunos = query
                .AsEnumerable() // força ordenação em memória
                .OrderByDescending(a => a.Projetos.Sum(p => p.Curtidas.Count))
                .ToList();

            ViewBag.Busca = busca;
            ViewBag.Habilidade = habilidade;
            ViewBag.Curso = curso;

            return View(alunos);
        }
    }
}
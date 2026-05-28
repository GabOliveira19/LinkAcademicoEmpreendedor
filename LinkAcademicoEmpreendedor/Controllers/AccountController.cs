using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.ViewModels;
using LinkAcademicoEmpreendedor.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Net.Http.Headers;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                var tipo = HttpContext.Session.GetString("TipoUsuario");
                return tipo == "Aluno"
                    ? RedirectToAction("Dashboard", "Aluno")
                    : RedirectToAction("Dashboard", "Empresa");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string tipoUsuario)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                ViewBag.Erro = "Preencha todos os campos.";
                return View();
            }

            if (tipoUsuario == "Aluno")
            {
                var aluno = await _context.Alunos
                    .FirstOrDefaultAsync(a => a.Email == email);

                // VERIFICAR SENHA COM BCRYPT
                if (aluno != null && SenhaService.VerificarSenha(senha, aluno.Senha))
                {
                    HttpContext.Session.SetInt32("UserId", aluno.Id);
                    HttpContext.Session.SetString("UserName", aluno.Nome);
                    HttpContext.Session.SetString("TipoUsuario", "Aluno");
                    HttpContext.Session.SetString("UserFoto", aluno.FotoPerfil ?? "");
                    return RedirectToAction("Dashboard", "Aluno");
                }
            }
            else
            {
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Email == email);

                // VERIFICAR SENHA COM BCRYPT
                if (empresa != null && SenhaService.VerificarSenha(senha, empresa.Senha))
                {
                    HttpContext.Session.SetInt32("UserId", empresa.Id);
                    HttpContext.Session.SetString("UserName", empresa.RazaoSocial);
                    HttpContext.Session.SetString("TipoUsuario", "Empresa");
                    HttpContext.Session.SetString("UserFoto", empresa.LogoEmpresa ?? "");
                    return RedirectToAction("Dashboard", "Empresa");
                }
            }

            ViewBag.Erro = "Email ou senha inválidos.";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Cadastro Aluno
        public IActionResult CadastroAluno()
        {
            ViewBag.Areas = new SelectList(_context.Areas.OrderBy(a => a.Nome).ToList(), "Id", "Nome");
            return View(new CadastroAlunoViewModel());
        }

        // POST: Cadastro Aluno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastroAluno(CadastroAlunoViewModel model)
        {
            ViewBag.Areas = new SelectList(_context.Areas.OrderBy(a => a.Nome).ToList(), "Id", "Nome");

            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Alunos.AnyAsync(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Este email ja esta cadastrado.");
                return View(model);
            }

            // Mapear ViewModel para entidade Aluno
            var aluno = new Aluno
            {
                Nome = model.Nome,
                Email = model.Email,
                Senha = SenhaService.CriptografarSenha(model.Senha),
                Curso = model.Curso,
                Instituicao = model.Instituicao,
                AnoIngresso = model.AnoIngresso,
                DataCadastro = DateTime.Now,
                AreaId = model.AreaId // persistir área escolhida
            };

            aluno.RedesSociais = MontarRedesSociais(model.Plataformas, model.Urls);

            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Cadastro realizado com sucesso! Faca login.";
            return RedirectToAction("Login");
        }

        // GET: Cadastro Empresa
        public IActionResult CadastroEmpresa()
        {
            return View(new CadastroEmpresaViewModel());
        }

        // POST: Cadastro Empresa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastroEmpresa(CadastroEmpresaViewModel model)
        {
            if (!model.CnpjValidado)
            {
                ModelState.AddModelError("Cnpj", "Voce deve consultar o CNPJ ou usar o cadastro manual antes de cadastrar.");
                return View(model);
            }

            // Valida CNPJ
            if (!ValidarCnpj(model.Cnpj))
            {
                ModelState.AddModelError("Cnpj", "CNPJ invalido.");
                return View(model);
            }

            // Verifica situacao cadastral (apenas se foi preenchido)
            if (!string.IsNullOrEmpty(model.SituacaoCadastral))
            {
                var situacoesInvalidas = new[] { "INAPTA", "BAIXADA", "SUSPENSA", "NULA" };
                if (situacoesInvalidas.Any(s => model.SituacaoCadastral.ToUpper().Contains(s)))
                {
                    ModelState.AddModelError("Cnpj", $"Empresa com situacao cadastral invalida: {model.SituacaoCadastral}");
                    return View(model);
                }
            }

            // Verifica se CNPJ ja existe
            string cnpjLimpo = new string(model.Cnpj.Where(char.IsDigit).ToArray());
            if (await _context.Empresas.AnyAsync(e => e.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "") == cnpjLimpo))
            {
                ModelState.AddModelError("Cnpj", "Este CNPJ ja esta cadastrado.");
                return View(model);
            }

            // Verifica se email ja existe
            if (await _context.Empresas.AnyAsync(e => e.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Este e-mail ja esta cadastrado.");
                return View(model);
            }

            ModelState.Remove("CnpjValidado");
            ModelState.Remove("ConfirmarSenha");

            if (!ModelState.IsValid)
                return View(model);

            // Converte DataAbertura de string para DateTime
            DateTime? dataAbertura = null;
            if (!string.IsNullOrEmpty(model.DataAbertura))
            {
                string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd" };
                foreach (var formato in formatos)
                {
                    if (DateTime.TryParseExact(model.DataAbertura, formato, null, System.Globalization.DateTimeStyles.None, out var data))
                    {
                        dataAbertura = data;
                        break;
                    }
                }
                if (dataAbertura == null && DateTime.TryParse(model.DataAbertura, out var dataGeneric))
                {
                    dataAbertura = dataGeneric;
                }
            }

            // Cria a empresa
            var empresa = new Empresa
            {
                Cnpj = model.Cnpj,
                RazaoSocial = model.RazaoSocial,
                NomeFantasia = model.NomeFantasia,
                SituacaoCadastral = model.SituacaoCadastral,
                DataAbertura = dataAbertura,
                NaturezaJuridica = model.NaturezaJuridica,
                Endereco = model.Endereco,
                Numero = model.Numero,
                Bairro = model.Bairro,
                Cidade = model.Cidade,
                Estado = model.Estado,
                Cep = model.Cep,
                Email = model.Email,
                Senha = SenhaService.CriptografarSenha(model.Senha), // CRIPTOGRAFAR
                Telefone = model.Telefone,
                Descricao = model.Descricao,
                AreaAtuacao = model.AreaAtuacao,
                LogoEmpresa = model.LogoEmpresa,
                NomeResponsavel = model.NomeResponsavel,
                DataCadastro = DateTime.Now,
                Ativo = true
            };

            empresa.RedesSociais = MontarRedesSociais(model.Plataformas, model.Urls);

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Empresa cadastrada com sucesso! Faca login para continuar.";
            return RedirectToAction("Login");
        }

        // AJAX: Consultar CNPJ
        [HttpGet]
        public async Task<IActionResult> ConsultarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return Json(new { sucesso = false, mensagem = "CNPJ nao informado." });

            string cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

            if (!ValidarCnpj(cnpjLimpo))
                return Json(new { sucesso = false, mensagem = $"CNPJ invalido. Recebido: {cnpjLimpo}" });

            if (await _context.Empresas.AnyAsync(e => e.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "") == cnpjLimpo))
                return Json(new { sucesso = false, mensagem = "Este CNPJ ja esta cadastrado no sistema." });

            var dados = await ConsultarCnpjApiAsync(cnpjLimpo);

            if (dados == null)
                return Json(new { sucesso = false, mensagem = $"Nao foi possivel consultar o CNPJ {cnpjLimpo}. Use o cadastro manual." });

            var situacoesInvalidas = new[] { "INAPTA", "BAIXADA", "SUSPENSA", "NULA" };
            if (!string.IsNullOrEmpty(dados.SituacaoCadastral) && situacoesInvalidas.Any(s => dados.SituacaoCadastral.ToUpper().Contains(s)))
                return Json(new { sucesso = false, mensagem = $"Empresa com situacao cadastral invalida: {dados.SituacaoCadastral}" });

            return Json(new
            {
                sucesso = true,
                dados = new
                {
                    cnpj = FormatarCnpj(cnpjLimpo),
                    razaoSocial = dados.RazaoSocial,
                    nomeFantasia = dados.NomeFantasia,
                    situacaoCadastral = dados.SituacaoCadastral,
                    dataAbertura = dados.DataAbertura,
                    naturezaJuridica = dados.NaturezaJuridica,
                    endereco = dados.Endereco,
                    numero = dados.Numero,
                    bairro = dados.Bairro,
                    cidade = dados.Cidade,
                    estado = dados.Estado,
                    cep = FormatarCep(dados.Cep)
                }
            });
        }

        // Metodo privado para consultar API de CNPJ (com fallback)
        private async Task<CnpjDados?> ConsultarCnpjApiAsync(string cnpjLimpo)
        {
            var dados = await TentarBrasilApiAsync(cnpjLimpo);
            if (dados == null)
                dados = await TentarReceitaWsAsync(cnpjLimpo);
            if (dados == null)
                dados = await TentarCnpjWsAsync(cnpjLimpo);
            return dados;
        }

        private async Task<CnpjDados?> TentarBrasilApiAsync(string cnpjLimpo)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dados == null || string.IsNullOrEmpty(dados.Razao_social))
                    return null;

                return new CnpjDados
                {
                    Cnpj = dados.Cnpj ?? cnpjLimpo,
                    RazaoSocial = dados.Razao_social ?? "",
                    NomeFantasia = !string.IsNullOrEmpty(dados.Nome_fantasia) ? dados.Nome_fantasia : dados.Razao_social ?? "",
                    SituacaoCadastral = dados.Descricao_situacao_cadastral ?? "",
                    DataAbertura = dados.Data_inicio_atividade ?? "",
                    NaturezaJuridica = dados.Natureza_juridica ?? "",
                    Endereco = dados.Logradouro ?? "",
                    Numero = dados.Numero ?? "",
                    Bairro = dados.Bairro ?? "",
                    Cidade = dados.Municipio ?? "",
                    Estado = dados.Uf ?? "",
                    Cep = dados.Cep ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<CnpjDados?> TentarReceitaWsAsync(string cnpjLimpo)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"https://receitaws.com.br/v1/cnpj/{cnpjLimpo}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<ReceitaWsCnpjResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dados == null || dados.Status == "ERROR" || string.IsNullOrEmpty(dados.Nome))
                    return null;

                return new CnpjDados
                {
                    Cnpj = dados.Cnpj ?? cnpjLimpo,
                    RazaoSocial = dados.Nome ?? "",
                    NomeFantasia = !string.IsNullOrEmpty(dados.Fantasia) ? dados.Fantasia : dados.Nome ?? "",
                    SituacaoCadastral = dados.Situacao ?? "",
                    DataAbertura = dados.Abertura ?? "",
                    NaturezaJuridica = dados.Natureza_juridica ?? "",
                    Endereco = dados.Logradouro ?? "",
                    Numero = dados.Numero ?? "",
                    Bairro = dados.Bairro ?? "",
                    Cidade = dados.Municipio ?? "",
                    Estado = dados.Uf ?? "",
                    Cep = dados.Cep ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<CnpjDados?> TentarCnpjWsAsync(string cnpjLimpo)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"https://publica.cnpj.ws/cnpj/{cnpjLimpo}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<CnpjWsResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dados == null || string.IsNullOrEmpty(dados.Razao_social))
                    return null;

                return new CnpjDados
                {
                    Cnpj = cnpjLimpo,
                    RazaoSocial = dados.Razao_social ?? "",
                    NomeFantasia = !string.IsNullOrEmpty(dados.Estabelecimento?.Nome_fantasia) ? dados.Estabelecimento.Nome_fantasia : dados.Razao_social ?? "",
                    SituacaoCadastral = dados.Estabelecimento?.Situacao_cadastral ?? "",
                    DataAbertura = dados.Estabelecimento?.Data_inicio_atividade ?? "",
                    NaturezaJuridica = dados.Natureza_juridica?.Descricao ?? "",
                    Endereco = dados.Estabelecimento?.Logradouro ?? "",
                    Numero = dados.Estabelecimento?.Numero ?? "",
                    Bairro = dados.Estabelecimento?.Bairro ?? "",
                    Cidade = dados.Estabelecimento?.Cidade?.Nome ?? "",
                    Estado = dados.Estabelecimento?.Estado?.Sigla ?? "",
                    Cep = dados.Estabelecimento?.Cep ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        // Validar CNPJ
        private bool ValidarCnpj(string cnpj)
        {
            string cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpjLimpo.Length != 14)
                return false;

            if (cnpjLimpo.Distinct().Count() == 1)
                return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpjLimpo.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCnpj += digito;

            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cnpjLimpo.EndsWith(digito);
        }

        private string FormatarCnpj(string cnpj)
        {
            if (cnpj.Length != 14) return cnpj;
            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        private string FormatarCep(string? cep)
        {
            if (string.IsNullOrEmpty(cep)) return "";
            string cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
            if (cepLimpo.Length != 8) return cep;
            return $"{cepLimpo.Substring(0, 5)}-{cepLimpo.Substring(5, 3)}";
        }

        // GET: Alterar Senha
        public IActionResult AlterarSenha()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");

            return View();
        }

        // POST: Alterar Senha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(string senhaAtual, string novaSenha, string confirmarSenha)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null)
                return RedirectToAction("Login");

            if (novaSenha != confirmarSenha)
            {
                ViewBag.Erro = "As senhas nao conferem.";
                return View();
            }

            if (novaSenha.Length < 6)
            {
                ViewBag.Erro = "A nova senha deve ter no minimo 6 caracteres.";
                return View();
            }

            if (tipoUsuario == "Aluno")
            {
                var aluno = await _context.Alunos.FindAsync(userId);
                // VERIFICAR SENHA COM BCRYPT
                if (aluno == null || !SenhaService.VerificarSenha(senhaAtual, aluno.Senha))
                {
                    ViewBag.Erro = "Senha atual incorreta.";
                    return View();
                }
                // CRIPTOGRAFAR NOVA SENHA
                aluno.Senha = SenhaService.CriptografarSenha(novaSenha);
            }
            else
            {
                var empresa = await _context.Empresas.FindAsync(userId);
                // VERIFICAR SENHA COM BCRYPT
                if (empresa == null || !SenhaService.VerificarSenha(senhaAtual, empresa.Senha))
                {
                    ViewBag.Erro = "Senha atual incorreta.";
                    return View();
                }
                // CRIPTOGRAFAR NOVA SENHA
                empresa.Senha = SenhaService.CriptografarSenha(novaSenha);
            }

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Senha alterada com sucesso!";

            return tipoUsuario == "Aluno"
                ? RedirectToAction("Dashboard", "Aluno")
                : RedirectToAction("Dashboard", "Empresa");
        }

        // GET: Excluir Conta
        public IActionResult ExcluirConta()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            return View(new ExcluirContaViewModel());
        }

        // POST: Excluir Conta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConta(ExcluirContaViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            if (userId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            if (model.ConfirmacaoTexto != "EXCLUIR")
            {
                ModelState.AddModelError("ConfirmacaoTexto", "Digite 'EXCLUIR' para confirmar.");
                return View(model);
            }

            try
            {
                if (tipoUsuario == "Aluno")
                {
                    var aluno = await _context.Alunos.FindAsync(userId);

                    if (aluno == null)
                    {
                        HttpContext.Session.Clear();
                        TempData["Erro"] = "Conta nao encontrada.";
                        return RedirectToAction("Login");
                    }

                    // VERIFICAR SENHA COM BCRYPT
                    if (!SenhaService.VerificarSenha(model.Senha, aluno.Senha))
                    {
                        ModelState.AddModelError("Senha", "Senha incorreta.");
                        return View(model);
                    }

                    var NotificacoesAluno = await _context.Notificacoes
                        .Where(n => n.TipoDestinatario == "Aluno" && n.DestinatarioId == userId)
                        .ToListAsync();
                    if (NotificacoesAluno.Any())
                        _context.Notificacoes.RemoveRange(NotificacoesAluno);

                    var candidaturas = await _context.Candidaturas
                        .Where(c => c.AlunoId == userId)
                        .ToListAsync();
                    if (candidaturas.Any())
                        _context.Candidaturas.RemoveRange(candidaturas);

                    var curtidas = await _context.Curtidas
                        .Where(c => c.AlunoId == userId)
                        .ToListAsync();
                    if (curtidas.Any())
                        _context.Curtidas.RemoveRange(curtidas);

                    var comentarios = await _context.Comentarios
                        .Where(c => c.AlunoId == userId)
                        .ToListAsync();
                    if (comentarios.Any())
                        _context.Comentarios.RemoveRange(comentarios);

                    var projetos = await _context.Projetos
                        .Where(p => p.AlunoId == userId)
                        .ToListAsync();

                    foreach (var projeto in projetos)
                    {
                        var curtidasProjeto = await _context.Curtidas
                            .Where(c => c.ProjetoId == projeto.Id)
                            .ToListAsync();
                        if (curtidasProjeto.Any())
                            _context.Curtidas.RemoveRange(curtidasProjeto);

                        var comentariosProjeto = await _context.Comentarios
                            .Where(c => c.ProjetoId == projeto.Id)
                            .ToListAsync();
                        if (comentariosProjeto.Any())
                            _context.Comentarios.RemoveRange(comentariosProjeto);
                    }

                    if (projetos.Any())
                        _context.Projetos.RemoveRange(projetos);

                    _context.Alunos.Remove(aluno);
                }
                else
                {
                    var empresa = await _context.Empresas.FindAsync(userId);

                    if (empresa == null)
                    {
                        HttpContext.Session.Clear();
                        TempData["Erro"] = "Conta nao encontrada.";
                        return RedirectToAction("Login");
                    }

                    // VERIFICAR SENHA COM BCRYPT
                    if (!SenhaService.VerificarSenha(model.Senha, empresa.Senha))
                    {
                        ModelState.AddModelError("Senha", "Senha incorreta.");
                        return View(model);
                    }

                    var NotificacoesEmpresa = await _context.Notificacoes
                        .Where(n => n.TipoDestinatario == "Empresa" && n.DestinatarioId == userId)
                        .ToListAsync();
                    if (NotificacoesEmpresa.Any())
                        _context.Notificacoes.RemoveRange(NotificacoesEmpresa);

                    var oportunidades = await _context.Oportunidades
                        .Where(o => o.EmpresaId == userId)
                        .ToListAsync();

                    foreach (var oportunidade in oportunidades)
                    {
                        var candidaturasOp = await _context.Candidaturas
                            .Where(c => c.OportunidadeId == oportunidade.Id)
                            .ToListAsync();
                        if (candidaturasOp.Any())
                            _context.Candidaturas.RemoveRange(candidaturasOp);
                    }

                    if (oportunidades.Any())
                        _context.Oportunidades.RemoveRange(oportunidades);

                    _context.Empresas.Remove(empresa);
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Clear();

                TempData["Sucesso"] = "Sua conta foi excluida com sucesso.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Ocorreu um erro ao excluir a conta. Tente novamente.";
                return View(model);
            }
        }

        // GET: Esqueceu Senha
        public IActionResult EsqueceuSenha()
        {
            return View();
        }

        // POST: Esqueceu Senha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueceuSenha(string email, string tipoUsuario)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Erro = "Informe o email.";
                return View();
            }

            if (tipoUsuario == "Aluno")
            {
                var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Email == email);
                if (aluno == null)
                {
                    ViewBag.Erro = "Email nao encontrado.";
                    return View();
                }

                TempData["RecuperarSenhaId"] = aluno.Id;
                TempData["RecuperarSenhaTipo"] = "Aluno";
                TempData["RecuperarSenhaEmail"] = email;
                return RedirectToAction("RedefinirSenha");
            }
            else
            {
                var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Email == email);
                if (empresa == null)
                {
                    ViewBag.Erro = "Email nao encontrado.";
                    return View();
                }

                TempData["RecuperarSenhaId"] = empresa.Id;
                TempData["RecuperarSenhaTipo"] = "Empresa";
                TempData["RecuperarSenhaEmail"] = email;
                return RedirectToAction("RedefinirSenha");
            }
        }

        // GET: Redefinir Senha
        public IActionResult RedefinirSenha()
        {
            var id = TempData.Peek("RecuperarSenhaId");
            var tipo = TempData.Peek("RecuperarSenhaTipo");
            var email = TempData.Peek("RecuperarSenhaEmail");

            if (id == null || tipo == null)
            {
                TempData["Erro"] = "Sessao expirada. Tente novamente.";
                return RedirectToAction("EsqueceuSenha");
            }

            ViewBag.Email = email;
            return View();
        }

        // POST: Redefinir Senha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha(string novaSenha, string confirmarSenha)
        {
            var id = TempData["RecuperarSenhaId"];
            var tipo = TempData["RecuperarSenhaTipo"]?.ToString();

            if (id == null || tipo == null)
            {
                TempData["Erro"] = "Sessao expirada. Tente novamente.";
                return RedirectToAction("EsqueceuSenha");
            }

            if (string.IsNullOrEmpty(novaSenha) || novaSenha.Length < 6)
            {
                ViewBag.Erro = "A senha deve ter no minimo 6 caracteres.";
                TempData["RecuperarSenhaId"] = id;
                TempData["RecuperarSenhaTipo"] = tipo;
                return View();
            }

            if (novaSenha != confirmarSenha)
            {
                ViewBag.Erro = "As senhas nao conferem.";
                TempData["RecuperarSenhaId"] = id;
                TempData["RecuperarSenhaTipo"] = tipo;
                return View();
            }

            int userId = Convert.ToInt32(id);

            if (tipo == "Aluno")
            {
                var aluno = await _context.Alunos.FindAsync(userId);
                if (aluno != null)
                {
                    // CRIPTOGRAFAR NOVA SENHA
                    aluno.Senha = SenhaService.CriptografarSenha(novaSenha);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var empresa = await _context.Empresas.FindAsync(userId);
                if (empresa != null)
                {
                    // CRIPTOGRAFAR NOVA SENHA
                    empresa.Senha = SenhaService.CriptografarSenha(novaSenha);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Sucesso"] = "Senha alterada com sucesso! Faca login com a nova senha.";
            return RedirectToAction("Login");
        }
        private List<RedeSocial> MontarRedesSociais(List<string>? plataformas, List<string>? urls)
        {
            var redes = new List<RedeSocial>();

            if (plataformas == null || urls == null)
                return redes;

            for (int i = 0; i < plataformas.Count && i < urls.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(plataformas[i]) || string.IsNullOrWhiteSpace(urls[i]))
                    continue;

                redes.Add(new RedeSocial
                {
                    Plataforma = plataformas[i],
                    Url = NormalizarUrlRedeSocial(plataformas[i], urls[i])
                });
            }

            return redes;
        }

        private string NormalizarUrlRedeSocial(string plataforma, string url)
        {
            url = url.Trim();

            if (plataforma.Equals("E-mail", StringComparison.OrdinalIgnoreCase))
                return url.StartsWith("mailto:") ? url : $"mailto:{url}";

            if (plataforma.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                var numeros = new string(url.Where(char.IsDigit).ToArray());
                return url.StartsWith("http") ? url : $"https://wa.me/55{numeros}";
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                return $"https://{url}";

            return url;
        }
    }

    // Classes auxiliares para deserializacao da API
    public class CnpjDados
    {
        public string Cnpj { get; set; } = "";
        public string RazaoSocial { get; set; } = "";
        public string? NomeFantasia { get; set; }
        public string? SituacaoCadastral { get; set; }
        public string? DataAbertura { get; set; }
        public string? NaturezaJuridica { get; set; }
        public string? Endereco { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
    }

    public class BrasilApiCnpjResponse
    {
        public string? Cnpj { get; set; }
        public string? Razao_social { get; set; }
        public string? Nome_fantasia { get; set; }
        public string? Descricao_situacao_cadastral { get; set; }
        public string? Data_inicio_atividade { get; set; }
        public string? Natureza_juridica { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public string? Cep { get; set; }
    }

    public class ReceitaWsCnpjResponse
    {
        public string? Status { get; set; }
        public string? Cnpj { get; set; }
        public string? Nome { get; set; }
        public string? Fantasia { get; set; }
        public string? Situacao { get; set; }
        public string? Abertura { get; set; }
        public string? Natureza_juridica { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public string? Cep { get; set; }
    }

    public class CnpjWsResponse
    {
        public string? Razao_social { get; set; }
        public CnpjWsNaturezaJuridica? Natureza_juridica { get; set; }
        public CnpjWsEstabelecimento? Estabelecimento { get; set; }
    }

    public class CnpjWsNaturezaJuridica
    {
        public string? Descricao { get; set; }
    }

    public class CnpjWsEstabelecimento
    {
        public string? Nome_fantasia { get; set; }
        public string? Situacao_cadastral { get; set; }
        public string? Data_inicio_atividade { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cep { get; set; }
        public CnpjWsCidade? Cidade { get; set; }
        public CnpjWsEstado? Estado { get; set; }
    }

    public class CnpjWsCidade
    {
        public string? Nome { get; set; }
    }

    public class CnpjWsEstado
    {
        public string? Sigla { get; set; }
    }
}
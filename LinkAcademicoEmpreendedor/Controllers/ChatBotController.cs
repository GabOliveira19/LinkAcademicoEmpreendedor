    using Microsoft.AspNetCore.Mvc;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class ChatBotController : Controller
    {
        // Tela do chatbot
        public IActionResult Index()
        {
            return View();
        }

        // Recebe mensagens do usuário
        [HttpPost]
        public JsonResult EnviarMensagem(string mensagem)
        {
            mensagem = mensagem.ToLower();

            string resposta;
            string redirecionar = "";

            // Verifica login
            var userId = HttpContext.Session.GetInt32("UserId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");

            // =========================
            // SAUDAÇÕES
            // =========================
            if (mensagem.Contains("oi") ||
                mensagem.Contains("olá") ||
                mensagem.Contains("ola") ||
                mensagem.Contains("bom dia") ||
                mensagem.Contains("boa tarde") ||
                mensagem.Contains("boa noite"))
            {
                int hora = DateTime.Now.Hour;

                string saudacao;

                if (hora >= 5 && hora < 12)
                {
                    saudacao = "Bom dia";
                }
                else if (hora >= 12 && hora < 18)
                {
                    saudacao = "Boa tarde";
                }
                else
                {
                    saudacao = "Boa noite";
                }

                resposta = $"{saudacao}! Sou o assistente virtual do SkillBridge. Como posso ajudar você?";
            }

            // =========================
            // LOGIN
            // =========================
            else if (mensagem.Contains("login") ||
                     mensagem.Contains("entrar") ||
                     mensagem.Contains("acessar"))
            {
                resposta = "Utilize seu e-mail e senha cadastrados para acessar o sistema.";
            }

            // =========================
            // ALTERAR SENHA
            // =========================
            else if (mensagem.Contains("alterar senha") ||
                     mensagem.Contains("trocar senha") ||
                     mensagem.Contains("mudar senha"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado para alterar sua senha.";
                }
                else
                {
                    resposta = "Redirecionando para alteração de senha da sua conta...";
                    redirecionar = "/Account/AlterarSenha";
                }
            }

            // =========================
            // PERFIL
            // =========================
            else if (mensagem.Contains("perfil") ||
                     mensagem.Contains("editar perfil"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado para acessar seu perfil.";
                }
                else
                {
                    // Redireciona SOMENTE para o perfil do usuário logado
                    if (tipoUsuario == "Empresa")
                    {
                        resposta = "Redirecionando para o perfil da empresa...";
                        redirecionar = "/Empresa/EditarPerfil";
                    }
                    else
                    {
                        resposta = "Redirecionando para o seu perfil...";
                        redirecionar = "/Aluno/EditarPerfil";
                    }
                }
            }

            // =========================
            // CURRÍCULO
            // =========================
            else if (mensagem.Contains("meu currículo") ||
                     mensagem.Contains("meu curriculo") ||
                     mensagem.Contains("ver meu curriculo"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado para editar seu currículo.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Apenas alunos podem editar o currículo do seu perfil.";
                }
                else
                {
                    resposta = "Redirecionando para o seu currículo...";
                    redirecionar = "/Aluno/Dashboard";
                }
            }

            // =========================
            // FOTO
            // =========================
            else if (mensagem.Contains("foto"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else
                {
                    resposta = "Redirecionando para edição da sua foto de perfil...";

                    if (tipoUsuario == "Empresa")
                    {
                        redirecionar = "/Empresa/EditarPerfil";
                    }
                    else
                    {
                        redirecionar = "/Aluno/EditarPerfil";
                    }
                }
            }

            // =========================
            // HABILIDADES
            // =========================
            else if (mensagem.Contains("habilidade") ||
                     mensagem.Contains("habilidades"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Somente alunos podem alterar habilidades.";
                }
                else
                {
                    resposta = "Redirecionando para edição das suas habilidades...";
                    redirecionar = "/Aluno/EditarPerfil";
                }
            }

            // =========================
            // PROJETOS
            // =========================
            else if (mensagem.Contains("criar projeto") ||
                     mensagem.Contains("novo projeto") ||
                     mensagem.Contains("cadastrar projeto") ||
                     mensagem.Contains("adicionar projeto"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado para criar projetos.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Somente alunos podem criar projetos.";
                }
                else
                {
                    resposta = "Redirecionando para criação de projeto...";
                    redirecionar = "/Projeto/Criar";
                }
            }

            else if (mensagem.Contains("meus projetos") ||
                     mensagem.Contains("ver projetos"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Somente alunos possuem projetos.";
                }
                else
                {
                    resposta = "Redirecionando para seus projetos...";
                    redirecionar = "/Aluno/Dashboard";
                }
            }

            else if (mensagem.Contains("editar projeto"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Somente alunos podem editar projetos.";
                }
                else
                {
                    resposta = "Acesse seus projetos no dashboard para editar apenas os projetos da sua conta.";
                    redirecionar = "/Aluno/Dashboard";
                }
            }

            else if (mensagem.Contains("excluir projeto") ||
                     mensagem.Contains("remover projeto"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else if (tipoUsuario != "Aluno")
                {
                    resposta = "Somente alunos podem excluir projetos.";
                }
                else
                {
                    resposta = "Acesse seus projetos no dashboard para excluir apenas projetos da sua conta.";
                    redirecionar = "/Aluno/Dashboard";
                }
            }

            // =========================
            // VAGAS
            // =========================
            else if (mensagem.Contains("vaga") ||
                     mensagem.Contains("vagas") ||
                     mensagem.Contains("oportunidade") ||
                     mensagem.Contains("oportunidades"))
            {
                resposta = "Redirecionando para oportunidades...";
                redirecionar = "/Oportunidade";
            }

            // =========================
            // TALENTOS
            // =========================
            else if (mensagem.Contains("buscar talentos") ||
           mensagem.Contains("talento") ||
           mensagem.Contains("talentos") ||
           mensagem.Contains("buscar alunos"))
            {
                resposta = "Redirecionando para Talentos...";
                redirecionar = "/Aluno/Listar";
            }
            // =========================
            // CURRÍCULOS DE ALUNOS
            // =========================
            else if (mensagem.Contains("currículos") ||
                     mensagem.Contains("curriculos") ||
                     mensagem.Contains("ver currículos") ||
                     mensagem.Contains("ver curriculos") ||
                     mensagem.Contains("currículo de aluno") ||
                     mensagem.Contains("curriculo de aluno") ||
                     mensagem.Contains("currículo dos alunos") ||
                     mensagem.Contains("curriculo dos alunos") ||
                     mensagem.Contains("ver talentos") ||
                     mensagem.Contains("perfil dos alunos"))
            {
                resposta = "Redirecionando para a área de currículos e talentos. Para visualizar o currículo de um aluno específico, acesse o perfil do aluno na seção: Talentos.";

                redirecionar = "/Aluno/Listar";
            }

            // =========================
            // EMPRESA
            // =========================
            else if (mensagem.Contains("perfil empresa") ||
                     mensagem.Contains("perfil da empresa") ||
                     mensagem.Contains("minha empresa") ||
                     mensagem.Contains("dados da empresa"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado como empresa.";
                }
                else if (tipoUsuario != "Empresa")
                {
                    resposta = "Somente empresas podem acessar o perfil empresarial.";
                }
                else
                {
                    resposta = "Redirecionando para o perfil da empresa...";
                    redirecionar = "/Empresa/EditarPerfil";
                }
            }

            // =========================
            // LOGO
            // =========================
            else if (mensagem.Contains("logo"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else if (tipoUsuario != "Empresa")
                {
                    resposta = "Somente empresas podem alterar logos.";
                }
                else
                {
                    resposta = "Redirecionando para alteração da logo da empresa...";
                    redirecionar = "/Empresa/EditarPerfil";
                }
            }

            // =========================
            // SEJA PREMIUM
            // =========================
            else if (mensagem.Contains("premium") ||
                     mensagem.Contains("seja premium") ||
                     mensagem.Contains("plano premium") ||
                     mensagem.Contains("planos premium") ||
                     mensagem.Contains("assinatura"))
            {
                if (userId == null)
                {
                    resposta = "O Seja Premium é exclusivo para empresas. Faça login como empresa para visualizar os planos Core, Advanced e Advanced Plus.";
                }
                else if (tipoUsuario != "Empresa")
                {
                    resposta = "O Seja Premium está disponível para empresas. Ele oferece selo premium, mais visibilidade nas vagas, busca ampliada de talentos e filtros avançados conforme o plano.";
                }
                else
                {
                    resposta = "Redirecionando para os planos Premium da empresa. Você poderá escolher entre Core, Advanced e Advanced Plus.";
                    redirecionar = "/Premium/Planos";
                }
            }

            // =========================
            // EXCLUIR CONTA
            // =========================
            else if (mensagem.Contains("excluir conta") ||
                     mensagem.Contains("deletar conta"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado para excluir sua conta.";
                }
                else
                {
                    resposta = "Redirecionando para gerenciamento da sua conta...";
                    redirecionar = "/Account/ExcluirConta";
                }
            }

            // =========================
            // DASHBOARD
            // =========================
            else if (mensagem.Contains("dashboard") ||
                     mensagem.Contains("painel"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else
                {
                    resposta = "Redirecionando para seu painel...";

                    if (tipoUsuario == "Empresa")
                    {
                        redirecionar = "/Empresa/Dashboard";
                    }
                    else
                    {
                        redirecionar = "/Aluno/Dashboard";
                    }
                }
            }

            // =========================
            // AVALIAÇÕES
            // =========================
            else if (mensagem.Contains("avaliações") ||
                     mensagem.Contains("avaliação") ||
                     mensagem.Contains("avaliacao") ||
                     mensagem.Contains("avaliar"))
            {
                if (userId == null)
                {
                    resposta = "Você precisa estar logado.";
                }
                else
                {
                    resposta = "Redirecionando para a área de avaliações...";
                    redirecionar = "/Avaliacao/MinhasAvaliacoes";
                }
            }

            // =========================
            // RANKING DE ALUNOS
            // =========================
            else if (mensagem.Contains("ranking alunos") ||
                     mensagem.Contains("ranking de alunos") ||
                     mensagem.Contains("melhores alunos"))
            {
                resposta = "Redirecionando para o ranking de alunos...";
                redirecionar = "/Ranking/Alunos";
            }

            // =========================
            // RANKING DE EMPRESAS
            // =========================
            else if (mensagem.Contains("ranking empresas") ||
                     mensagem.Contains("ranking de empresas") ||
                     mensagem.Contains("melhores empresas"))
            {
                resposta = "Redirecionando para o ranking de empresas...";
                redirecionar = "/Ranking/Empresas";
            }

            // =========================
            // SISTEMA
            // =========================
            else if (mensagem.Contains("sistema") ||
                     mensagem.Contains("plataforma") ||
                     mensagem.Contains("skillbridge"))
            {
                resposta = "O SkillBridge conecta alunos e empresas através de projetos, talentos e oportunidades.";
            }

            // =========================
            // CHATBOT
            // =========================
            else if (mensagem.Contains("assistente") ||
                     mensagem.Contains("ia") ||
                     mensagem.Contains("chatbot"))
            {
                resposta = "Sou o assistente virtual do SkillBridge e posso ajudar você com dúvidas e navegação da plataforma.";
            }

            else if (mensagem.Contains("ajuda") ||
         mensagem.Contains("suporte") ||
         mensagem.Contains("não ajudou") ||
         mensagem.Contains("nao ajudou") ||
         mensagem.Contains("não resolveu") ||
         mensagem.Contains("nao resolveu"))
            {
                resposta = "Redirecionando para a Central de Ajuda completa do SkillBridge...";
                redirecionar = "/Ajuda";
            }

            // =========================
            // RESPOSTA PADRÃO
            // =========================
            else
            {
                resposta = "Desculpe, não entendi sua pergunta. Você pode perguntar sobre perfil, currículo, projetos, vagas, talentos, empresas e funcionalidades do SkillBridge.";
            }

            return Json(new
            {
                resposta,
                redirecionar
            });
        }
    }
}

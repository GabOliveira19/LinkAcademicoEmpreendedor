using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Services;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class MigracaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MigracaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ACESSE: /Migracao/CriptografarSenhas
        public async Task<IActionResult> CriptografarSenhas()
        {
            int alunosAtualizados = 0;
            int empresasAtualizadas = 0;

            // Criptografar senhas dos Alunos
            var alunos = await _context.Alunos.ToListAsync();
            foreach (var aluno in alunos)
            {
                if (!string.IsNullOrEmpty(aluno.Senha) && !SenhaService.SenhaEstaCriptografada(aluno.Senha))
                {
                    aluno.Senha = SenhaService.CriptografarSenha(aluno.Senha);
                    alunosAtualizados++;
                }
            }

            // Criptografar senhas das Empresas
            var empresas = await _context.Empresas.ToListAsync();
            foreach (var empresa in empresas)
            {
                if (!string.IsNullOrEmpty(empresa.Senha) && !SenhaService.SenhaEstaCriptografada(empresa.Senha))
                {
                    empresa.Senha = SenhaService.CriptografarSenha(empresa.Senha);
                    empresasAtualizadas++;
                }
            }

            await _context.SaveChangesAsync();

            return Content($"Migracao concluida!\n\n" +
                           $"Alunos atualizados: {alunosAtualizados}\n" +
                           $"Empresas atualizadas: {empresasAtualizadas}\n\n" +
                           $"IMPORTANTE: Apague o MigracaoController.cs agora!");
        }
    }
}
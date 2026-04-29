using BCrypt.Net;

namespace LinkAcademicoEmpreendedor.Services
{
    public static class SenhaService
    {
        public static string CriptografarSenha(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerificarSenha(string senhaDigitada, string senhaHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(senhaDigitada, senhaHash);
            }
            catch
            {
                return senhaDigitada == senhaHash;
            }
        }

        public static bool SenhaEstaCriptografada(string senha)
        {
            return senha != null &&
                   (senha.StartsWith("$2a$") ||
                    senha.StartsWith("$2b$") ||
                    senha.StartsWith("$2y$"));
        }
    }
}
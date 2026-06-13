using LinkAcademicoEmpreendedor.Models;
using System;
using System.Linq;

namespace LinkAcademicoEmpreendedor.Helpers
{
    public static class PremiumHelper
    {
        public static bool IsPremiumAtivo(Empresa empresa)
        {
            if (empresa?.AssinaturasPremium == null)
                return false;

            return empresa.AssinaturasPremium.Any(a => a.Status == "Ativa" && a.Fim > DateTime.Now);
        }

        public static int ObterNivelPremium(Empresa empresa)
        {
            var assinatura = ObterAssinaturaAtiva(empresa);
            return ObterNivelPorNome(assinatura?.PlanoPremium?.Nome);
        }

        public static AssinaturaPremium? ObterAssinaturaAtiva(Empresa empresa)
        {
            return empresa?.AssinaturasPremium?
                .Where(a => a.Status == "Ativa" && a.Fim > DateTime.Now)
                .OrderByDescending(a => a.PlanoPremium?.Ordem ?? 0)
                .FirstOrDefault();
        }

        public static int ObterNivelPorNome(string? planoNome)
        {
            return planoNome switch
            {
                "Core" => 1,
                "Advanced" => 2,
                "Advanced Plus" => 3,
                _ => 0
            };
        }
    }
}

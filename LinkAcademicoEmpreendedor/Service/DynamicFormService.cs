using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkAcademicoEmpreendedor.Services
{
    public class DynamicFormService
    {
        public Task<Dictionary<string, string>> GetFieldsByAreaIdAsync(int areaId)
        {
            var fields = new Dictionary<string, string>();

            switch (areaId)
            {
                case 1: // Tecnologia
                    fields.Add("Github", "Link do GitHub");
                    fields.Add("Stack", "Tecnologias utilizadas");
                    fields.Add("Deploy", "Link do deploy");
                    break;

                case 2: // Saúde
                    fields.Add("Instituicao", "Instituição/Hospital");
                    fields.Add("Metodologia", "Metodologia científica");
                    fields.Add("Artigos", "Referências/Artigos");
                    break;

                case 3: // Engenharia
                    fields.Add("Materiais", "Materiais utilizados");
                    fields.Add("Normas", "Normas técnicas");
                    break;

                case 4: // Design
                    fields.Add("Ferramentas", "Ferramentas utilizadas");
                    fields.Add("Portfolio", "Link do portfólio");
                    break;

                default:
                    fields.Add("Descricao", "Descrição geral");
                    break;
            }

            return Task.FromResult(fields);
        }
    }
}
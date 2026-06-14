using System.Collections.Generic;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class PacoteTokenItemViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public int QuantidadeTokens { get; set; }

        public decimal Valor { get; set; }
    }

    public class PacotesTokenViewModel
    {
        public List<PacoteTokenItemViewModel> Pacotes { get; set; } = new List<PacoteTokenItemViewModel>();
    }
}
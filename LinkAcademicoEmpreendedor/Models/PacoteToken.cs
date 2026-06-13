namespace LinkAcademicoEmpreendedor.Models
{
    public class PacoteToken
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public int QuantidadeTokens { get; set; }

        public decimal Valor { get; set; }

        public bool Ativo { get; set; } = true;
    }
}

namespace LinkAcademicoEmpreendedor.Models
{
    public class PlanoPremium
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
        public string Beneficios { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }
}
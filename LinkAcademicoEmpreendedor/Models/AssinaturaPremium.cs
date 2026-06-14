namespace LinkAcademicoEmpreendedor.Models
{
    public class AssinaturaPremium
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int PlanoPremiumId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Status { get; set; }
        public string? MercadoPagoPreferenceId { get; set; }

        public string? MercadoPagoPaymentId { get; set; }

        public decimal ValorPago { get; set; }
        public Empresa? Empresa { get; set; }
        public PlanoPremium? PlanoPremium { get; set; }
    }
}
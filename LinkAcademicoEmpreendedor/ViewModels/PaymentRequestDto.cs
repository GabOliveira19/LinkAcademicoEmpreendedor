namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class PaymentRequestDto
    {
        public decimal transaction_amount { get; set; }
        public string token { get; set; }
        public string description { get; set; }
        public int installments { get; set; }
        public string payment_method_id { get; set; }
        public string issuer_id { get; set; }
        public PayerDto payer { get; set; }
    }

    public class TokenPaymentRequestDto : PaymentRequestDto
    {
        public int PacoteId { get; set; }
    }

    public class PremiumPaymentRequestDto : PaymentRequestDto
    {
        public int PlanoId { get; set; }
    }

    public class PayerDto
    {
        public string email { get; set; }
        public IdentificationDto identification { get; set; }
    }

    public class IdentificationDto
    {
        public string type { get; set; }
        public string number { get; set; }
    }
}

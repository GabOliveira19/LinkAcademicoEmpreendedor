using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.Settings
{
    public class MercadoPagoSettings
    {
        public const string SectionName = "MercadoPago";

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string PublicKey { get; set; } = string.Empty;
    }
}
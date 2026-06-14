using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.Models
{
    public class ConfiguracaoVisualUsuario
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        [Required]
        [StringLength(10)]
        public string TipoUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Tema { get; set; } = "Claro";

        [Required]
        [StringLength(20)]
        public string TamanhoFonte { get; set; } = "Normal";

        [Required]
        [StringLength(20)]
        public string Densidade { get; set; } = "Confortavel";

        public bool ReduzirAnimacoes { get; set; }

        public bool ModoDaltonico { get; set; }

        public bool ReduzirCores { get; set; }

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;
    }
}

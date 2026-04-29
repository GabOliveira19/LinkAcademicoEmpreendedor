using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }

        [Required]
        public int AvaliadorId { get; set; }

        [Required]
        public int AvaliadoId { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoAvaliador { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TipoAvaliado { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Nota { get; set; }

        [StringLength(500)]
        public string? Comentario { get; set; }

        public int? OportunidadeId { get; set; }

        // IMPORTANTE: Nome deve ser DataAvaliacao (igual a tabela)
        public DateTime DataAvaliacao { get; set; } = DateTime.Now;

        // Propriedades nao mapeadas
        [NotMapped]
        public string? NomeAvaliador { get; set; }

        [NotMapped]
        public string? NomeAvaliado { get; set; }

        [NotMapped]
        public string? FotoAvaliador { get; set; }

        [NotMapped]
        public string? FotoAvaliado { get; set; }
    }
}
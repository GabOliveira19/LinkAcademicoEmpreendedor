using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Notificacao
    {
        public int Id { get; set; }

        [Required]
        public int DestinatarioId { get; set; }

        [Required]
        [StringLength(10)]
        public string TipoDestinatario { get; set; } = string.Empty; // "Aluno" ou "Empresa"

        [Required]
        [StringLength(200)]
        public string Mensagem { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Titulo { get; set; }

        [StringLength(200)]
        public string? Link { get; set; }

        public bool Lida { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
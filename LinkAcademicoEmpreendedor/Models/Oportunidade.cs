using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Oportunidade
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O titulo e obrigatorio")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descricao e obrigatoria")]
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Tipo { get; set; }

        [StringLength(200)]
        public string? Requisitos { get; set; }

        [StringLength(100)]
        public string? Local { get; set; }

        public string? Modalidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Salario { get; set; }

        public DateTime DataPublicacao { get; set; } = DateTime.Now;

        public DateTime? DataExpiracao { get; set; }

        public bool Ativa { get; set; } = true;

        public int EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        // Navegacao para candidaturas
        public virtual ICollection<Candidatura> Candidaturas { get; set; } = new List<Candidatura>();

        [NotMapped]
        public int TotalCandidaturas => Candidaturas?.Count ?? 0;
    }
}
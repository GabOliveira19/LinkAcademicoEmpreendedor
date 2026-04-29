using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Candidatura
    {
        public int Id { get; set; }

        public DateTime DataCandidatura { get; set; } = DateTime.Now;

        [StringLength(4000)]  // Aumentado de 1000 para 4000 para ter mais oportunidades de candidatura
        public string? MensagemApresentacao { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pendente";

        public DateTime? DataVisualizacao { get; set; }

        public DateTime? DataResposta { get; set; }

        [StringLength(2000)]  // Aumentado de 500 para 2000
        public string? MensagemResposta { get; set; }

        public int AlunoId { get; set; }
        public int OportunidadeId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        [ForeignKey("OportunidadeId")]
        public virtual Oportunidade? Oportunidade { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Entrevista
    {
        public int Id { get; set; }

        public int CandidaturaId { get; set; }
        public int AlunoId { get; set; }
        public int EmpresaId { get; set; }
        public int OportunidadeId { get; set; }

        [Required]
        [StringLength(160)]
        public string Titulo { get; set; } = string.Empty;

        public DateTime DataHora { get; set; }

        public int DuracaoMinutos { get; set; } = 45;

        [StringLength(1000)]
        public string? Observacoes { get; set; }

        [Required]
        [StringLength(120)]
        public string CodigoSala { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Agendada";

        public DateTime CriadaEm { get; set; } = DateTime.Now;

        [ForeignKey("CandidaturaId")]
        public virtual Candidatura? Candidatura { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        [ForeignKey("OportunidadeId")]
        public virtual Oportunidade? Oportunidade { get; set; }
    }
}

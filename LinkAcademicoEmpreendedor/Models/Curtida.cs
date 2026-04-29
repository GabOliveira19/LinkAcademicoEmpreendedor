using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Curtida
    {
        public int Id { get; set; }

        public DateTime DataCurtida { get; set; } = DateTime.Now;

        // Chaves estrangeiras: permite que Aluno OU Empresa curtam
        public int? AlunoId { get; set; }
        public int? EmpresaId { get; set; }
        public int ProjetoId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
    }
}
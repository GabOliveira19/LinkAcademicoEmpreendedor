using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class FavoritoTalento
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }
        public int AlunoId { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("EmpresaId")]
        public Empresa? Empresa { get; set; }

        [ForeignKey("AlunoId")]
        public Aluno? Aluno { get; set; }
    }
}

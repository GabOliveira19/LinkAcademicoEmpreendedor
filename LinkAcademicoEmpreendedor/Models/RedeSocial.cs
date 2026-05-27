using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.Models
{
    // Crie esta classe se ainda não existir
    public class RedeSocial
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Plataforma { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Url { get; set; } = string.Empty;

        public int? AlunoId { get; set; }
        public Aluno? Aluno { get; set; }

        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }
}
using System.Collections.Generic;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Area
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        // opcional: Descricao breve
        public string? Descricao { get; set; }

        // Navegação (opcional)
        public virtual ICollection<Aluno>? Alunos { get; set; }
    }
}
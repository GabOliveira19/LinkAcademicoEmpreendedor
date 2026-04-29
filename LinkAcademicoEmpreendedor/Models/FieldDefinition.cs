namespace LinkAcademicoEmpreendedor.Models
{
    // Metadata mínima para campos dinâmicos por área.
    public class FieldDefinition
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public string Nome { get; set; } = string.Empty; // ex: "GitHub", "Materiais Utilizados"
        public string Tipo { get; set; } = "string"; // ex: string, number, boolean, file, select, textarea, url
        public bool Obrigatorio { get; set; } = false;
        public string? OpcoesJson { get; set; } // para campos select / opções em JSON
        public int Ordem { get; set; } = 0;

        public Area? Area { get; set; }
    }
}
namespace LinkAcademicoEmpreendedor.Models
{
    public class MovimentacaoToken
    {
        public int Id { get; set; }

        public int AlunoId { get; set; }

        public int Quantidade { get; set; }

        public string Tipo { get; set; }

        public DateTime Data { get; set; }
    }
}

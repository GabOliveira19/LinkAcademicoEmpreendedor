namespace LinkAcademicoEmpreendedor.Models
{
    public class CarteiraToken
    {
        public int Id { get; set; }

        public int AlunoId { get; set; }

        public int Saldo { get; set; }

        public Aluno Aluno { get; set; }
    }
}

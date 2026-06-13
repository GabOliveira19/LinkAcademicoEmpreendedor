using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class DashboardAlunoViewModel
    {
        public Aluno? Aluno { get; set; }
        public List<Projeto> MeusProjetos { get; set; } = new List<Projeto>();
        public List<Oportunidade> OportunidadesRecentes { get; set; } = new List<Oportunidade>();
        public List<Candidatura> MinhasCandidaturas { get; set; } = new List<Candidatura>();
        public int TotalCurtidas { get; set; }
        public int TotalComentarios { get; set; }
        public int SaldoTokens { get; set; }
    }

    public class DashboardEmpresaViewModel
    {
        public Empresa? Empresa { get; set; }
        public List<Oportunidade> MinhasOportunidades { get; set; } = new List<Oportunidade>();
        public List<Aluno> AlunosRecentes { get; set; } = new List<Aluno>();
        public List<Projeto> ProjetosDestaque { get; set; } = new List<Projeto>();
        public int TotalCandidaturas { get; set; }
        public AssinaturaPremium? AssinaturaPremium { get; set; }
        public bool PodeRenovarPremium { get; set; }
    }
}
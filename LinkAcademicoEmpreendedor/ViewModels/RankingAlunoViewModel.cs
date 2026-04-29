namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class RankingAlunoViewModel
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? FotoPerfil { get; set; }
        public string? Curso { get; set; }
        public string? Instituicao { get; set; }

        public int TotalComentarios { get; set; }
        public int TotalCurtidas { get; set; }
        public int TotalProjetos { get; set; }

        public int PontuacaoTotal { get; set; }

        public int Posicao { get; set; }

        // Calcula pontuacao com pesos
        public void CalcularPontuacao()
        {
            // Pesos: Projetos = 10 pontos, Comentarios = 3 pontos, Curtidas = 1 ponto
            PontuacaoTotal = (TotalProjetos * 10) + (TotalComentarios * 3) + (TotalCurtidas * 1);
        }
    }
}
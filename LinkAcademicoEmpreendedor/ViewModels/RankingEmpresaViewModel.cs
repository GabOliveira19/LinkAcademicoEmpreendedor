namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class RankingEmpresaViewModel
    {
        public int EmpresaId { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string? LogoEmpresa { get; set; }
        public string? AreaAtuacao { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }

        public int TotalAvaliacoes { get; set; }
        public double MediaAvaliacoes { get; set; }

        public int Posicao { get; set; }
    }
}
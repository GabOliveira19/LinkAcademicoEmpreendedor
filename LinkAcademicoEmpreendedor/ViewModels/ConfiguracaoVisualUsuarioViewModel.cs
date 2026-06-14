namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class ConfiguracaoVisualUsuarioViewModel
    {
        public string Tema { get; set; } = "Claro";
        public string TamanhoFonte { get; set; } = "Normal";
        public string Densidade { get; set; } = "Confortavel";
        public bool ReduzirAnimacoes { get; set; }
        public bool ModoDaltonico { get; set; }
        public bool ReduzirCores { get; set; }
    }
}

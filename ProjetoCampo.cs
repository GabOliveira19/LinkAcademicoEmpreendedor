public class ProjetoCampo {
    public int Id { get; set; }
    public int ProjetoId { get; set; }
    public string Chave { get; set; } = "";
    public string Valor { get; set; } = "";
    public Projeto Projeto { get; set; } = null!;
}
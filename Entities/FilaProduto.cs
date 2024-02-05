namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaProduto
{
    public int Id { get; set; }
    public int? ProdutoGlokId { get; set; }
    public string Codigo { get; set; }
    public string Descricao { get; set; }
    public float Cubagem { get; set; }
    public float PesoLiquido { get; set; }
    public float PesoBruto { get; set; }
    public float Valor { get; set; }
    public string Unidade { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
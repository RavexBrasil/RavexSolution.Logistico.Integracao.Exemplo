namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaPedidoItem
{
    public int Id { get; set; }
    public int FilaPedidoId { get; set; }
    public int? PedidoItemGlokId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public string? CodigoProduto { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
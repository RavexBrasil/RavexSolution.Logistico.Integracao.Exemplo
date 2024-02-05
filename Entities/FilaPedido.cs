namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaPedido
{
    public int Id { get; set; }
    public int? PedidoGlokId { get; set; }
    public string Numero { get; set; }
    public DateTime EstimativaEntrega { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal Cubagem { get; set; }
    public decimal ValorPedido { get; set; }
    public string CodigoPontoReferencia { get; set; }
    public string CnpjUnidade { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
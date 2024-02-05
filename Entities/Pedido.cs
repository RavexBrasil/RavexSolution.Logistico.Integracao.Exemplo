namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class Pedido
{
    public string Numero { get; set; }
    public DateTime EstimativaEntrega { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal Cubagem { get; set; }
    public decimal ValorPedido { get; set; }
    public long PontoReferenciaId { get; set; }
    public int UnidadeId { get; set; }
}
namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class PedidoItem
{
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public int ProdutoId { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
}
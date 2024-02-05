namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class NotaFiscalItem
{
    public int Sequencia { get; set; }
    public float ValorUnitario { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public short Prioridade { get; set; }
    public int? ProdutoId { get; set; }
}
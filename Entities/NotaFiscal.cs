namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class NotaFiscal
{
    public short Sequencia { get; set; }
    public int ViagemId { get; set; }
    public int EntregaId { get; set; }
    public string Numero { get; set; }
    public DateTime CriadoDataHora { get; set; }
    public int Serie { get; set; }
    public string TipoOperacao { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal Cubagem { get; set; }
    public float Valor { get; set; }
    public short QuantidadeCaixas { get; set; }
    public int QuantidadeEstimadaItens { get; set; }
}
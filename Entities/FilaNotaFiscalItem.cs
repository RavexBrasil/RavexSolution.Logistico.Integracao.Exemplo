namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaNotaFiscalItem
{
    public int Id { get; set; }
    public int? NotaFiscalItemGlokId { get; set; }
    public int FilaNotaFiscalId { get; set; }
    public byte Sequencia { get; set; }
    public float ValorUnitario { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public short Prioridade { get; set; }
    public string? CodigoProduto { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaNotaFiscal
{
    public int Id { get; set; }
    public int FilaEntregaId { get; set; }
    public int? NotaFiscalGlokId { get; set; }
    public short Sequencia { get; set; }
    public string TipoOperacao { get; set; }
    public string Numero { get; set; }
    public int Serie { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal Cubagem { get; set; }
    public float Valor { get; set; }
    public short QuantidadeCaixas { get; set; }
    public int QuantidadeEstimadaItens { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
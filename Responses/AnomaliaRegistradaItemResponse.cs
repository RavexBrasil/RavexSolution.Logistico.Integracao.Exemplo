namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

public class AnomaliaRegistradaItemResponse
{
    public string Codigo { get; set; }
    public int ItemId { get; set; }
    public decimal? QuantidadeDevolvida { get; set; }
    public decimal? PesoBrutoDevolvido { get; set; }
    public decimal? PesoLiquidoDevolvido { get; set; }
    public string? NotaFiscalDevolucao { get; set; }
    public string? SerieNotaFiscalDevolucao { get; set; }
    public TipoAnomaliaResponse Motivo { get; set; }
}
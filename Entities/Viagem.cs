namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class Viagem
{
    public string Identificador { get; set; }
    public DateTime EstimativaInicio { get; set; }
    public DateTime EstimativaFim { get; set; }
    public string Tipo { get; set; }
    public decimal PesoBrutoTotal { get; set; }
    public decimal PesoLiquidoTotal { get; set; }
    public float Valor { get; set; }
    public decimal KmEstimado { get; set; }
    public short QuantidadeCaixas { get; set; }
    public bool PossuiOrdemEspecial { get; set; }
    public bool ComputarIndicador { get; set; }
    public int EmbarcadorId { get; set; }
    public int UnidadeId { get; set; }
    public int CooperativaId { get; set; }
    public int TransportadoraId { get; set; }
}
namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class Entregas
{
    public int ViagemId { get; set; }
    public short Sequencia { get; set; }
    public int QuantidadeEstimadaNotasFiscais { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal Cubagem { get; set; }
    public long PontoReferenciaId { get; set; }
}
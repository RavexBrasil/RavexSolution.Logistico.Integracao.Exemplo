namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class PontoReferencia
{
    public string Codigo { get; set; }
    public char TipoPessoa { get; set; }
    public string Nome { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int GrupoReferenciaId { get; set; }
}
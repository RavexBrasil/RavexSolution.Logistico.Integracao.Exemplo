namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

public class TipoAnomaliaResponse
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    public string? Codigo { get; set; }
    public DepartamentoResponse Setor { get; set; }
}
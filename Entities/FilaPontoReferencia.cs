namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaPontoReferencia
{
    public int Id { get; set; }
    public int? PontoReferenciaGlokId { get; set; }
    public string Codigo { get; set; }
    public char TipoPessoa { get; set; }
    public string Nome { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string NomeGrupoReferencia { get; set; }
    public DateTime? LidoDataHora { get; set; }
    public bool ProcessadoComSucesso { get; set; }
    public bool ProcessadoComFalha { get; set; }
    public string? Observacao { get; set; }
    public int Tentativas { get; set; }
}
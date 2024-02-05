using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class AnomaliaRegistradaResponseV2
{
    [DataMember] public int TipoRetorno { get; set; }
    [DataMember] public string SenhaControle { get; set; }
    [DataMember] public int NotaFiscalId { get; set; }
    [DataMember] public string Observacao { get; set; }
    [DataMember] public string NumeroNotaFiscal { get; set; }
    [DataMember] public string SerieNotaFiscal { get; set; }
    [DataMember] public bool DevolucaoContabil { get; set; }
    [DataMember] public decimal? Latitude { get; set; }
    [DataMember] public decimal? Longitude { get; set; }
    [DataMember] public DateTime? DataHoraOcorrencia { get; set; }
    [DataMember] public DateTime? DataHoraAceite { get; set; }
    [DataMember] public UsuarioAnomaliaResponse Usuario { get; set; }
    [DataMember] public OperadorResponse Operador { get; set; }
    [DataMember] public TipoAnomaliaResponse Motivo { get; set; }
    [DataMember] public AnomaliaRegistradaItemResponse Item { get; set; }
}
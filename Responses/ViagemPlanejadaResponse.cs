using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ViagemPlanejadaResponse
{
    [DataMember] public long Id { get; set; }
    [DataMember] public DateTime CriadoDatahora { get; set; }
}
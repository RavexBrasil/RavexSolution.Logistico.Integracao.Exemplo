using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ViagensFinalizadasResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string Identificador { get; set; }
    [DataMember] public DateTime DataFimViagem { get; set; }
    [DataMember] public bool PossuiCustoAdicional { get; set; }
    [DataMember] public bool PossuiPernoite { get; set; }
    [DataMember] public bool PossuiAnomalia { get; set; }
    [DataMember] public IEnumerable<PernoitesAprovadosResponse>? Pernoites { get; set; }
    [DataMember] public IEnumerable<AnomaliaRegistradaResponseV2>? Anomalias { get; set; }
    [DataMember] public IEnumerable<CustosAdicionaisAprovadosResponse>? CustosAdicionais { get; set; }
}
using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class RoteiroResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string? IdentificadorRota { get; set; }
    [DataMember] public DateTime? DataInicioPrevisto { get; set; }
    [DataMember] public DateTime? DataFimPrevisto { get; set; }
    [DataMember] public int? DistanciaCalculadaEmMetros { get; set; }
    [DataMember] public int? DuracaoCalculadaEmMetros { get; set; }
    [DataMember] public int? DistanciaAjustadaEmMetros { get; set; }
    [DataMember] public int? DuracaoAjustadaEmMinutos { get; set; }
    [DataMember] public string Tipo { get; set; }
    [DataMember] public ViagemPlanejadaResponse? ViagemPlanejada { get; set; }
    [DataMember] public ViagemFaturadaResponse? ViagemFaturada { get; set; }
    [DataMember] public ListaVeiculoResponse? Veiculo { get; set; }
    [DataMember] public ListaVeiculoResponse? Spot { get; set; }
    [DataMember] public ListaMotoristaResponse? Motorista { get; set; }
    [DataMember] public ListaReferenciaResponse Origem { get; set; }
    [DataMember] public ListaReferenciaResponse Destino { get; set; }
    [DataMember] public EntidadeAplicativoResponse Unidade { get; set; }
}
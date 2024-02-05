using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class PernoitesAprovadosResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public DateTime DataPernoite { get; set; }
    [DataMember] public DateTime? DataTratamentoPernoite { get; set; }
    [DataMember] public short NumeroTripulantes { get; set; }
    [DataMember] public string? JustificativaTratamento { get; set; }
    [DataMember] public bool PernoiteProgramada { get; set; }
    [DataMember] public StatusSolicitacaoResponse Status { get; set; }
    [DataMember] public MotivoResponse? Motivo { get; set; }
    [DataMember] public UsuarioAprovadorResponse? UsuarioAprovador { get; set; }
}
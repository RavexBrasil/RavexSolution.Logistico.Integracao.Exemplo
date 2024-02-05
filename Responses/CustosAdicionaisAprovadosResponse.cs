using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class CustosAdicionaisAprovadosResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public bool Previsto { get; set; }
    [DataMember] public decimal? ValorAprovado { get; set; }
    [DataMember] public string? UrlImagem { get; set; }
    [DataMember] public MotivoCustoAdicionalResponse? Motivo { get; set; }
    [DataMember] public StatusSolicitacaoResponse Status { get; set; }
    [DataMember] public ModalidadeResponse? Modalidade { get; set; }
    [DataMember] public UsuarioAprovadorResponse? UsuarioAprovador { get; set; }
}
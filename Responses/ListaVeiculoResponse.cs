using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ListaVeiculoResponse
{
    [DataMember] public int? Id { get; set; }
    [DataMember] public string? Placa { get; set; }
}
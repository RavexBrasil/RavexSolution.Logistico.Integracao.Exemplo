using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ListaMotoristaResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string? Nome { get; set; }
    [DataMember] public string? Cpf { get; set; }
}
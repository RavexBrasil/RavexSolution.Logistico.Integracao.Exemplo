using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ObterIdResponse
{
    [DataMember] public int Id { get; set; }
}
using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class EntidadeAplicativoResponse
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string Nome { get; set; }
}
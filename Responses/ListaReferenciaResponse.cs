using System.Runtime.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

[DataContract]
public class ListaReferenciaResponse
{
    [DataMember] public long Id { get; set; }
    [DataMember] public string Codigo { get; set; }
    [DataMember] public string Nome { get; set; }
    [DataMember] public string RazaoSocial { get; set; }
}
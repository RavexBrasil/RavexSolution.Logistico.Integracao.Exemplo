using System.Text.Json.Serialization;

namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

public record TokenResponse([property: JsonPropertyName("access_token")]
    string AccessToken);
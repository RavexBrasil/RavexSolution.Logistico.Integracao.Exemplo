namespace RavexSolution.Logistico.Integracao.Exemplo.Responses;

public class TokenRequest
{
    public string username { get; init; }
    public string password { get; init; }
    public string grant_type { get; } = "password";
}
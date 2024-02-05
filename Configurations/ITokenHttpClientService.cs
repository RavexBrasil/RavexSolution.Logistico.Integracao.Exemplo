using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Configurations;

public interface ITokenHttpClientService
{
    [Post("/usuario/autenticar")]
    public Task<TokenResponse> Token([Body(BodySerializationMethod.UrlEncoded)] TokenRequest pRequest);
}
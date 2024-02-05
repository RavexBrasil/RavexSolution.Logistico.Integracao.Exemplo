using System.Net;
using System.Net.Http.Headers;
using RavexSolution.Logistico.Integracao.Exemplo.Services;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Handlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenHttpClientService _tokenHttpClientService;
    private static AuthenticationHeaderValue? _authorization;
    private readonly ILogger<ITokenHttpClientService> _logger;
    private readonly Configuracoes _configuracao;

    public AuthHeaderHandler(ILogger<ITokenHttpClientService> logger
        , Configuracoes configuracao
        , ITokenHttpClientService tokenHttpClientService)
    {
        _logger = logger;
        _configuracao = configuracao;
        _tokenHttpClientService = tokenHttpClientService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage pRequestMessage,
        CancellationToken pCancellationToken)
    {
        _authorization ??= await CarregandoToken();

        pRequestMessage.Headers.Authorization  = _authorization;
        var xResult = await base.SendAsync(pRequestMessage, pCancellationToken).ConfigureAwait(false);
        if (xResult.StatusCode is not HttpStatusCode.Unauthorized)
            return xResult;

        _authorization = await CarregandoToken();
        xResult = await SendAsync(pRequestMessage, pCancellationToken).ConfigureAwait(false);

        return xResult;
    }

    private async Task<AuthenticationHeaderValue> CarregandoToken()
    {
        _logger.LogDebug("Carregando token do logístico");

        var xToken = "";
        try
        {
            var xRequest = new TokenRequest {username = _configuracao.Usuario, password = _configuracao.Senha};
            xToken = (await _tokenHttpClientService.Token(xRequest)).AccessToken;
        }
        catch (ApiException xException)
        {
            _logger.LogCritical("Usuário ou Senha incorretos");
            _logger.LogCritical("Matando processo por falha na autenticação, verifique o usuário e senha");
            _logger.LogError(xException, "{Mensagem}", xException.Message);
            Environment.Exit(-1);
        }

        var xAuthorization = new AuthenticationHeaderValue("Bearer", xToken);
        return xAuthorization;
    }
}
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoPontoReferenciaRepository
{
    private readonly IntegracaoContext _integracaoContext;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<IntegracaoPontoReferenciaRepository> _logger;

    public IntegracaoPontoReferenciaRepository(
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoPontoReferenciaRepository> logger, Configuracoes configuracoes)
    {
        _integracaoContext = integracaoContext;
        _logger = logger;
        _configuracoes = configuracoes;
    }

    public IEnumerable<FilaPontoReferencia> ObterReferenciasAIntegrar(int pConfiguracoesQuantidadePontos)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaPontoReferencia
                .Where(p =>
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas
                    && !p.ProcessadoComSucesso
                    && !p.ProcessadoComFalha)
                .OrderBy(p => p.Id)
                .Take(pConfiguracoesQuantidadePontos);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException.Message);
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _integracaoContext.SaveChangesAsync();
    }
}
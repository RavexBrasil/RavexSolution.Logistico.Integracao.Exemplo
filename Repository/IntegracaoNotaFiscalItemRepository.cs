using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoNotaFiscalItemRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoNotaFiscalItemRepository> _logger;

    public IntegracaoNotaFiscalItemRepository(
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoNotaFiscalItemRepository> logger,
        Configuracoes configuracoes)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _integracaoContext = integracaoContext;
    }

    public IEnumerable<FilaNotaFiscalItem> ObterNotaFiscalItemAProcessar(int pFilaNotaFiscalId)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaNotaFiscalItem
                .Where(p =>
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas
                    && p.FilaNotaFiscalId == pFilaNotaFiscalId
                    && !p.ProcessadoComSucesso
                    && !p.ProcessadoComFalha)
                .OrderBy(p => p.Id);
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
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoNotaFiscalRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoNotaFiscalRepository> _logger;


    public IntegracaoNotaFiscalRepository(
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoNotaFiscalRepository> logger,
        Configuracoes configuracoes)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _integracaoContext = integracaoContext;
    }

    public IEnumerable<FilaNotaFiscal> ObterNotaFiscalAProcessar(int pFilaEntregaId)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaNotaFiscal
                .Where(p => 
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas
                    && p.FilaEntregaId == pFilaEntregaId
                    && !p.ProcessadoComFalha
                    && !p.ProcessadoComSucesso)
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
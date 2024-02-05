using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoEntregasRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoEntregasRepository> _logger;

    public IntegracaoEntregasRepository(IntegracaoContext integracaoContext, Configuracoes configuracoes,
        ILogger<IntegracaoEntregasRepository> logger)
    {
        _configuracoes = configuracoes;
        _logger = logger;
        _integracaoContext = integracaoContext;
    }

    public IEnumerable<FilaEntregas> ObterEntregasAProcessar(int pFilaViagemId)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaEntregas
                .Where(p => 
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas 
                    && p.FilaViagemId == pFilaViagemId 
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
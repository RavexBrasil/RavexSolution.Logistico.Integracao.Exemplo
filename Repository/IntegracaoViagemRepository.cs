using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoViagemRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoViagemRepository> _logger;

    public IntegracaoViagemRepository(
        Configuracoes configuracoes,
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoViagemRepository> logger)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _integracaoContext = integracaoContext;
    }

    public IEnumerable<FilaViagem> ObterViagensAProcessar(int pConfiguracoesQuantidadeViagens)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaViagem
                .Where(p => 
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas
                    && !p.ProcessadoComSucesso
                    && !p.ProcessadoComFalha)
                .OrderBy(p => p.Id)
                .Take(pConfiguracoesQuantidadeViagens);
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
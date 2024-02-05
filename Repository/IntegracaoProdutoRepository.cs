using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoProdutoRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoProdutoRepository> _logger;

    public IntegracaoProdutoRepository(
        Configuracoes configuracoes,
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoProdutoRepository> logger)
    {
        _integracaoContext = integracaoContext;
        _configuracoes = configuracoes;
        _logger = logger;
    }

    public IEnumerable<FilaProduto> ObterProdutosAProcessar(int pConfiguracoesQuantidadePedidos)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaProduto
                .Where(p => 
                    p.Tentativas <= _configuracoes.NumeroMaximoDeTentativas
                    && !p.ProcessadoComSucesso
                    && !p.ProcessadoComFalha)
                .OrderBy(p => p.Id)
                .Take(pConfiguracoesQuantidadePedidos);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogInformation(xException, "{Mensagem}", xException.Message);
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _integracaoContext.SaveChangesAsync();
    }
}
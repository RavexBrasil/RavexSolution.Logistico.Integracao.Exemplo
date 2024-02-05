using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Repository;

public class IntegracaoPedidoRepository
{
    private readonly Configuracoes _configuracoes;
    private readonly IntegracaoContext _integracaoContext;
    private readonly ILogger<IntegracaoProdutoRepository> _logger;

    public IntegracaoPedidoRepository(
        Configuracoes configuracoes,
        IntegracaoContext integracaoContext,
        ILogger<IntegracaoProdutoRepository> logger)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _integracaoContext = integracaoContext;
    }

    public IEnumerable<FilaPedido> ObterPedidosAProcessar(int pConfiguracoesQuantidadePedidos)
    {
        try
        {
            var xRetorno = _integracaoContext.FilaPedido
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
            _logger.LogError(xException, "{Mensagem}", xException.Message);
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _integracaoContext.SaveChangesAsync();
    }
}
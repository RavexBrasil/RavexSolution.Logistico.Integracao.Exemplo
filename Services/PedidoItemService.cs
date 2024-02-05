using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class PedidoItemService
{
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;
    private readonly IntegracaoPedidoItemRepository _integracaoPedidoItemRepository;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<PedidoService> _logger;

    public PedidoItemService(ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService
        , IntegracaoPedidoItemRepository pIntegracaoPedidoItemRepository
        , ILogger<PedidoService> pLogger, Configuracoes configuracoes)
    {
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _integracaoPedidoItemRepository = pIntegracaoPedidoItemRepository;
        _logger = pLogger;
        _configuracoes = configuracoes;
    }

    public async Task ProcessarItensDoPedido(int pFilaPedidoId, int pPedidoGlokId)
    {
        _logger.LogInformation("[Iniciado, FilaPedidoId: {FilaPedidoId}", pFilaPedidoId);
        var xFilaPedidosItens = ObterPedidosItens(pFilaPedidoId);
        
        foreach (var xFilaPedidoItem in xFilaPedidosItens)
        {
            Response<int>? xRetornoIntegracaoPedidoItem = null;
            var xStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[ItensDoPedido: {FilaPedidoId}]", xFilaPedidoItem.FilaPedidoId);

                var xPedidoItem = await MontarObjetoPedidoItemAIntegrar(xFilaPedidoItem);

                if (xPedidoItem != null)
                {
                    xRetornoIntegracaoPedidoItem =
                        await IntegrarItemPedido(pPedidoGlokId, xPedidoItem, xFilaPedidoItem);   
                }
            }
            catch (ApiException xException)
            {
                if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500 or >= (HttpStatusCode)500)
                {
                    xFilaPedidoItem.Observacao = xException.Message;
                    xFilaPedidoItem.ProcessadoComSucesso = false;
                    xFilaPedidoItem.ProcessadoComFalha = true;
                    xFilaPedidoItem.LidoDataHora = DateTime.UtcNow;
                    _logger.LogError(xException, "{Mensagem}", xException.Content);
                }
            }
            catch (Exception xException)
            {
                xFilaPedidoItem.Observacao = xException.Message;
                xFilaPedidoItem.ProcessadoComSucesso = false;
                xFilaPedidoItem.ProcessadoComFalha = true;
                xFilaPedidoItem.LidoDataHora = DateTime.UtcNow;
                _logger.LogError("{Mensagem}", xException);
            }
            finally
            {
                await SalvarAtualizacoesFila(xFilaPedidoItem, xRetornoIntegracaoPedidoItem?.Data);

                xStopwatch.Stop();
                _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
            }
        }
    }

    private async Task<Response<int>?> IntegrarItemPedido(int pPedidoGlokId, PedidoItem pPedidoItem,
        FilaPedidoItem pFilaPedidoItem)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaPedidoItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostItemPedido(pPedidoGlokId, pPedidoItem);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaPedidoItem.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaPedidoItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaPedidoItem.Tentativas) * 1000;
                            pFilaPedidoItem.Tentativas++;
                            if (pFilaPedidoItem.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
                                break;

                            _logger.LogInformation(
                                $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");


                            await Task.Delay(delayMilliseconds);
                        }

                        break;
                    }
                }
            }
            catch (Exception xException)
            {
                _logger.LogError(xException, "{Mensagem}", xException);
                if (pFilaPedidoItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaPedidoItem.Tentativas) * 1000;
                    pFilaPedidoItem.Tentativas++;
                    if (pFilaPedidoItem.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
                        break;

                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");


                    await Task.Delay(delayMilliseconds);
                }
            }

            if (pFilaPedidoItem.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
            {
                _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
                throw new Exception("Número máximo de tentativas atingido. Desistindo...");
            }
        }

        return null;
    }

    private async Task<PedidoItem?> MontarObjetoPedidoItemAIntegrar(FilaPedidoItem pFilaPedidoItem)
    {
        _logger.LogInformation("[Iniciado]");
        ObterIdResponse? xDataProduto = null;
        try
        {
            xDataProduto =
                (await _sistemaLogisticaHttpClientService.ObterIdProdutoPorCodigo(pFilaPedidoItem.CodigoProduto)).Data;

            _logger.LogInformation("[CodigoProduto: {Codigo}]"
                                   + " [ProdutoId: {RequestProdutoId}]"
                , pFilaPedidoItem.CodigoProduto
                , xDataProduto.Id);

            var xRetorno = new PedidoItem
            {
                Quantidade = pFilaPedidoItem.Quantidade,
                ValorUnitario = pFilaPedidoItem.ValorUnitario,
                PesoBruto = pFilaPedidoItem.PesoBruto,
                PesoLiquido = pFilaPedidoItem.PesoLiquido,
                ProdutoId = xDataProduto.Id
            };

            return xRetorno;
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
            throw;
        }
        catch (ApiException xException)
        {
            if (xDataProduto is null)
                throw new Exception(
                    $"{xException.StatusCode} Produto não encontrado pelo código {pFilaPedidoItem.CodigoProduto}");
        }
        
        return null;
    }

    private List<FilaPedidoItem> ObterPedidosItens(int pFilaPedidoId)
    {
        try
        {
            var xRetorno = _integracaoPedidoItemRepository
                .ObterItensAProcessar(pFilaPedidoId)
                .ToList();
            _logger.LogInformation("[QuantidadeItensAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError("{Mensagem}", xException);
            throw;
        }
    }
    
    private async Task SalvarAtualizacoesFila(FilaPedidoItem pFilaPedidoItem, int? pIdRetornadoIntegracao)
    {
        pFilaPedidoItem.LidoDataHora = DateTime.UtcNow;
        pFilaPedidoItem.Tentativas++;
        pFilaPedidoItem.PedidoItemGlokId = pIdRetornadoIntegracao;

        if (pFilaPedidoItem.PedidoItemGlokId > 0)
        {
            pFilaPedidoItem.ProcessadoComSucesso = true;
            pFilaPedidoItem.Tentativas++;
            _logger.LogInformation("[ProcessadoComSucesso] [ItemId: {Item}]", pFilaPedidoItem.Id);
        }

        try
        {
            await _integracaoPedidoItemRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
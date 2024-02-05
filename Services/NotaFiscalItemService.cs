using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class NotaFiscalItemService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<NotaFiscalItemService> _logger;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;
    private readonly IntegracaoNotaFiscalItemRepository _integracaoNotaFiscalItemRepository;

    public NotaFiscalItemService(
        ILogger<NotaFiscalItemService> pLogger,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService,
        IntegracaoNotaFiscalItemRepository pIntegracaoNotaFiscalItemRepository,
        Configuracoes configuracoes)
    {
        _logger = pLogger;
        _configuracoes = configuracoes;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _integracaoNotaFiscalItemRepository = pIntegracaoNotaFiscalItemRepository;
    }

    public async Task ProcessarItensDaNotaFiscalItem(int pFilaNotaFiscalId,
        int pViagemGlokId, int pEntregaGlokId,
        int pNotaFiscalGlokId)
    {
        _logger.LogInformation("[Iniciado, FilaNotaFiscalId: {FilaNotaFiscalId}]", pFilaNotaFiscalId);
        var xFilaNotaFiscalItens = ObterNotaFiscalItens(pFilaNotaFiscalId);

        foreach (var xFilaNotaFiscalItem in xFilaNotaFiscalItens)
        {
            var xStopwatch = Stopwatch.StartNew();
            Response<int>? xRetornoIntegracaoNotaFiscalItem = null;
            try
            {
                _logger.LogInformation("[NotaFiscalItem: {NotaFiscalItemId}]", xFilaNotaFiscalItem.Id);

                var xNotaFiscalItem = await MontarObjetoNotaFiscalItemAIntegrar(xFilaNotaFiscalItem);

                if (xNotaFiscalItem != null)
                {
                    xRetornoIntegracaoNotaFiscalItem =
                        await IntegrarItemNotaFiscalItem(pViagemGlokId, pEntregaGlokId, pNotaFiscalGlokId,
                            xNotaFiscalItem,
                            xFilaNotaFiscalItem);
                }
            }
            catch (ApiException xException)
            {
                if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500 or >= (HttpStatusCode)500)
                {
                    xFilaNotaFiscalItem.Observacao = xException.Message;
                    xFilaNotaFiscalItem.ProcessadoComSucesso = false;
                    xFilaNotaFiscalItem.ProcessadoComFalha = true;
                    xFilaNotaFiscalItem.LidoDataHora = DateTime.UtcNow;
                    _logger.LogError(xException, "{Mensagem}", xException.Content);
                }
            }
            catch (Exception xException)
            {
                xFilaNotaFiscalItem.Observacao = xException.Message;
                xFilaNotaFiscalItem.ProcessadoComSucesso = false;
                xFilaNotaFiscalItem.ProcessadoComFalha = true;
                xFilaNotaFiscalItem.LidoDataHora = DateTime.UtcNow;
                _logger.LogError("{Mensagem}", xException);
            }
            finally
            {
                await SalvarAtualizacoesFila(xFilaNotaFiscalItem, xRetornoIntegracaoNotaFiscalItem?.Data);

                xStopwatch.Stop();
                _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
            }
        }
    }

    private async Task<Response<int>?> IntegrarItemNotaFiscalItem(int pViagemGlokId, int pEntregaGlokId,
        int pNotaFiscalGlokId, NotaFiscalItem pNotaFiscalItem,
        FilaNotaFiscalItem pFilaNotaFiscalItem)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaNotaFiscalItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostNotaFiscalItem(pViagemGlokId,
                    pEntregaGlokId, pNotaFiscalGlokId, pNotaFiscalItem);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaNotaFiscalItem.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaNotaFiscalItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaNotaFiscalItem.Tentativas) * 1000;
                            pFilaNotaFiscalItem.Tentativas++;
                            if (pFilaNotaFiscalItem.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
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
                if (pFilaNotaFiscalItem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaNotaFiscalItem.Tentativas) * 1000;
                    pFilaNotaFiscalItem.Tentativas++;
                    if (pFilaNotaFiscalItem.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
                        break;

                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");


                    await Task.Delay(delayMilliseconds);
                }
            }
        }

        if (pFilaNotaFiscalItem.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task<NotaFiscalItem?> MontarObjetoNotaFiscalItemAIntegrar(FilaNotaFiscalItem pFilaNotaFiscalItem)
    {
        _logger.LogInformation("[Iniciado]");
        ObterIdResponse? xDataProduto = null;

        try
        {
            if (pFilaNotaFiscalItem.CodigoProduto != null)
                xDataProduto =
                    (await _sistemaLogisticaHttpClientService.ObterIdProdutoPorCodigo(pFilaNotaFiscalItem
                        .CodigoProduto)).Data;

            _logger.LogInformation("[NotaFiscalItemId: {NotaFiscalItemId}]"
                , pFilaNotaFiscalItem.Id);

            var xRetorno = new NotaFiscalItem
            {
                Sequencia = pFilaNotaFiscalItem.Sequencia,
                ValorUnitario = pFilaNotaFiscalItem.ValorUnitario,
                Quantidade = pFilaNotaFiscalItem.Quantidade,
                PesoBruto = pFilaNotaFiscalItem.PesoBruto,
                PesoLiquido = pFilaNotaFiscalItem.PesoLiquido,
                Prioridade = pFilaNotaFiscalItem.Prioridade,
                ProdutoId = xDataProduto?.Id
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
                    $"{xException.StatusCode} Produto não encontrado pelo código {pFilaNotaFiscalItem.CodigoProduto}");
        }

        return null;
    }

    private List<FilaNotaFiscalItem> ObterNotaFiscalItens(int pFilaNotaFiscalId)
    {
        try
        {
            var xRetorno = _integracaoNotaFiscalItemRepository
                .ObterNotaFiscalItemAProcessar(pFilaNotaFiscalId)
                .ToList();
            _logger.LogInformation("[QuantidadeItensAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaNotaFiscalItem pFilaNotaFiscalItem, int? pIdRetornadoIntegracao)
    {
        pFilaNotaFiscalItem.LidoDataHora = DateTime.UtcNow;
        pFilaNotaFiscalItem.Tentativas++;
        pFilaNotaFiscalItem.NotaFiscalItemGlokId = pIdRetornadoIntegracao;

        if (pFilaNotaFiscalItem.NotaFiscalItemGlokId > 0)
        {
            pFilaNotaFiscalItem.ProcessadoComSucesso = true;
            pFilaNotaFiscalItem.Tentativas++;
            _logger.LogInformation("[ProcessadoComSucesso] [ItemId: {Item}]", pFilaNotaFiscalItem.Id);
        }

        try
        {
            await _integracaoNotaFiscalItemRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class PedidoService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<PedidoService> _logger;
    private readonly IntegracaoPedidoRepository _integracaoPedidoRepository;
    private readonly PedidoItemService _pedidoItemService;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public PedidoService(ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService,
        IntegracaoPedidoRepository integracaoPedidoRepository,
        ILogger<PedidoService> pLogger,
        PedidoItemService pedidoItemService,
        Configuracoes configuracoes)
    {
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _integracaoPedidoRepository = integracaoPedidoRepository;
        _logger = pLogger;
        _pedidoItemService = pedidoItemService;
        _configuracoes = configuracoes;
    }

    public async Task ProcessarPedidos(List<FilaPedido> pFilaPedidos)
    {
        _logger.LogInformation("[Iniciado]");
        foreach (var xFilaPedido in pFilaPedidos)
        {
            if (xFilaPedido.PedidoGlokId > 0)
            {
                await _pedidoItemService.ProcessarItensDoPedido(xFilaPedido.Id, (int)xFilaPedido.PedidoGlokId);
            }
            else
            {
                await ProcessarFilaPedido(xFilaPedido);
                if (xFilaPedido.PedidoGlokId > 0)
                {
                    await _pedidoItemService.ProcessarItensDoPedido(xFilaPedido.Id, (int)xFilaPedido.PedidoGlokId);
                }
            }
        }
    }

    private async Task ProcessarFilaPedido(FilaPedido pFilaPedido)
    {
        var xStopwatch = Stopwatch.StartNew();
        Response<int>? xRetornoIntegracaoPedido = null;

        try
        {
            _logger.LogInformation("[PedidoNumero: {Numero}]", pFilaPedido.Numero);

            var xPedido = await MontarPedidoAEntregar(pFilaPedido);

            if (xPedido != null)
            {
                xRetornoIntegracaoPedido = await IntegrarPedido(xPedido, pFilaPedido);
            }
        }
        catch (ApiException xException)
        {
            if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500 or >= (HttpStatusCode)500)
            {
                pFilaPedido.Observacao = xException.Content;
                pFilaPedido.ProcessadoComSucesso = false;
                pFilaPedido.ProcessadoComFalha = true;
                pFilaPedido.LidoDataHora = DateTime.UtcNow;
                _logger.LogError(xException, "{Mensagem}", xException.Content);
            }
        }
        catch (Exception xException)
        {
            pFilaPedido.Observacao = xException.Message;
            pFilaPedido.ProcessadoComSucesso = false;
            pFilaPedido.ProcessadoComFalha = true;
            pFilaPedido.LidoDataHora = DateTime.UtcNow;
            _logger.LogError("{Mensagem}", xException);
        }
        finally
        {
            await SalvarAtualizacoesFila(pFilaPedido, xRetornoIntegracaoPedido?.Data);

            xStopwatch.Stop();
            _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
        }
    }

    private async Task<Response<int>?> IntegrarPedido(Pedido pPedido, FilaPedido pFilaPedido)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaPedido.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostPedido(pPedido);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaPedido.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaPedido.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaPedido.Tentativas) * 1000;
                            pFilaPedido.Tentativas++;
                            if (pFilaPedido.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
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
                if (pFilaPedido.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaPedido.Tentativas) * 1000;
                    pFilaPedido.Tentativas++;
                    if (pFilaPedido.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                        break;
                    
                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");

                    await Task.Delay(delayMilliseconds);
                }
            }
        }
                        
        if (pFilaPedido.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task<Pedido?> MontarPedidoAEntregar(FilaPedido pFilaPedido)
    {
        _logger.LogInformation("[Iniciado]");
        
        ObterIdResponse? xDataReferencia = null;
        ObterIdResponse? xDataUnidade = null;
        try
        {
            xDataReferencia =
                (await _sistemaLogisticaHttpClientService.ObterIdPorCodigoReferencia(pFilaPedido
                    .CodigoPontoReferencia))
                .Data;

            xDataUnidade =
                (await _sistemaLogisticaHttpClientService.ObterIdUnidadePorCnpj(pFilaPedido.CnpjUnidade)).Data;

            _logger.LogInformation("[PedidoNumero: {RequestNumero}]"
                                   + " [UnidadeId: {RequestUnidadeId}]"
                                   + " [ReferenciaId : {RequestPontoReferencia}]"
                , pFilaPedido.Numero
                , xDataUnidade.Id
                , xDataReferencia.Id);

            var xRetorno = new Pedido
            {
                Numero = pFilaPedido.Numero,
                EstimativaEntrega = pFilaPedido.EstimativaEntrega,
                DataPedido = pFilaPedido.DataPedido,
                PesoBruto = pFilaPedido.PesoBruto,
                PesoLiquido = pFilaPedido.PesoLiquido,
                Cubagem = pFilaPedido.Cubagem,
                ValorPedido = pFilaPedido.ValorPedido,
                PontoReferenciaId = xDataReferencia.Id,
                UnidadeId = xDataUnidade.Id
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
            if (xDataUnidade is null)
                throw new Exception(
                    $"{xException.StatusCode} Unidade não encontrado para o CNPJ {pFilaPedido.CnpjUnidade}");

            if (xDataReferencia is null)
                throw new Exception(
                    $"{xException.StatusCode} Ponto de referência não encontrado para o código {pFilaPedido.CodigoPontoReferencia}");
        }

        return null;
    }

    public List<FilaPedido>? ObterPedidos(int pConfiguracoesQuantidadePedidos)
    {
        try
        {
            var xRetorno = _integracaoPedidoRepository.ObterPedidosAProcessar(pConfiguracoesQuantidadePedidos)
                .ToList();
            _logger.LogInformation("[QuantidadePedidosAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            return null;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaPedido pFilaPedidos, int? pIdRetornadoIntegracao)
    {
        pFilaPedidos.LidoDataHora = DateTime.UtcNow;
        pFilaPedidos.Tentativas++;
        pFilaPedidos.PedidoGlokId = pIdRetornadoIntegracao;

        if (pFilaPedidos.PedidoGlokId > 0)
        {
            pFilaPedidos.ProcessadoComSucesso = true;
            pFilaPedidos.Tentativas++;
            _logger.LogInformation("[ProcessadoComSucesso] [PedidoId: {PedidoId}]", pFilaPedidos.PedidoGlokId);
        }

        try
        {
            await _integracaoPedidoRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
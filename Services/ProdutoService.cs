using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class ProdutoService
{
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;
    private readonly IntegracaoProdutoRepository _integracaoProdutoRepository;
    private readonly ILogger<ProdutoService> _logger;
    private readonly Configuracoes _configuracoes;

    public ProdutoService(ILogger<ProdutoService> logger
        , ISistemaLogisticaHttpClientService sistemaLogisticaHttpClientService
        , IntegracaoProdutoRepository integracaoProdutoRepository, Configuracoes configuracoes)
    {
        _sistemaLogisticaHttpClientService = sistemaLogisticaHttpClientService;
        _integracaoProdutoRepository = integracaoProdutoRepository;
        _configuracoes = configuracoes;
        _logger = logger;
    }

    public List<FilaProduto>? ObterProdutosDaFila(int pConfiguracoesQuantidadeProdutos)
    {
        try
        {
            var xRetorno = _integracaoProdutoRepository.ObterProdutosAProcessar(pConfiguracoesQuantidadeProdutos)
                .ToList();
            _logger.LogInformation("[QuantidadeProdutosAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            return null;
        }
    }

    public async Task ProcessarProdutos(List<FilaProduto> pFilaProdutos)
    {
        _logger.LogInformation("[Iniciado]");
        foreach (var xFilaProduto in pFilaProdutos)
        {
            Response<int>? xRetornoIntegracaoProduto = null;

            var xStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[CodigoProduto: {Codigo}]", xFilaProduto.Codigo);

                xRetornoIntegracaoProduto = await IntegrarProduto(xFilaProduto);
            }
            catch (ApiException xException)
            {
                if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500 or >= (HttpStatusCode)500)
                {
                    xFilaProduto.Observacao = xException.Content;
                    xFilaProduto.ProcessadoComSucesso = false;
                    xFilaProduto.ProcessadoComFalha = true;
                    xFilaProduto.LidoDataHora = DateTime.UtcNow;
                    _logger.LogError(xException, "{Mensagem}", xException.Content);
                }
            }
            catch (Exception xException)
            {
                xFilaProduto.Observacao = xException.Message;
                xFilaProduto.ProcessadoComSucesso = false;
                xFilaProduto.ProcessadoComFalha = true;
                xFilaProduto.LidoDataHora = DateTime.UtcNow;
                _logger.LogError("{Mensagem}", xException);
            }
            finally
            {
                await SalvarAtualizacoesFila(xFilaProduto, xRetornoIntegracaoProduto?.Data);

                xStopwatch.Stop();
                _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
            }
        }
    }

    private async Task<Response<int>?> IntegrarProduto(FilaProduto pFilaProduto)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaProduto.Tentativas <= _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostProduto(pFilaProduto);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaProduto.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaProduto.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaProduto.Tentativas) * 1000;
                            pFilaProduto.Tentativas++;
                            if (pFilaProduto.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
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
                if (pFilaProduto.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaProduto.Tentativas) * 1000;
                    pFilaProduto.Tentativas++;
                    if (pFilaProduto.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                        break;

                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");

                    await Task.Delay(delayMilliseconds);
                }
            }
        }

        if (pFilaProduto.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task SalvarAtualizacoesFila(FilaProduto pFilaProduto, int? pIdRetornadoIntegracao)
    {
        pFilaProduto.LidoDataHora = DateTime.UtcNow;
        pFilaProduto.ProdutoGlokId = pIdRetornadoIntegracao;
        pFilaProduto.Tentativas++;
        
        if (pFilaProduto.ProdutoGlokId > 0)
        {
            pFilaProduto.ProcessadoComSucesso = true;
            _logger.LogInformation("[ProcessadoComSucesso] [ViagemId: {ViagemId}]", pFilaProduto.ProdutoGlokId);
        }

        try
        {
            await _integracaoProdutoRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
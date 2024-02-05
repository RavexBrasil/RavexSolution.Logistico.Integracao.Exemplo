using System.Diagnostics;
using Refit;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class NotaFiscalService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<NotaFiscalService> _logger;
    private readonly IntegracaoNotaFiscalRepository _integracaoNotaFiscalRepository;
    private readonly NotaFiscalItemService _notaFiscalItemService;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public NotaFiscalService(
        ILogger<NotaFiscalService> pLogger,
        IntegracaoNotaFiscalRepository integracaoNotaFiscalRepository,
        NotaFiscalItemService notaFiscalItemService,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService, Configuracoes configuracoes)
    {
        _logger = pLogger;
        _integracaoNotaFiscalRepository = integracaoNotaFiscalRepository;
        _notaFiscalItemService = notaFiscalItemService;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _configuracoes = configuracoes;
    }

    public async Task ProcessarNotaFiscal(int pFilaEntregaId, int pViagemGlokId, int pEntregaGlokId)
    {
        _logger.LogInformation("[Iniciado: ObtendoNotasFiscaisEntrega: {EntregaId}]", pEntregaGlokId);

        var xFilaNotasFiscais = ObterNotasFiscais(pFilaEntregaId);

        foreach (var xFilaNotaFiscal in xFilaNotasFiscais)
        {
            if (xFilaNotaFiscal.NotaFiscalGlokId > 0)
            {
                await _notaFiscalItemService.ProcessarItensDaNotaFiscalItem(
                    xFilaNotaFiscal.Id, pViagemGlokId, pEntregaGlokId,
                    (int)xFilaNotaFiscal.NotaFiscalGlokId);
            }
            else
            {
                await ProcessarFilaNotaFiscal(xFilaNotaFiscal, pViagemGlokId, pEntregaGlokId);
                if (xFilaNotaFiscal.NotaFiscalGlokId > 0)
                {
                    await _notaFiscalItemService.ProcessarItensDaNotaFiscalItem(
                        xFilaNotaFiscal.Id, pViagemGlokId, pEntregaGlokId,
                        (int)xFilaNotaFiscal.NotaFiscalGlokId);
                }
            }
        }
    }

    private async Task ProcessarFilaNotaFiscal(FilaNotaFiscal pFilaNotaFiscal, int pViagemGlokId,
        int pEntregaGlokId)
    {
        _logger.LogInformation("[Iniciado]");
        var xStopwatch = Stopwatch.StartNew();
        Response<int>? xRetornoIntegracaoNotaFiscal = null;

        try
        {
            _logger.LogInformation("[NotaFiscalNumero: {Numero}]", pFilaNotaFiscal.Numero);

            xRetornoIntegracaoNotaFiscal = await IntegrarNotaFiscal(pFilaNotaFiscal, pViagemGlokId, pEntregaGlokId);
        }
        catch (ApiException xException)
        {
            if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500)
            {
                pFilaNotaFiscal.Observacao = xException.Content;
                pFilaNotaFiscal.ProcessadoComSucesso = false;
                pFilaNotaFiscal.ProcessadoComFalha = true;
                pFilaNotaFiscal.LidoDataHora = DateTime.UtcNow;
                _logger.LogError("{Mensagem}", xException.Content);
            }
        }
        catch (Exception xException)
        {
            pFilaNotaFiscal.Observacao = xException.Message;
            pFilaNotaFiscal.ProcessadoComSucesso = false;
            pFilaNotaFiscal.ProcessadoComFalha = true;
            pFilaNotaFiscal.LidoDataHora = DateTime.UtcNow;
            _logger.LogError("{Mensagem}", xException);
        }
        finally
        {
            await SalvarAtualizacoesFila(pFilaNotaFiscal, xRetornoIntegracaoNotaFiscal?.Data);

            xStopwatch.Stop();
            _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
        }
    }

    private async Task<Response<int>?> IntegrarNotaFiscal(FilaNotaFiscal pFilaNotaFiscal, int pViagemGlokId,
        int pEntregaGlokId)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaNotaFiscal.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                var xRetorno = new NotaFiscal
                {
                    Sequencia = pFilaNotaFiscal.Sequencia,
                    ViagemId = pViagemGlokId,
                    EntregaId = pEntregaGlokId,
                    Numero = pFilaNotaFiscal.Numero,
                    Serie = pFilaNotaFiscal.Serie,
                    TipoOperacao = pFilaNotaFiscal.TipoOperacao,
                    PesoBruto = pFilaNotaFiscal.PesoBruto,
                    PesoLiquido = pFilaNotaFiscal.PesoLiquido,
                    Cubagem = pFilaNotaFiscal.Cubagem,
                    Valor = pFilaNotaFiscal.Valor,
                    QuantidadeCaixas = pFilaNotaFiscal.QuantidadeCaixas,
                    CriadoDataHora = DateTime.UtcNow,
                    QuantidadeEstimadaItens = pFilaNotaFiscal.QuantidadeEstimadaItens
                };

                return await _sistemaLogisticaHttpClientService.PostNotaFiscal(xRetorno.ViagemId
                    , xRetorno.EntregaId
                    , xRetorno);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaNotaFiscal.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaNotaFiscal.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaNotaFiscal.Tentativas) * 1000;
                            pFilaNotaFiscal.Tentativas++;
                            if (pFilaNotaFiscal.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                                break;
                            
                            _logger.LogInformation(
                                $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");
                    
                    
                            await Task.Delay(delayMilliseconds);
                        }
                    }

                        break;
                }
            }
            catch (Exception xException)
            {
                _logger.LogError(xException, "{Mensagem}", xException);
                if (pFilaNotaFiscal.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaNotaFiscal.Tentativas) * 1000;
                    pFilaNotaFiscal.Tentativas++;
                    if (pFilaNotaFiscal.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                        break;
                    
                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");
                    
                    
                    await Task.Delay(delayMilliseconds);
                }
            }
        }
                
        if (pFilaNotaFiscal.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private List<FilaNotaFiscal> ObterNotasFiscais(int pFilaEntregaId)
    {
        try
        {
            var xRetorno = _integracaoNotaFiscalRepository
                .ObterNotaFiscalAProcessar(pFilaEntregaId)
                .ToList();
            _logger.LogInformation("[QuantidadeNotaFiscalAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaNotaFiscal pFilaNotaFiscal, int? pIdRetornadoIntegracao)
    {
        pFilaNotaFiscal.LidoDataHora = DateTime.UtcNow;
        pFilaNotaFiscal.Tentativas++;
        pFilaNotaFiscal.NotaFiscalGlokId = pIdRetornadoIntegracao;

        if (pFilaNotaFiscal.NotaFiscalGlokId > 0)
        {
            pFilaNotaFiscal.ProcessadoComSucesso = true;
            pFilaNotaFiscal.Tentativas++;
            
            _logger.LogInformation("[ProcessadoComSucesso] [NotaFiscalId: {NotaFiscalId}]",
                pFilaNotaFiscal.NotaFiscalGlokId);
        }

        try
        {
            await _integracaoNotaFiscalRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
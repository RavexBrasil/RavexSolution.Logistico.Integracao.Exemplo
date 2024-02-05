using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class EntregasService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<EntregasService> _logger;
    private readonly NotaFiscalService _notaFiscalService;
    private readonly IntegracaoEntregasRepository _integracaoEntregasRepository;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public EntregasService(
        ILogger<EntregasService> pLogger,
        NotaFiscalService notaFiscalService,
        IntegracaoEntregasRepository pIntegracaoEntregasRepository,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService, Configuracoes configuracoes)
    {
        _logger = pLogger;
        _notaFiscalService = notaFiscalService;
        _integracaoEntregasRepository = pIntegracaoEntregasRepository;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _configuracoes = configuracoes;
    }

    public async Task ProcessarEntregas(int pFilaViagemId, int pViagemGlokId)
    {
        _logger.LogInformation("[Iniciado: ObtendoEntregasViagem: {Id}]", pFilaViagemId);

        var xFilaEntregas = ObterEntregas(pFilaViagemId);

        foreach (var xFilaEntrega in xFilaEntregas)
        {
            if (xFilaEntrega.EntregaGlokId > 0)
            {
                await _notaFiscalService.ProcessarNotaFiscal(xFilaEntrega.Id, pViagemGlokId,
                    (int)xFilaEntrega.EntregaGlokId);
            }
            else
            {
                await ProcessarFilaEntregas(xFilaEntrega, pViagemGlokId);
                if (xFilaEntrega.EntregaGlokId is not null && xFilaEntrega.EntregaGlokId > 0)
                    await _notaFiscalService.ProcessarNotaFiscal(xFilaEntrega.Id, pViagemGlokId,
                        (int)xFilaEntrega.EntregaGlokId);
            }
        }
    }

    private async Task ProcessarFilaEntregas(FilaEntregas pFilaEntregas, int pViagemGlokId)
    {
        _logger.LogInformation("[Iniciado]");
        var xStopwatch = Stopwatch.StartNew();
        Response<int>? xRetornoIntegracaoEntregas = null;
        try
        {
            _logger.LogInformation("[EntregasDaViagem: {FilaViagemId}]", pFilaEntregas.FilaViagemId);

            var xEntregas = await MontarObjetoEntregaAIntegrar(pFilaEntregas, pViagemGlokId);

            if (xEntregas != null)
            {
                xRetornoIntegracaoEntregas = await IntegrarEntrega(xEntregas, pFilaEntregas);
            }
        }
        catch (ApiException xException)
        {
            if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500)
            {
                pFilaEntregas.Observacao = xException.Content;
                pFilaEntregas.ProcessadoComSucesso = false;
                pFilaEntregas.ProcessadoComFalha = true;
                pFilaEntregas.LidoDataHora = DateTime.UtcNow;
                _logger.LogError("{Mensagem}", xException.Content);
            }
        }
        catch (Exception xException)
        {
            pFilaEntregas.Observacao = xException.Message;
            pFilaEntregas.ProcessadoComSucesso = false;
            pFilaEntregas.ProcessadoComFalha = true;
            pFilaEntregas.LidoDataHora = DateTime.UtcNow;
            _logger.LogError("{Mensagem}", xException);
        }
        finally
        {
            await SalvarAtualizacoesFila(pFilaEntregas, xRetornoIntegracaoEntregas?.Data);

            xStopwatch.Stop();
            _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
        }
    }

    private async Task<Response<int>?> IntegrarEntrega(Entregas pEntregas, FilaEntregas pFilaEntregas)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaEntregas.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostEntrega(pEntregas.ViagemId, pEntregas);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaEntregas.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaEntregas.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaEntregas.Tentativas) * 1000;
                            pFilaEntregas.Tentativas++;
                            if (pFilaEntregas.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
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
                if (pFilaEntregas.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaEntregas.Tentativas) * 1000;
                    pFilaEntregas.Tentativas++;
                    if (pFilaEntregas.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                        break;
                    
                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");
                    
                    
                    await Task.Delay(delayMilliseconds);
                }
            }
        }

        if (pFilaEntregas.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task<Entregas?> MontarObjetoEntregaAIntegrar(FilaEntregas pFilaEntregas, int pViagemGlokId)
    {
        _logger.LogInformation("[Iniciado]");
        ObterIdResponse? xDataReferencia = null;

        try
        {
            xDataReferencia =
                (await _sistemaLogisticaHttpClientService
                    .ObterIdPorCodigoReferencia(pFilaEntregas.CodigoReferencia))
                .Data;

            _logger.LogInformation("[ReferenciaId: {Referencia}]", xDataReferencia.Id);

            var xRetorno = new Entregas
            {
                Sequencia = pFilaEntregas.Sequencia,
                QuantidadeEstimadaNotasFiscais = pFilaEntregas.QuantidadeEstimadaNotasFiscais,
                Latitude = pFilaEntregas.Latitude,
                Longitude = pFilaEntregas.Longitude,
                PesoBruto = pFilaEntregas.PesoBruto,
                PesoLiquido = pFilaEntregas.PesoLiquido,
                Cubagem = pFilaEntregas.Cubagem,
                PontoReferenciaId = xDataReferencia.Id,
                ViagemId = pViagemGlokId
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
            if (xDataReferencia is null)
                throw new Exception(
                    $"{xException.StatusCode} Referência não encontrada para o código {pFilaEntregas.CodigoReferencia}");
        }

        return null;
    }

    private List<FilaEntregas> ObterEntregas(int pFilaViagemId)
    {
        try
        {
            var xRetorno = _integracaoEntregasRepository
                .ObterEntregasAProcessar(pFilaViagemId)
                .ToList();
            _logger.LogInformation("[QuantidadeEntregasAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaEntregas pFilaEntregas, int? pIdRetornadoIntegracao)
    {
        pFilaEntregas.LidoDataHora = DateTime.UtcNow;
        pFilaEntregas.Tentativas++;
        pFilaEntregas.EntregaGlokId = pIdRetornadoIntegracao;

        if (pFilaEntregas.EntregaGlokId > 0)
        {
            pFilaEntregas.ProcessadoComSucesso = true;
            pFilaEntregas.Tentativas++;
            _logger.LogInformation("[ProcessadoComSucesso] [EntregaId: {Entrega}]", pFilaEntregas.EntregaGlokId);
        }

        try
        {
            await _integracaoEntregasRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
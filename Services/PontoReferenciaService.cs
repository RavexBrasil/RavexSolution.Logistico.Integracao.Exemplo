using System.Diagnostics;
using System.Net;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class PontoReferenciaService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<PontoReferenciaService> _logger;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;
    private readonly IntegracaoPontoReferenciaRepository _integracaoPontoReferenciaRepository;

    public PontoReferenciaService(
        Configuracoes configuracoes
        , ILogger<PontoReferenciaService> logger
        , ISistemaLogisticaHttpClientService sistemaLogisticaHttpClientService
        , IntegracaoPontoReferenciaRepository integracaoPontoReferenciaRepository)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _sistemaLogisticaHttpClientService = sistemaLogisticaHttpClientService;
        _integracaoPontoReferenciaRepository = integracaoPontoReferenciaRepository;
    }

    public async Task ProcessarItens(List<FilaPontoReferencia> pFilaPontoReferencia)
    {
        _logger.LogInformation("[Iniciado]");

        foreach (var xFilaPontoReferencia in pFilaPontoReferencia)
        {
            Response<int>? xRetornoIntegracaoPontoReferencia = null;
            var xStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[CodigoPontoReferencia: {Codigo}]", xFilaPontoReferencia.Codigo);

                var xPontoReferencia = await MontarObjetoReferenciaAIntegrar(xFilaPontoReferencia);

                if (xPontoReferencia != null)
                {
                    xRetornoIntegracaoPontoReferencia =
                        await IntegrarPontoReferencia(xPontoReferencia, xFilaPontoReferencia);
                }
            }
            catch (ApiException xException)
            {
                if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500 or >= (HttpStatusCode)500)
                {
                    xFilaPontoReferencia.LidoDataHora = DateTime.UtcNow;
                    xFilaPontoReferencia.ProcessadoComSucesso = false;
                    xFilaPontoReferencia.ProcessadoComFalha = true;
                    xFilaPontoReferencia.Observacao = xException.Content;
                    _logger.LogError(xException, "{Mensagem}", xException.Content);
                }
            }
            catch (Exception xException)
            {
                xFilaPontoReferencia.LidoDataHora = DateTime.UtcNow;
                xFilaPontoReferencia.ProcessadoComSucesso = false;
                xFilaPontoReferencia.ProcessadoComFalha = true;
                xFilaPontoReferencia.Observacao = xException.Message;
                _logger.LogError("{Mensagem}", xException);
            }
            finally
            {
                await SalvarAtualizacoesFila(xFilaPontoReferencia, xRetornoIntegracaoPontoReferencia?.Data);
                xStopwatch.Stop();
                _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
            }
        }
    }

    private async Task<Response<int>?> IntegrarPontoReferencia(PontoReferencia pPontoReferencia,
        FilaPontoReferencia pFilaPontoReferencia)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaPontoReferencia.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostPontoReferencia(pPontoReferencia);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaPontoReferencia.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaPontoReferencia.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaPontoReferencia.Tentativas) * 1000;
                            pFilaPontoReferencia.Tentativas++;
                            if (pFilaPontoReferencia.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
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
                if (pFilaPontoReferencia.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaPontoReferencia.Tentativas) * 1000;
                    pFilaPontoReferencia.Tentativas++;
                    if (pFilaPontoReferencia.Tentativas == _configuracoes.NumeroMaximoDeTentativas)
                        break;

                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");

                    await Task.Delay(delayMilliseconds);
                }
            }
        }

        if (pFilaPontoReferencia.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task<PontoReferencia?> MontarObjetoReferenciaAIntegrar(FilaPontoReferencia pFilaPontoReferencia)
    {
        _logger.LogInformation("[Iniciado]");
        ObterIdResponse? xDataGrupoReferencia = null;

        try
        {
            xDataGrupoReferencia =
                (await _sistemaLogisticaHttpClientService.ObterIdPorNome(pFilaPontoReferencia.NomeGrupoReferencia))
                .Data;

            _logger.LogInformation("[CodigoPontoReferencia: {RequestCodigoPontoReferencia}]"
                                   + " [GrupoReferencia: {RequestGrupoReferencia}]"
                , pFilaPontoReferencia.Codigo
                , xDataGrupoReferencia.Id);

            var xRetorno = new PontoReferencia
            {
                Codigo = pFilaPontoReferencia.Codigo,
                TipoPessoa = pFilaPontoReferencia.TipoPessoa,
                Nome = pFilaPontoReferencia.Nome,
                Latitude = pFilaPontoReferencia.Latitude,
                Longitude = pFilaPontoReferencia.Longitude,
                GrupoReferenciaId = xDataGrupoReferencia.Id
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
            if (xDataGrupoReferencia is null)
                throw new Exception(
                    $"{xException.StatusCode} Grupo de referencia não encontrado para o nome {pFilaPontoReferencia.NomeGrupoReferencia}");
        }

        return null;
    }

    public List<FilaPontoReferencia>? ObterPontosReferencia(int pConfiguracoesQuantidadePontoReferencia)
    {
        try
        {
            var xRetorno = _integracaoPontoReferenciaRepository
                .ObterReferenciasAIntegrar(pConfiguracoesQuantidadePontoReferencia)
                .ToList();
            _logger.LogInformation("[QuantidadePontoReferenciaAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError("{Mensagem}", xException);
            return null;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaPontoReferencia pFilaPontoReferencia, int? pIdRetornadoIntegracao)
    {
        pFilaPontoReferencia.LidoDataHora = DateTime.UtcNow;
        pFilaPontoReferencia.Tentativas++;
        pFilaPontoReferencia.PontoReferenciaGlokId = pIdRetornadoIntegracao;

        if (pFilaPontoReferencia.PontoReferenciaGlokId > 0)
        {
            pFilaPontoReferencia.ProcessadoComSucesso = true;
            pFilaPontoReferencia.Tentativas++;
            _logger.LogInformation("[ProcessadoComSucesso] [PontoReferenciaId: {PontoReferenciaId}]"
                , pFilaPontoReferencia.PontoReferenciaGlokId);
        }

        try
        {
            await _integracaoPontoReferenciaRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }
}
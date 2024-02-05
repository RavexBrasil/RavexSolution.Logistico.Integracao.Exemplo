using Refit;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class ViagemService
{
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<ViagemService> _logger;
    private readonly PernoitesService _pernoitesService;
    private readonly AnomaliasService _anomaliasService;
    private readonly EntregasService _entregaRepository;
    private readonly CustoAdicionalService _custoAdicionalService;
    private readonly IntegracaoViagemRepository _integracaoViagemRepository;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public ViagemService(
        ILogger<ViagemService> pLogger,
        IntegracaoViagemRepository integracaoViagemRepository,
        EntregasService entregaRepository,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService,
        CustoAdicionalService custoAdicionalService,
        PernoitesService pernoitesService,
        AnomaliasService anomaliasService, Configuracoes configuracoes)
    {
        _logger = pLogger;
        _integracaoViagemRepository = integracaoViagemRepository;
        _entregaRepository = entregaRepository;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
        _custoAdicionalService = custoAdicionalService;
        _pernoitesService = pernoitesService;
        _anomaliasService = anomaliasService;
        _configuracoes = configuracoes;
    }

    public async Task ProcessarViagens(List<FilaViagem> pFilaViagens)
    {
        _logger.LogInformation("[Iniciado]");
        foreach (var xFilaViagem in pFilaViagens)
        {
            if (xFilaViagem.ViagemGlokId > 0)
            {
                await _entregaRepository.ProcessarEntregas(xFilaViagem.Id, (int)xFilaViagem.ViagemGlokId);
            }
            else
            {
                await ProcessarFilaViagens(xFilaViagem);
                if (xFilaViagem.ViagemGlokId > 0)
                {
                    await _entregaRepository.ProcessarEntregas(xFilaViagem.Id, (int)xFilaViagem.ViagemGlokId);
                }
            }
        }
    }

    private async Task ProcessarFilaViagens(FilaViagem pFilaViagens)
    {
        var xStopwatch = Stopwatch.StartNew();
        Response<int>? xRetornoIntegracaoViagem = null;

        try
        {
            _logger.LogInformation("[ViagemIdentificador: {Identificador}]", pFilaViagens.Identificador);

            var xViagem = await MontarObjetoViagemAIntegrar(pFilaViagens);

            if (xViagem != null)
            {
                xRetornoIntegracaoViagem = await IntegrarViagem(xViagem, pFilaViagens);
            }
        }
        catch (ApiException xException)
        {
            if (xException.StatusCode is >= (HttpStatusCode)400 and < (HttpStatusCode)500)
            {
                pFilaViagens.Observacao = xException.Content;
                pFilaViagens.ProcessadoComSucesso = false;
                pFilaViagens.ProcessadoComFalha = true;
                pFilaViagens.LidoDataHora = DateTime.UtcNow;
                _logger.LogError(xException, "{Mensagem}", xException.Content);
            }
        }
        catch (Exception xException)
        {
            pFilaViagens.Observacao = xException.Message;
            pFilaViagens.ProcessadoComSucesso = false;
            pFilaViagens.ProcessadoComFalha = true;
            pFilaViagens.LidoDataHora = DateTime.UtcNow;
            _logger.LogError(xException, "{Mensagem}", xException);
        }
        finally
        {
            await SalvarAtualizacoesFila(pFilaViagens, xRetornoIntegracaoViagem?.Data);

            xStopwatch.Stop();
            _logger.LogInformation("[Finalizado] [Tempo: {xStopwatchElapsed}]", xStopwatch.Elapsed);
        }
    }

    private async Task<Response<int>?> IntegrarViagem(Viagem pViagem, FilaViagem pFilaViagem)
    {
        _logger.LogInformation("[Iniciado]");

        while (pFilaViagem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
        {
            try
            {
                return await _sistemaLogisticaHttpClientService.PostViagem(pViagem);
            }
            catch (ApiException xException)
            {
                switch (xException.StatusCode)
                {
                    case >= (HttpStatusCode)400 and < (HttpStatusCode)500:
                        pFilaViagem.Tentativas++;
                        throw;
                    case >= (HttpStatusCode)500:
                    {
                        if (pFilaViagem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                        {
                            var delayMilliseconds = (int)Math.Pow(2, pFilaViagem.Tentativas) * 1000;
                            pFilaViagem.Tentativas++;
                            if (pFilaViagem.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
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
                if (pFilaViagem.Tentativas < _configuracoes.NumeroMaximoDeTentativas)
                {
                    var delayMilliseconds = (int)Math.Pow(2, pFilaViagem.Tentativas) * 1000;
                    pFilaViagem.Tentativas++;
                    if (pFilaViagem.Tentativas == _configuracoes.NumeroMaximoDeTentativas) 
                        break;
                    
                    _logger.LogInformation(
                        $"Aguardando {delayMilliseconds} milissegundos antes de tentar novamente...");

                    await Task.Delay(delayMilliseconds);
                }
            }
        }
                
        if (pFilaViagem.Tentativas >= _configuracoes.NumeroMaximoDeTentativas)
        {
            _logger.LogInformation("Número máximo de tentativas atingido. Desistindo...");
            throw new Exception("Número máximo de tentativas atingido. Desistindo...");
        }

        return null;
    }

    private async Task<Viagem?> MontarObjetoViagemAIntegrar(FilaViagem pFilaViagem)
    {
        _logger.LogInformation("[Iniciado]");

        ObterIdResponse? xDataEmbarcador = null;
        ObterIdResponse? xDataUnidade = null;
        ObterIdResponse? xDataCooperativa = null;
        ObterIdResponse? xDataTransportadora = null;

        try
        {
            xDataEmbarcador =
                (await _sistemaLogisticaHttpClientService.ObterIdEmbarcadorPorCnpj(pFilaViagem.CnpjEmbarcador))
                .Data;

            xDataUnidade = (await _sistemaLogisticaHttpClientService.ObterIdUnidadePorCnpj(pFilaViagem.CnpjUnidade))
                .Data;

            xDataCooperativa =
                (await _sistemaLogisticaHttpClientService.ObterIdCooperativaPorCnpj(pFilaViagem.CnpjCooperativa))
                .Data;

            xDataTransportadora =
                (await _sistemaLogisticaHttpClientService.ObterIdTransportadoraPorCnpj(pFilaViagem
                    .CnpjTransportadora))
                .Data;

            _logger.LogInformation("[ViagemIdentificador: {RequestIdentificador}]"
                                   + " [EmbarcadorId: {RequestEmbarcador}]"
                                   + " [UnidadeId : {RequestUnidade}]"
                                   + " [CooperativaId : {RequestCooperativa}]"
                                   + " [TransportadoraId : {RequestTransportadora}]"
                , pFilaViagem.Identificador
                , xDataEmbarcador.Id
                , xDataUnidade.Id
                , xDataCooperativa.Id
                , xDataTransportadora.Id
            );

            var xRetorno = new Viagem
            {
                Identificador = pFilaViagem.Identificador,
                EstimativaInicio = pFilaViagem.EstimativaInicio,
                EstimativaFim = pFilaViagem.EstimativaFim,
                Tipo = pFilaViagem.Tipo,
                PesoBrutoTotal = pFilaViagem.PesoBrutoTotal,
                PesoLiquidoTotal = pFilaViagem.PesoLiquidoTotal,
                Valor = pFilaViagem.Valor,
                KmEstimado = pFilaViagem.KmEstimado,
                QuantidadeCaixas = pFilaViagem.QuantidadeCaixas,
                PossuiOrdemEspecial = pFilaViagem.PossuiOrdemEspecial,
                ComputarIndicador = pFilaViagem.ComputarIndicador,
                EmbarcadorId = xDataEmbarcador.Id,
                UnidadeId = xDataUnidade.Id,
                CooperativaId = xDataCooperativa.Id,
                TransportadoraId = xDataTransportadora.Id
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
            if (xDataEmbarcador is null)
                throw new BadHttpRequestException(
                    $"{xException.StatusCode} Embarcador não encontrado para o CNPJ {pFilaViagem.CnpjEmbarcador}");

            if (xDataUnidade is null)
                throw new BadHttpRequestException(
                    $"{xException.StatusCode} Unidade não encontrado para o CNPJ {pFilaViagem.CnpjUnidade}");

            if (xDataCooperativa is null)
                throw new BadHttpRequestException(
                    $"{xException.StatusCode} Cooperativa não encontrado para o CNPJ {pFilaViagem.CnpjCooperativa}");

            if (xDataTransportadora is null)
                throw new BadHttpRequestException(
                    $"{xException.StatusCode} Transportadora não encontrado para o CNPJ {pFilaViagem.CnpjTransportadora}");
        }

        return null;
    }

    public List<FilaViagem> ObterViagens(int pConfiguracoesQuantidadeViagens)
    {
        try
        {
            var xRetorno = _integracaoViagemRepository.ObterViagensAProcessar(pConfiguracoesQuantidadeViagens)
                .ToList();
            _logger.LogInformation("[QuantidadeViagensAProcessar]: {RetornoCount}", xRetorno.Count);
            return xRetorno;
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }

    private async Task SalvarAtualizacoesFila(FilaViagem pFilaViagens, int? pIdRetornadoIntegracao)
    {
        pFilaViagens.LidoDataHora = DateTime.UtcNow;
        pFilaViagens.Tentativas++;
        pFilaViagens.ViagemGlokId = pIdRetornadoIntegracao;

        if (pFilaViagens.ViagemGlokId > 0)
        {
            pFilaViagens.ProcessadoComSucesso = true;
            
            _logger.LogInformation("[ProcessadoComSucesso] [ViagemId: {ViagemId}]", pFilaViagens.ViagemGlokId);
        }

        try
        {
            await _integracaoViagemRepository.SaveChangesAsync();
        }
        catch (Exception xException)
        {
            _logger.LogError(xException, "{Mensagem}", xException);
            throw;
        }
    }

    public async Task<IEnumerable<ViagensFinalizadasResponse>?> ObterViagensFinalizadas()
    {
        _logger.LogInformation("[Buscando viagens...]");
        IEnumerable<ViagensFinalizadasResponse>? xDataViagem = null;
        try
        {
            xDataViagem =
                (await _sistemaLogisticaHttpClientService.ObterListaDeViagensFinalizadasPorPeriodo(
                    DateTime.Now.AddHours(-1),
                    DateTime.Now)).Data;
            if (xDataViagem != null)
            {
                var xViagensFinalizadasResponses = xDataViagem.ToList();
                if (xViagensFinalizadasResponses.Any())
                {
                    foreach (var xViagem in xViagensFinalizadasResponses)
                    {
                        if (xViagem.PossuiPernoite)
                            xViagem.Pernoites = await _pernoitesService.ObterPernoites(xViagem.Id);

                        if (xViagem.PossuiAnomalia)
                            xViagem.Anomalias = await _anomaliasService.ObterAnomalias(xViagem.Id);

                        if (xViagem.PossuiCustoAdicional)
                            xViagem.CustosAdicionais = await _custoAdicionalService.ObterCustosAdicionais(xViagem.Id);

                        _logger.LogInformation(
                            "[Viagem: {Viagem},",
                            JsonConvert.SerializeObject(xViagem));
                    }

                    return xViagensFinalizadasResponses;
                }
            }

            _logger.LogInformation("[Nenhuma viagem encontrada.]");
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
        }
        catch (ApiException xException)
        {
            if (xDataViagem is null)
                throw new Exception(
                    $"{xException.StatusCode} Nenhuma viagem encontrada.");
        }

        return null;
    }
}
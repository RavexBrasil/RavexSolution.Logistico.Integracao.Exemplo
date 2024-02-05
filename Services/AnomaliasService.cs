using Newtonsoft.Json;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class AnomaliasService
{
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;
    private readonly ILogger<AnomaliasService> _logger;

    public AnomaliasService(
        ILogger<AnomaliasService> pLogger,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService)
    {
        _logger = pLogger;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
    }

    public async Task<IEnumerable<AnomaliaRegistradaResponseV2>?> ObterAnomalias(int pViagemId)
    {
        _logger.LogInformation("[Buscando anomalias...]");
        try
        {
            var xAnomalias = (await _sistemaLogisticaHttpClientService.ObterListaDeAnomaliasPorViagem(pViagemId)).Data;
            if (xAnomalias is not null)
            {
                var xAnomaliasResponse = xAnomalias.ToList();
                _logger.LogInformation(
                    "[Anomalia: {Anomalia},",
                    JsonConvert.SerializeObject(xAnomalias));

                return xAnomaliasResponse;
            }

            _logger.LogInformation("[Nenhuma anomalia encontrada.]");
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
        }

        catch (ApiException xException)
        {
            throw new Exception(
                $"{xException.StatusCode} Nenhuma anomalia encontrada.");
        }

        return null;
    }
}
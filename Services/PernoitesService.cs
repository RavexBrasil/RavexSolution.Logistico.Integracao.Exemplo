using Newtonsoft.Json;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class PernoitesService
{
    private readonly ILogger<PernoitesService> _logger;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public PernoitesService(
        ILogger<PernoitesService> pLogger,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService)
    {
        _logger = pLogger;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
    }

    public async Task<IEnumerable<PernoitesAprovadosResponse>?> ObterPernoites(int pViagemId)
    {
        _logger.LogInformation("[Buscando pernoites...]");
        try
        {
            var xPernoites = (await _sistemaLogisticaHttpClientService.ObterListaDePernoitesPorViagem(pViagemId)).Data;
            if (xPernoites is not null)
            {
                var xPernoitesResponse = xPernoites.ToList();
                    _logger.LogInformation(
                        "[Pernoite: {Pernoite},",
                        JsonConvert.SerializeObject(xPernoitesResponse));
                
                return xPernoitesResponse;
            }

            _logger.LogInformation("[Nenhuma pernoite encontrada.]");
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
        }

        catch (ApiException xException)
        {
            throw new Exception(
                $"{xException.StatusCode} Nenhuma pernoite encontrada.");
        }

        return null;
    }
}
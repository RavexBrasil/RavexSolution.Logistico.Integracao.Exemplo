using Newtonsoft.Json;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class RoteiroService
{
    private readonly ILogger<RoteiroService> _logger;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public RoteiroService(
        ILogger<RoteiroService> pLogger,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService)
    {
        _logger = pLogger;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
    }

    public async Task<IEnumerable<RoteiroResponse>?> ObterRoteiro()
    {
        _logger.LogInformation("[Buscando roteiro...]");
        IEnumerable<RoteiroResponse>? xDataRoteiro = null;
        try
        {
            xDataRoteiro =
                (await _sistemaLogisticaHttpClientService.ObterRoteiroPorPeriodo(DateTime.Now.AddHours(-1),
                    DateTime.Now)).Data;
            if (xDataRoteiro is not null)
            {
                var roteiroResponses = xDataRoteiro.ToList();
                if (roteiroResponses.Any())
                {
                    var xRoteiroResponses = roteiroResponses.ToList();
                    foreach (var xRoteiro in xRoteiroResponses)
                    {
                        _logger.LogInformation(
                            "[Roteiro: {Roteiro},",
                            JsonConvert.SerializeObject(xRoteiro));
                    }
                    return xRoteiroResponses;
                }   
            }
            _logger.LogInformation("[Nenhum roteiro encontrado.]");
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
            throw;
        }
        
        catch (ApiException xException)
        {
            if (xDataRoteiro is null)
                throw new Exception(
                    $"{xException.StatusCode} Nenhum roteiro encontrado");
        }

        return null;
    }
}
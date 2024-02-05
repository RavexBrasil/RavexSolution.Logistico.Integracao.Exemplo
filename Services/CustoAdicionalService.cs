using Newtonsoft.Json;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Services;

public class CustoAdicionalService
{
    private readonly ILogger<CustoAdicionalService> _logger;
    private readonly ISistemaLogisticaHttpClientService _sistemaLogisticaHttpClientService;

    public CustoAdicionalService(
        ILogger<CustoAdicionalService> pLogger,
        ISistemaLogisticaHttpClientService pSistemaLogisticaHttpClientService)
    {
        _logger = pLogger;
        _sistemaLogisticaHttpClientService = pSistemaLogisticaHttpClientService;
    }

    public async Task<IEnumerable<CustosAdicionaisAprovadosResponse>?> ObterCustosAdicionais(int pViagemId)
    {
        _logger.LogInformation("[Buscando custos adicionais...]");
        try
        {
            var xCustosAdicionais =
                (await _sistemaLogisticaHttpClientService.ObterListaDeCustoAdicionalPorViagem(pViagemId)).Data;
            if (xCustosAdicionais is not null)
            {
                var xCustoAdicionalResponse = xCustosAdicionais.ToList();
                _logger.LogInformation(
                    "[CustoAdicional: {CustoAdicional},",
                    JsonConvert.SerializeObject(xCustosAdicionais));

                return xCustoAdicionalResponse;
            }

            _logger.LogInformation("[Nenhum custo adicional encontrado.]");
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Conexão não pôde ser estabelecida com o serviço.");
        }

        catch (ApiException xException)
        {
            throw new Exception(
                $"{xException.StatusCode} Nenhum custo adicional encontrado.");
        }

        return null;
    }
}
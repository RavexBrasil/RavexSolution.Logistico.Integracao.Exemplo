using System.Diagnostics;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using RavexSolution.Logistico.Integracao.Exemplo.Services;

namespace RavexSolution.Logistico.Integracao.Exemplo.Workers;
public class ConsumidorRoteiroWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<ConsumidorRoteiroWorker> _logger;
    private readonly Thread _thread;
    private bool _rodando;

    public ConsumidorRoteiroWorker(Configuracoes pConfiguracoes
        , ILogger<ConsumidorRoteiroWorker> pLogger
        , IServiceProvider serviceProvider)
    {
        _configuracoes = pConfiguracoes;
        _logger = pLogger;
        _serviceProvider = serviceProvider;
        _thread = new Thread(() => Task.Run(ProcessaThread)) { Name = nameof(ConsumidorRoteiroWorker) };
    }

    private async Task<IEnumerable<RoteiroResponse>?> ProcessaThread()
    {
        while (_rodando)
        {
            var xStopwatch = Stopwatch.StartNew();
            try
            {
                await using var xScope = _serviceProvider.CreateAsyncScope();
                {
                    var xService = xScope.ServiceProvider.GetRequiredService<RoteiroService>();
                    return await xService.ObterRoteiro();
                }
            }
            catch (Exception xException)
            {
                _logger.LogError(xException, "{Mensagem}", xException.Message);
            }
            finally
            {
                _logger.LogInformation("[FinalizadoProcesso] [Elapsed:{StopwatchElapsed}] [Aguardando:{Aguardando} minutos]..."
                    , xStopwatch.Elapsed
                    , _configuracoes.IntegracaoWorkerIntervaloEntreProcessamentoEmMinutos);
                await Task.Delay(TimeSpan.FromMinutes(_configuracoes.IntegracaoWorkerIntervaloEntreProcessamentoEmMinutos));
            }
        }

        return null;
    }

    public Task StartAsync(CancellationToken pCancellationToken)
    {
        if (!_configuracoes.AtivarConsumidorRoteiroWorker)
        {
            _rodando = false;
            _logger.LogInformation("[{Worker}] Worker desativado", nameof(ConsumidorRoteiroWorker));
            return Task.CompletedTask;
        }

        _rodando = true;
        _thread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken pCancellationToken)
    {
        _rodando = false;
        return Task.CompletedTask;
    }
}
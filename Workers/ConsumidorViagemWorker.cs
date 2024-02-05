using System.Diagnostics;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using RavexSolution.Logistico.Integracao.Exemplo.Services;

namespace RavexSolution.Logistico.Integracao.Exemplo.Workers;
public class ConsumidorViagemWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<ConsumidorViagemWorker> _logger;
    private readonly Thread _thread;
    private bool _rodando;

    public ConsumidorViagemWorker(Configuracoes pConfiguracoes
        , ILogger<ConsumidorViagemWorker> pLogger
        , IServiceProvider serviceProvider)
    {
        _configuracoes = pConfiguracoes;
        _logger = pLogger;
        _serviceProvider = serviceProvider;
        _thread = new Thread(() => Task.Run(ProcessaThread)) { Name = nameof(ConsumidorViagemWorker) };
    }

    private async Task<IEnumerable<ViagensFinalizadasResponse>?> ProcessaThread()
    {
        while (_rodando)
        {
            var xStopwatch = Stopwatch.StartNew();
            try
            {
                await using var xScope = _serviceProvider.CreateAsyncScope();
                {
                    var xService = xScope.ServiceProvider.GetRequiredService<ViagemService>();
                    return await xService.ObterViagensFinalizadas();
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
        if (!_configuracoes.AtivarConsumidorViagemWorker)
        {
            _rodando = false;
            _logger.LogInformation("[{Worker}] Worker desativado", nameof(ConsumidorViagemWorker));
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
using System.Diagnostics;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Services;

namespace RavexSolution.Logistico.Integracao.Exemplo.Workers;

public class IntegracaoViagemWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<IntegracaoViagemWorker> _logger;
    private readonly Thread _thread;
    private bool _rodando;

    public IntegracaoViagemWorker(Configuracoes pConfiguracoes
        , ILogger<IntegracaoViagemWorker> pLogger
        , IServiceProvider serviceProvider)
    {
        _configuracoes = pConfiguracoes;
        _logger = pLogger;
        _serviceProvider = serviceProvider;
        _thread = new Thread(() => Task.Run(ProcessaThread)) { Name = nameof(IntegracaoViagemWorker) };
    }

    private async Task ProcessaThread()
    {
        while (_rodando)
        {
            var xStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("[IniciadoIntegracaoViagemWorker]");
            try
            {
                await using var xScope = _serviceProvider.CreateAsyncScope();
                {
                    var xService = xScope.ServiceProvider.GetRequiredService<ViagemService>();
                    var xViagens = xService.ObterViagens(_configuracoes.QuantidadeRegistrosAProcessar);
                    if (xViagens.Count > 0)
                        await xService.ProcessarViagens(xViagens);
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
    }

    public Task StartAsync(CancellationToken pCancellationToken)
    {
        if (!_configuracoes.AtivarIntegracaoViagemWorker)
        {
            _logger.LogInformation("[{Worker}] Worker desativado", nameof(IntegracaoViagemWorker));
            _rodando = false;
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
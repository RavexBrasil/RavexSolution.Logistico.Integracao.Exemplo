using System.Diagnostics;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Services;

namespace RavexSolution.Logistico.Integracao.Exemplo.Workers;
public class IntegracaoPontoReferenciaWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Configuracoes _configuracoes;
    private readonly ILogger<IntegracaoPontoReferenciaWorker> _logger;
    private readonly Thread _thread;
    private bool _rodando;

    public IntegracaoPontoReferenciaWorker(IServiceProvider serviceProvider
        , Configuracoes configuracoes
        , ILogger<IntegracaoPontoReferenciaWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuracoes = configuracoes;
        _logger = logger;
        _thread = new Thread(() => Task.Run(ProcessaThread)) { Name = nameof(IntegracaoPontoReferenciaWorker) };
    }

    private async Task ProcessaThread()
    {
        while (_rodando)
        {
            var xStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("[IniciadoIntegracaoPontoReferenciaWorker]");
            try
            {
                await using var xScope = _serviceProvider.CreateAsyncScope();
                {
                    var xService = xScope.ServiceProvider.GetRequiredService<PontoReferenciaService>();
                    var xPontoReferencia = xService.ObterPontosReferencia(_configuracoes.QuantidadeRegistrosAProcessar);
                    if (xPontoReferencia is not null && xPontoReferencia.Count > 0)
                        await xService.ProcessarItens(xPontoReferencia);
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
        if (!_configuracoes.AtivarIntegracaoPontoReferenciaWorker)
        {
            _logger.LogInformation("[{Worker}] Worker desativado", nameof(IntegracaoPontoReferenciaWorker));
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
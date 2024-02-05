using System.Diagnostics;
using System.Reflection;

namespace RavexSolution.Logistico.Integracao.Exemplo.Workers
{
    public class MainWorker : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MainWorker> _logger;

        public MainWorker(ILogger<MainWorker> pLogger
            , IConfiguration pConfiguration)
        {
            _logger = pLogger;
            _configuration = pConfiguration;
        }

        public Task StartAsync(CancellationToken pCancellationToken)
        {
            _logger.LogInformation("------------------------------------------------------------------------------------------------------------------------");
            _logger.LogInformation("Iniciando serviço");
            _logger.LogInformation("Nome do Processo: {ProcessName}", Process.GetCurrentProcess().ProcessName);
            _logger.LogInformation("Path............: {Path}", Assembly.GetEntryAssembly()?.Location);
            _logger.LogInformation("Versão..........: {Version}", Assembly.GetEntryAssembly()?.GetName().Version);
            _logger.LogInformation("Compilação......: {LastWriteTime:O}", File.GetLastWriteTime(Assembly.GetEntryAssembly()?.Location ?? string.Empty));

            foreach (var xKeyValuePair in _configuration.AsEnumerable())
            {
                _logger.LogInformation("Configuração...: {Key} = {Value}"
                    , xKeyValuePair.Key
                    , xKeyValuePair.Value);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken pCancellationToken)
        {
            _logger.LogInformation("Parando worker");
            return Task.CompletedTask;
        }
    }
}
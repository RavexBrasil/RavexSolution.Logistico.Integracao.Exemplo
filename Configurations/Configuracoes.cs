namespace RavexSolution.Logistico.Integracao.Exemplo.Configurations
{
    public class Configuracoes
    {
        public bool AtivarIntegracaoPedidoWorker { get; set; }
        public bool AtivarIntegracaoPontoReferenciaWorker { get; set; }
        public bool AtivarIntegracaoProdutoWorker { get; set; }
        public bool AtivarIntegracaoViagemWorker { get; set; }
        public bool AtivarConsumidorRoteiroWorker { get; set; }
        public bool AtivarConsumidorViagemWorker { get; set; }
        public int IntegracaoWorkerIntervaloEntreProcessamentoEmMinutos { get; set; }
        public string BaseUrl { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public int NumeroMaximoDeTentativas { get; set; }
        public string DbPath { get; set; }
        public int QuantidadeRegistrosAProcessar { get; set; }
    }
}
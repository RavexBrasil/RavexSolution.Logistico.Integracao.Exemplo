namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaEntregas
{
        public int Id { get; set; }
        public int FilaViagemId { get; set; }
        public int? EntregaGlokId { get; set; }
        public string CodigoReferencia { get; set; }
        public short Sequencia { get; set; }
        public int QuantidadeEstimadaNotasFiscais { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal PesoBruto { get; set; }
        public decimal PesoLiquido { get; set; }
        public decimal Cubagem { get; set; }
        public DateTime? LidoDataHora { get; set; }
        public bool ProcessadoComSucesso { get; set; }
        public bool ProcessadoComFalha { get; set; }
        public string? Observacao { get; set; }
        public int Tentativas { get; set; }
}
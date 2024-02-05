namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class FilaViagem
{
        public int Id { get; set; }
        public int? ViagemGlokId { get; set; }
        public string Identificador { get; set; }
        public DateTime EstimativaInicio { get; set; }
        public DateTime EstimativaFim { get; set; }
        public string Tipo { get; set; }
        public decimal PesoBrutoTotal { get; set; }
        public decimal PesoLiquidoTotal { get; set; }
        public float Valor { get; set; }
        public decimal KmEstimado { get; set; }
        public short QuantidadeCaixas { get; set; }
        public bool PossuiOrdemEspecial { get; set; }
        public bool ComputarIndicador { get; set; }
        public string CnpjEmbarcador { get; set; }
        public string CnpjUnidade { get; set; }
        public string CnpjCooperativa { get; set; }
        public string CnpjTransportadora { get; set; }
        public DateTime? LidoDataHora { get; set; }
        public bool ProcessadoComSucesso { get; set; }
        public bool ProcessadoComFalha { get; set; }
        public string? Observacao { get; set; }
        public int Tentativas { get; set; }
}
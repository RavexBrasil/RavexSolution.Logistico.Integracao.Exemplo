namespace RavexSolution.Logistico.Integracao.Exemplo.Entities;

public class Produto
{
    public DateTime CriadoDataHora { get; set; }
    public string Codigo { get; set; }
    public string Descricao { get; set; }
    public float Cubagem { get; set; }
    public float PesoLiquido { get; set; }
    public float PesoBruto { get; set; }
    public float Valor { get; set; }
    public string Unidade { get; set; }
}
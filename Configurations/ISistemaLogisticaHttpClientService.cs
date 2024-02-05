using RavexSolution.Logistico.Integracao.Exemplo.Entities;
using RavexSolution.Logistico.Integracao.Exemplo.Responses;
using Refit;

namespace RavexSolution.Logistico.Integracao.Exemplo.Configurations;

public interface ISistemaLogisticaHttpClientService
{
    [Get("/api/ponto-referencia/obter-id-por-codigo/{pCodigo}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdPorCodigoReferencia(string pCodigo);

    [Get("/api/entidade/obter-id-embarcador-por-cnpj/{pCnpj}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdEmbarcadorPorCnpj(string pCnpj);

    [Get("/api/entidade/obter-id-unidade-por-cnpj/{pCnpj}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdUnidadePorCnpj(string pCnpj);

    [Get("/api/entidade/obter-id-cooperativa-por-cnpj/{pCnpj}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdCooperativaPorCnpj(string pCnpj);

    [Get("/api/entidade/obter-id-transportadora-por-cnpj/{pCnpj}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdTransportadoraPorCnpj(string pCnpj);

    [Get("/api/produto/obter-id-por-codigo/{pCodigo}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdProdutoPorCodigo(string? pCodigo);

    [Get("/api/grupo-referencia/obter-id-por-nome/{pNome}")]
    [Headers("Authorization: Bearer")]
    Task<Response<ObterIdResponse>> ObterIdPorNome(string pNome);
    
    [Get("/api/roteiro/obter-roteiro-por-periodo")]
    [Headers("Authorization: Bearer")]
    Task<Response<IEnumerable<RoteiroResponse>?>> ObterRoteiroPorPeriodo(DateTime pDataInicial, DateTime pDataFinal);
    
    [Get("/api/viagem-faturada/finalizadas-por-periodo")]
    [Headers("Authorization: Bearer")]
    Task<Response<IEnumerable<ViagensFinalizadasResponse>?>> ObterListaDeViagensFinalizadasPorPeriodo(DateTime dataHoraInicio, DateTime dataHoraFim);
    
    [Get("/api/viagem-faturada/{pViagemId}/pernoites")]
    [Headers("Authorization: Bearer")]
    Task<Response<IEnumerable<PernoitesAprovadosResponse>?>> ObterListaDePernoitesPorViagem(int pViagemId);
    
    [Get("/api/viagem-faturada/{pViagemId}/custos-adicionais")]
    [Headers("Authorization: Bearer")]
    Task<Response<IEnumerable<CustosAdicionaisAprovadosResponse>?>> ObterListaDeCustoAdicionalPorViagem(int pViagemId);
    
    [Get("/api/viagem-faturada/{pViagemId}/anomalias-v2")]
    [Headers("Authorization: Bearer")]
    Task<Response<IEnumerable<AnomaliaRegistradaResponseV2>?>> ObterListaDeAnomaliasPorViagem(int pViagemId);

    [Post("/api/pedido")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostPedido(Pedido pAdicionarPedido);

    [Post("/api/ponto-referencia")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostPontoReferencia(PontoReferencia pAdicionarPontoReferencia);

    [Post("/api/produto")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostProduto(FilaProduto pAdicionarProduto);

    [Post("/api/pedido/{pPedidoId}/itens")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostItemPedido(int pPedidoId, PedidoItem pAdicionarPedidoItem);

    [Post("/api/viagem-faturada")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostViagem(Viagem pAdicionarViagem);

    [Post("/api/viagem-faturada/{id}/entregas")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostEntrega(int id, Entregas pAdicionarEntrega);

    [Post("/api/viagem-faturada/{id}/entregas/{entregaId}/notas-fiscais")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostNotaFiscal(int id, int entregaId, NotaFiscal pAdicionarNotaFiscal);

    [Post("/api/viagem-faturada/{id}/entregas/{entregaId}/notas-fiscais/{notaFiscalId}/itens")]
    [Headers("Authorization: Bearer")]
    Task<Response<int>> PostNotaFiscalItem(int id, int entregaId, int? notaFiscalId, NotaFiscalItem pAdicionarNotaFiscalItem);
}
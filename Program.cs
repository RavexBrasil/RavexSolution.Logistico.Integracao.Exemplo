using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Web;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Contexts;
using RavexSolution.Logistico.Integracao.Exemplo.Handlers;
using RavexSolution.Logistico.Integracao.Exemplo.Repository;
using RavexSolution.Logistico.Integracao.Exemplo.Services;
using RavexSolution.Logistico.Integracao.Exemplo.Workers;
using Refit;

var xBuilder = WebApplication.CreateBuilder(args);
{
    xBuilder.Host.UseNLog();
    var xConfiguracao = xBuilder.Configuration.GetRequiredSection(nameof(Configuracoes))
        .Get<Configuracoes>(p => p.ErrorOnUnknownConfiguration = true);

    // configurations
    xBuilder.Configuration.AddUserSecrets<Program>();
    xBuilder.Services.Configure<Configuracoes>(xBuilder.Configuration.GetSection(nameof(Configuracoes)),
        p => p.ErrorOnUnknownConfiguration = true);
    xBuilder.Services.AddSingleton<Configuracoes>(p => p.GetRequiredService<IOptions<Configuracoes>>().Value);
    
    xBuilder.Services.AddHostedService<MainWorker>();
    
    //Pedido
    xBuilder.Services.AddDbContext<IntegracaoContext>(p => p.UseSqlServer());
    xBuilder.Services.AddScoped<IntegracaoPedidoRepository>();
    xBuilder.Services.AddScoped<PedidoService>();
    
    //Item do pedido
    xBuilder.Services.AddHostedService<IntegracaoPedidoWorker>();
    xBuilder.Services.AddScoped<IntegracaoPedidoItemRepository>();
    xBuilder.Services.AddScoped<PedidoItemService>();
    
    //Produto
    xBuilder.Services.AddHostedService<IntegracaoProdutoWorker>();
    xBuilder.Services.AddScoped<IntegracaoProdutoRepository>();
    xBuilder.Services.AddScoped<ProdutoService>();
    
    //Ponto de referência
    xBuilder.Services.AddHostedService<IntegracaoPontoReferenciaWorker>();
    xBuilder.Services.AddScoped<IntegracaoPontoReferenciaRepository>();
    xBuilder.Services.AddScoped<PontoReferenciaService>();
    
    //Viagem
    xBuilder.Services.AddHostedService<IntegracaoViagemWorker>();
    xBuilder.Services.AddScoped<IntegracaoViagemRepository>();
    xBuilder.Services.AddScoped<ViagemService>();
    xBuilder.Services.AddHostedService<ConsumidorViagemWorker>();

    //Entrega
    xBuilder.Services.AddScoped<IntegracaoEntregasRepository>();
    xBuilder.Services.AddScoped<EntregasService>();
    
    //Nota fiscal
    xBuilder.Services.AddScoped<IntegracaoNotaFiscalRepository>();
    xBuilder.Services.AddScoped<NotaFiscalService>();
    
    //Item da nota fiscal
    xBuilder.Services.AddScoped<IntegracaoNotaFiscalItemRepository>();
    xBuilder.Services.AddScoped<NotaFiscalItemService>();
    
    //Busca dos roteiros
    xBuilder.Services.AddHostedService<ConsumidorRoteiroWorker>();
    xBuilder.Services.AddScoped<RoteiroService>();
    
    //Busca das pernoites
    xBuilder.Services.AddScoped<PernoitesService>();
    
    //Busca dos custos adicionais
    xBuilder.Services.AddScoped<CustoAdicionalService>();
    
    //Busca das anomalias
    xBuilder.Services.AddScoped<AnomaliasService>();
    
    // Refit
    xBuilder.Services.AddTransient<AuthHeaderHandler>();

    var xBaseAddress = new Uri(xConfiguracao.BaseUrl);
    xBuilder.Services.AddRefitClient<ITokenHttpClientService>()
        .ConfigureHttpClient(p => p.BaseAddress = xBaseAddress);

    xBuilder.Services.AddRefitClient<ISistemaLogisticaHttpClientService>()
        .AddHttpMessageHandler<AuthHeaderHandler>()
        .ConfigureHttpClient(p => p.BaseAddress = xBaseAddress);
}

var xApp = xBuilder.Build();

xApp.Run();
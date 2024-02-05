using Microsoft.EntityFrameworkCore;
using RavexSolution.Logistico.Integracao.Exemplo.Configurations;
using RavexSolution.Logistico.Integracao.Exemplo.Entities;

namespace RavexSolution.Logistico.Integracao.Exemplo.Contexts;

public class IntegracaoContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<FilaPedido> FilaPedido => Set<FilaPedido>();
    public DbSet<FilaEntregas> FilaEntregas => Set<FilaEntregas>();
    public DbSet<FilaNotaFiscal> FilaNotaFiscal => Set<FilaNotaFiscal>();
    public DbSet<FilaNotaFiscalItem> FilaNotaFiscalItem => Set<FilaNotaFiscalItem>();
    public DbSet<FilaPedidoItem> FilaPedidoItem => Set<FilaPedidoItem>();
    public DbSet<FilaPontoReferencia> FilaPontoReferencia => Set<FilaPontoReferencia>();
    public DbSet<FilaProduto> FilaProduto => Set<FilaProduto>();
    public DbSet<FilaViagem> FilaViagem => Set<FilaViagem>();
    public string DbPath { get; }

    public IntegracaoContext(Configuracoes pConfiguracoes, DbContextOptions<IntegracaoContext> pOptions, IConfiguration pConfiguration) : base(pOptions)
    {
        _configuration = pConfiguration;
        DbPath = pConfiguracoes.DbPath;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        modelBuilder.HasDefaultSchema("TemplateIntegracao");
        modelBuilder.Entity<FilaProduto>(p =>
        {
            p.Property(pProduto => pProduto.Valor).HasColumnType("float");
            p.Property(pProduto => pProduto.PesoBruto).HasColumnType("decimal(15,3)");
            p.Property(pProduto => pProduto.PesoLiquido).HasColumnType("decimal(15,3)");
            p.Property(pProduto => pProduto.Cubagem).HasColumnType("float");
        });

        modelBuilder.Entity<FilaViagem>(p =>
        {
            p.Property(pViagem => pViagem.Valor).HasColumnType("float");
            p.Property(pViagem => pViagem.KmEstimado).HasColumnType("decimal(15,3)");
            p.Property(pViagem => pViagem.PesoLiquidoTotal).HasColumnType("decimal(15,3)");
            p.Property(pViagem => pViagem.PesoBrutoTotal).HasColumnType("decimal(15,3)");
        });

        modelBuilder.Entity<FilaEntregas>(p =>
        {
            p.Property(pEntrega => pEntrega.Cubagem).HasColumnType("decimal(15,6)");
            p.Property(pEntrega => pEntrega.PesoLiquido).HasColumnType("decimal(15,3)");
            p.Property(pEntrega => pEntrega.PesoBruto).HasColumnType("decimal(15,6)");
            p.Property(pEntrega => pEntrega.Latitude).HasColumnType("decimal(9,6)");
            p.Property(pEntrega => pEntrega.Longitude).HasColumnType("decimal(9,6)");
        });

        modelBuilder.Entity<FilaNotaFiscal>(p =>
        {
            p.Property(pNotaFiscal => pNotaFiscal.Cubagem).HasColumnType("decimal(15,6)");
            p.Property(pNotaFiscal => pNotaFiscal.PesoLiquido).HasColumnType("decimal(15,3)");
            p.Property(pNotaFiscal => pNotaFiscal.PesoBruto).HasColumnType("decimal(15,6)");
            p.Property(pNotaFiscal => pNotaFiscal.Valor).HasColumnType("float");
        });

        modelBuilder.Entity<FilaNotaFiscalItem>(p =>
        {
            p.Property(pNotaFiscalItem => pNotaFiscalItem.PesoLiquido).HasColumnType("decimal(15,3)");
            p.Property(pNotaFiscalItem => pNotaFiscalItem.PesoBruto).HasColumnType("decimal(15,6)");
            p.Property(pNotaFiscalItem => pNotaFiscalItem.Quantidade).HasColumnType("decimal(9,3)");
            p.Property(pNotaFiscalItem => pNotaFiscalItem.ValorUnitario).HasColumnType("float");
        });

        modelBuilder.Entity<FilaPedido>(p =>
        {
            p.Property(pPedido => pPedido.PesoBruto).HasColumnType("decimal(12,3)");
            p.Property(pPedido => pPedido.PesoLiquido).HasColumnType("decimal(12,3)");
            p.Property(pPedido => pPedido.Cubagem).HasColumnType("decimal(9,3)");
            p.Property(pPedido => pPedido.ValorPedido).HasColumnType("decimal(9,3)");
        });

        modelBuilder.Entity<FilaPedidoItem>(p =>
        {
            p.Property(pPedidoItem => pPedidoItem.Quantidade).HasColumnType("decimal(9, 3)");
            p.Property(pPedidoItem => pPedidoItem.PesoBruto).HasColumnType("decimal(12,6)");
            p.Property(pPedidoItem => pPedidoItem.PesoLiquido).HasColumnType("decimal(12,3)");
            p.Property(pPedidoItem => pPedidoItem.ValorUnitario).HasColumnType("decimal(9,3)");
        });

        modelBuilder.Entity<FilaPontoReferencia>(p =>
        {
            p.Property(pPontoReferencia => pPontoReferencia.Latitude).HasColumnType("decimal(8, 6)");
            p.Property(pPontoReferencia => pPontoReferencia.Longitude).HasColumnType("decimal(8, 6)");
            p.Property(pPontoReferencia => pPontoReferencia.Codigo).HasColumnType("varchar(30)").HasMaxLength(20);
            p.Property(pPontoReferencia => pPontoReferencia.Nome).HasColumnType("varchar(30)").HasMaxLength(100);
            p.Property(pPontoReferencia => pPontoReferencia.NomeGrupoReferencia).HasColumnType("varchar(30)").HasMaxLength(50);
            p.Property(pPontoReferencia => pPontoReferencia.TipoPessoa).HasColumnType("varchar(1)").HasMaxLength(1);
        });

        modelBuilder.Entity<FilaProduto>(p =>
        {
            p.Property(pProduto => pProduto.Cubagem).HasColumnType("float");
            p.Property(pProduto => pProduto.PesoLiquido).HasColumnType("decimal(15, 3)");
            p.Property(pProduto => pProduto.PesoBruto).HasColumnType("decimal(15, 3)");
            p.Property(pProduto => pProduto.Valor).HasColumnType("float");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
}
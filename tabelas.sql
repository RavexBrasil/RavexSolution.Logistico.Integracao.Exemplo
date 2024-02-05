CREATE SCHEMA TemplateIntegracao;

CREATE TABLE TemplateIntegracao.FilaPedido
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    PedidoGlokId INT NULL,
    CodigoPontoReferencia VARCHAR(30) NOT NULL,
    CnpjUnidade VARCHAR(30) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    EstimativaEntrega DATETIME NOT NULL,
    DataPedido DATETIME NOT NULL,
    PesoBruto DECIMAL(12, 3) NOT NULL,
    PesoLiquido DECIMAL(12, 3) NOT NULL,
    Cubagem DECIMAL(9, 3) NOT NULL,
    ValorPedido DECIMAL(9, 3) NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaPedidos_PedidoId ON FilaPedidos (PedidoGlokId);

CREATE TABLE TemplateIntegracao.FilaPedidoItem
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    PedidoItemGlokId INT,
    FilaPedidoId INT REFERENCES TemplateIntegracao.FilaPedido (Id) NOT NULL,
    Quantidade DECIMAL(9, 3)   NOT NULL,
    PesoBruto DECIMAL(12, 6) NOT NULL,
    PesoLiquido DECIMAL(12, 3) NOT NULL,
    ValorUnitario DECIMAL(9, 3) NOT NULL,
    CodigoProduto VARCHAR(30) NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaPedidoItens_ItemId ON FilaPedidoItens (PedidoItemGlokId);

CREATE TABLE TemplateIntegracao.FilaProduto
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ProdutoGlokId INT,
    Codigo VARCHAR(30) NOT NULL,
    Descricao VARCHAR(100) NOT NULL,
    Cubagem DECIMAL(15, 3) NOT NULL,
    Unidade VARCHAR(30) NOT NULL,
    PesoLiquido DECIMAL(15, 3),
    PesoBruto DECIMAL(15, 3),
    Valor FLOAT,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaProduto_ProdutoId ON FilaProduto (ProdutoGlokId);

CREATE TABLE TemplateIntegracao.FilaPontoReferencia
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    PontoReferenciaGlokId INT,
    NomeGrupoReferencia VARCHAR(30) NOT NULL,
    Codigo VARCHAR(20) NOT NULL,
    TipoPessoa VARCHAR(1) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Latitude DECIMAL(8, 6) NOT NULL,
    Longitude DECIMAL(8, 6) NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaPontoReferencia_ProdutoId ON FilaPontoReferencia (PontoReferenciaGlokId);

CREATE TABLE TemplateIntegracao.FilaViagem
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ViagemGlokId INT,
    Identificador VARCHAR(30) NOT NULL,
    EstimativaInicio DATETIME NOT NULL,
    EstimativaFim DATETIME NOT NULL,
    Tipo VARCHAR(10) NOT NULL,
    PesoBrutoTotal DECIMAL(15, 3) NOT NULL,
    PesoLiquidoTotal DECIMAL(15, 3) NOT NULL,
    Valor FLOAT NOT NULL,
    KmEstimado DECIMAL(15, 3) NOT NULL,
    QuantidadeCaixas SMALLINT NOT NULL,
    PossuiOrdemEspecial BIT NOT NULL,
    ComputarIndicador BIT NOT NULL,
    CnpjEmbarcador VARCHAR(30) NOT NULL,
    CnpjUnidade VARCHAR(30) NOT NULL,
    CnpjCooperativa VARCHAR(30) NOT NULL,
    CnpjTransportadora VARCHAR(30) NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaViagem_ViagemId ON TemplateIntegracao.FilaViagem (ViagemGlokId);

CREATE TABLE TemplateIntegracao.FilaEntrega
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    FilaViagemId INT REFERENCES TemplateIntegracao.FilaViagem (Id) NOT NULL,
    EntregaGlokId INT,
    CodigoReferencia VARCHAR(30) NOT NULL,
    Sequencia SMALLINT NOT NULL,
    QuantidadeEstimadaNotasFiscais INT NOT NULL,
    Latitude DECIMAL(9, 6) NOT NULL,
    Longitude DECIMAL(9, 6) NOT NULL,
    PesoBruto DECIMAL(15, 6) NOT NULL,
    PesoLiquido DECIMAL(15, 3) NOT NULL,
    Cubagem DECIMAL(15, 6) NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaEntregas_EntregaId ON TemplateIntegracao.FilaEntregas (EntregaGlokId);

CREATE TABLE TemplateIntegracao.FilaNotaFiscal
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    FilaEntregaId INT REFERENCES TemplateIntegracao.FilaEntregas (Id) NOT NULL,
    NotaFiscalGlokId INT,
    Sequencia SMALLINT NOT NULL,
    TipoOperacao VARCHAR(1) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Serie INT NOT NULL,
    PesoBruto DECIMAL(15, 6) NOT NULL,
    PesoLiquido DECIMAL(15, 3) NOT NULL,
    Cubagem DECIMAL(15, 6) NOT NULL,
    Valor FLOAT NOT NULL,
    QuantidadeCaixas SMALLINT NOT NULL,
    QuantidadeEstimadaItens INT NOT NULL,
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaNotaFiscal_NotaFiscalId ON TemplateIntegracao.FilaNotaFiscal (NotaFiscalGlokId);

CREATE TABLE TemplateIntegracao.FilaNotaFiscalItem
(
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    FilaNotaFiscalId INT REFERENCES TemplateIntegracao.FilaNotaFiscal (Id) NOT NULL,
    NotaFiscalItemGlokId INT,
    Sequencia TINYINT NOT NULL,
    ValorUnitario FLOAT NOT NULL,
    Quantidade DECIMAL(9, 3) NOT NULL,
    PesoBruto DECIMAL(15, 6) NOT NULL,
    PesoLiquido DECIMAL(15, 3) NOT NULL,
    Prioridade SMALLINT NOT NULL,
    CodigoProduto VARCHAR(60),
    LidoDataHora DATETIME,
    ProcessadoComSucesso BIT NOT NULL DEFAULT 0,
    ProcessadoComFalha BIT NOT NULL DEFAULT 0,
    Observacao VARCHAR(1000),
    Tentativas INT NOT NULL DEFAULT 0,
    CriadoDataHora DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IF NOT EXISTS FilaNotaFiscalItem_NotaFiscalItemId ON TemplateIntegracao.FilaNotaFiscalItem (NotaFiscaltemGlokId);
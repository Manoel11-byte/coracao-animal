-- SQL Server / Azure SQL. Versão 1. Execute em banco novo, preservando o PIM III.
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.Animais') IS NOT NULL AND COL_LENGTH(N'dbo.Animais',N'Id') IS NULL
 THROW 51000,'Esquema do PIM III detectado. Utilize um banco novo e preserve o anterior.',1;
GO
IF OBJECT_ID(N'dbo.Usuarios') IS NULL BEGIN
 CREATE TABLE dbo.Usuarios(
 Id int IDENTITY PRIMARY KEY,
 Nome nvarchar(150) NOT NULL,
 Email nvarchar(254) NOT NULL UNIQUE,
 SenhaHash nvarchar(500) NOT NULL,
 Papel nvarchar(20) NOT NULL DEFAULT N'usuario',
 Cpf nvarchar(11) NULL,
 Telefone nvarchar(20) NOT NULL DEFAULT N'',
 Endereco nvarchar(255) NOT NULL DEFAULT N'',
 Tentativas int NOT NULL DEFAULT 0,
 BloqueadoAte datetime2 NULL,
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT CK_Usuarios_Papel CHECK(Papel IN(N'usuario',
 N'admin')),
 CONSTRAINT CK_Usuarios_Cpf CHECK(Cpf IS NULL OR (LEN(Cpf)=11 AND Cpf NOT LIKE '%[^0-9]%')));
 CREATE UNIQUE INDEX IX_Usuarios_Cpf ON dbo.Usuarios(Cpf) WHERE Cpf IS NOT NULL;
END;
GO
IF OBJECT_ID(N'dbo.Sessoes') IS NULL BEGIN
 CREATE TABLE dbo.Sessoes(
 Hash nvarchar(64) PRIMARY KEY,
 UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
 Cliente nvarchar(10) NOT NULL,
 ExpiraEm datetime2 NOT NULL,
 CONSTRAINT CK_Sessoes_Cliente CHECK(Cliente IN(N'web',
 N'app')));
 CREATE INDEX IX_Sessoes_ExpiraEm ON dbo.Sessoes(ExpiraEm); CREATE INDEX IX_Sessoes_UsuarioId ON dbo.Sessoes(UsuarioId);
END;
GO
IF OBJECT_ID(N'dbo.Adotantes') IS NULL BEGIN
 CREATE TABLE dbo.Adotantes(
 Id int IDENTITY PRIMARY KEY,
 UsuarioId int NOT NULL UNIQUE REFERENCES dbo.Usuarios(Id),
 EspeciePreferida nvarchar(10) NOT NULL DEFAULT N'cao',
 Espaco nvarchar(10) NOT NULL DEFAULT N'medio',
 Experiencia nvarchar(10) NOT NULL DEFAULT N'alguma',
 CONSTRAINT CK_Adotantes_Especie CHECK(EspeciePreferida IN(N'cao',
 N'gato')),
 CONSTRAINT CK_Adotantes_Espaco CHECK(Espaco IN(N'pequeno',
 N'medio',
 N'grande')),
 CONSTRAINT CK_Adotantes_Experiencia CHECK(Experiencia IN(N'nenhuma',
 N'alguma',
 N'muita')));
 
END;
GO
IF OBJECT_ID(N'dbo.Animais') IS NULL BEGIN
 CREATE TABLE dbo.Animais(
 Id int IDENTITY PRIMARY KEY,
 Nome nvarchar(100) NOT NULL,
 Especie nvarchar(10) NOT NULL,
 Raca nvarchar(80) NOT NULL DEFAULT N'SRD',
 Idade int NOT NULL,
 Porte nvarchar(10) NOT NULL,
 Status nvarchar(20) NOT NULL DEFAULT N'disponivel',
 Descricao nvarchar(4000) NOT NULL DEFAULT N'',
 FotoUrl nvarchar(500) NULL,
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 Arquivado bit NOT NULL DEFAULT 0,
 CONSTRAINT CK_Animais_Especie CHECK(Especie IN(N'cao',
 N'gato')),
 CONSTRAINT CK_Animais_Idade CHECK(Idade BETWEEN 0 AND 40),
 CONSTRAINT CK_Animais_Porte CHECK(Porte IN(N'pequeno',
 N'medio',
 N'grande')),
 CONSTRAINT CK_Animais_Status CHECK(Status IN(N'disponivel',
 N'em_tratamento',
 N'em_processo',
 N'adotado')));
 CREATE INDEX IX_Animais_Catalogo ON dbo.Animais(Arquivado,Status,Especie,Porte) INCLUDE(Nome,Idade);
END;
GO
IF OBJECT_ID(N'dbo.Adocoes') IS NULL BEGIN
 CREATE TABLE dbo.Adocoes(
 Id int IDENTITY PRIMARY KEY,
 AnimalId int NOT NULL REFERENCES dbo.Animais(Id),
 AdotanteId int NOT NULL REFERENCES dbo.Adotantes(Id),
 Status nvarchar(20) NOT NULL DEFAULT N'pendente',
 Ativa bit NOT NULL DEFAULT 1,
 Observacoes nvarchar(2000) NOT NULL DEFAULT N'',
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 AtualizadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT CK_Adocoes_Status CHECK(Status IN(N'pendente',
 N'aprovada',
 N'concluida',
 N'cancelada')),
 CONSTRAINT CK_Adocoes_Ativa CHECK((Status IN(N'pendente',
 N'aprovada') AND Ativa=1) OR (Status IN(N'concluida',
 N'cancelada') AND Ativa=0)));
 CREATE UNIQUE INDEX IX_Adocoes_AnimalAtivo ON dbo.Adocoes(AnimalId) WHERE Ativa=1; CREATE INDEX IX_Adocoes_AdotanteData ON dbo.Adocoes(AdotanteId,CriadoEm DESC);
END;
GO
IF OBJECT_ID(N'dbo.Doacoes') IS NULL BEGIN
 CREATE TABLE dbo.Doacoes(
 Id int IDENTITY PRIMARY KEY,
 AdotanteId int NOT NULL REFERENCES dbo.Adotantes(Id),
 Valor decimal(10,
 2) NOT NULL,
 FormaPagamento nvarchar(20) NOT NULL,
 Status nvarchar(20) NOT NULL DEFAULT N'pendente',
 Descricao nvarchar(255) NOT NULL DEFAULT N'',
 Anonima bit NOT NULL DEFAULT 0,
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT CK_Doacoes_Valor CHECK(Valor>=1 AND Valor<=1000000),
 CONSTRAINT CK_Doacoes_Forma CHECK(FormaPagamento IN(N'pix',
 N'transferencia',
 N'dinheiro')),
 CONSTRAINT CK_Doacoes_Status CHECK(Status IN(N'pendente',
 N'confirmada',
 N'cancelada')));
 CREATE INDEX IX_Doacoes_AdotanteData ON dbo.Doacoes(AdotanteId,CriadoEm DESC);
END;
GO
IF OBJECT_ID(N'dbo.Voluntarios') IS NULL BEGIN
 CREATE TABLE dbo.Voluntarios(
 Id int IDENTITY PRIMARY KEY,
 UsuarioId int NOT NULL UNIQUE REFERENCES dbo.Usuarios(Id),
 Area nvarchar(100) NOT NULL,
 Disponibilidade nvarchar(500) NOT NULL,
 Status nvarchar(20) NOT NULL DEFAULT N'pendente',
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT CK_Voluntarios_Status CHECK(Status IN(N'pendente',
 N'ativo',
 N'inativo')));
 
END;
GO
IF OBJECT_ID(N'dbo.Auditorias') IS NULL BEGIN
 CREATE TABLE dbo.Auditorias(
 Id bigint IDENTITY PRIMARY KEY,
 UsuarioId int NULL,
 Acao nvarchar(100) NOT NULL,
 Entidade nvarchar(100) NOT NULL,
 RegistroId int NOT NULL,
 CriadoEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
 CREATE INDEX IX_Auditorias_Registro ON dbo.Auditorias(Entidade,RegistroId,CriadoEm);
END;
GO
CREATE OR ALTER TRIGGER dbo.trg_Adocoes_Auditoria ON dbo.Adocoes AFTER INSERT,UPDATE AS
BEGIN
 SET NOCOUNT ON;
 INSERT dbo.Auditorias(UsuarioId,Acao,Entidade,RegistroId)
 SELECT NULL,CASE WHEN d.Id IS NULL THEN N'criada (banco)' ELSE N'status: '+i.Status+N' (banco)' END,N'Adocoes',i.Id
 FROM inserted i LEFT JOIN deleted d ON i.Id=d.Id WHERE d.Id IS NULL OR i.Status<>d.Status;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_ResumoMensal @Ano int AS
BEGIN
 SET NOCOUNT ON;
 IF @Ano<2000 OR @Ano>2100 THROW 51000,'Ano inválido.',1;
 SELECT MONTH(CriadoEm) AS Mes,COUNT(*) AS Quantidade,SUM(Valor) AS TotalConfirmado FROM dbo.Doacoes
 WHERE Status=N'confirmada' AND CriadoEm>=DATEFROMPARTS(@Ano,1,1) AND CriadoEm<DATEFROMPARTS(@Ano+1,1,1)
 GROUP BY MONTH(CriadoEm) ORDER BY Mes;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_LimparSessoesExpiradas AS
BEGIN
 SET NOCOUNT ON;
 DELETE FROM dbo.Sessoes WHERE ExpiraEm<SYSUTCDATETIME();
END;
GO
IF OBJECT_ID(N'dbo.SchemaVersions') IS NULL CREATE TABLE dbo.SchemaVersions(Versao int PRIMARY KEY,AplicadaEm datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersions WHERE Versao=1) INSERT dbo.SchemaVersions(Versao) VALUES(1);
GO

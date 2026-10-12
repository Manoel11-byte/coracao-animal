-- Opcional. Animais fictícios, sem pessoas ou senhas de demonstração.
IF NOT EXISTS(SELECT 1 FROM dbo.Animais WHERE Nome=N'Luna')
 INSERT dbo.Animais(Nome,Especie,Raca,Idade,Porte,Status,Descricao) VALUES(N'Luna',N'cao',N'SRD',2,N'medio',N'disponivel',N'Animal fictício de demonstração. Carinhosa e cheia de energia.');
IF NOT EXISTS(SELECT 1 FROM dbo.Animais WHERE Nome=N'Nino')
 INSERT dbo.Animais(Nome,Especie,Raca,Idade,Porte,Status,Descricao) VALUES(N'Nino',N'gato',N'SRD',3,N'pequeno',N'disponivel',N'Animal fictício de demonstração. Tranquilo e curioso.');

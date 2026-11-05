# Portal e Windows 1.0.3 — 05/11/2026

O portal define fundo e texto no elemento que recebe os tokens do tema. Isso
corrige títulos e rótulos escuros em cartões escuros. Links, botões, alertas,
campos e preenchimento automático seguem as cores do tema. O layout é mantido.
Os testes de navegador verificam contraste mínimo de 4,5:1 nos textos dos
formulários, persistência do tema e ausência de rolagem horizontal em 1440,
390 e 320 pixels.

O Windows usa quatro seções, cartões de animais e filtros em uma linha. Os
formulários preservam rótulos acessíveis e o tratamento de erros permite tentar
novamente sem descartar os dados digitados. A versão é 1.0.3, com runtime .NET
10.0.14. A navegação é bloqueada apenas durante operações para evitar duplicação.

O CI audita dependências da API, Android e Windows, executa 39 testes .NET e
verifica o executável Windows em janela padrão e 900×650. O teste nativo navega
por catálogo, ajuda, login e cadastro sem criar contas ou enviar dados. As
capturas são publicadas no artefato `windows-review`. O pacote `windows-x64`
contém o executável autossuficiente para Windows x64.

A auditoria de dependências consulta falhas conhecidas; não equivale a um teste
de invasão. O binário Windows não recebe assinatura Authenticode neste projeto.

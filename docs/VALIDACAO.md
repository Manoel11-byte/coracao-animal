# Validação e aceite

## Executado

- API: build Release sem erros/avisos.
- Windows: build e publicação self-contained win-x64, gerando CoracaoAnimal.exe; sem execução no Windows neste ambiente Linux.
- Android: build Debug e APK assinado, sem erros/avisos; assinatura verificada por apksigner v2/v3. Instalação em aparelho/emulador ainda pendente.
- Testes: **17 casos aprovados, 0 falhas**, com evidências em `docs/evidencias/integration.trx` e `testes.txt`.
- Bicep: compilação do template sem erros. Nenhum recurso Azure provisionado.
- JavaScript: validação de sintaxe concluída.
- Site: teste funcional DOM com API real aprovado para início, login, administração/cadastro de animal, cadastro de conta, perfil, adoção, doação e voluntariado. Evidência em `docs/evidencias/site-dom.txt`; não substitui revisão visual.

Os testes cobrem autorização administrativa web/app, CSRF/origem, reserva de animal, perfil/aceite, isolamento e cancelamento de adoções, transições, doações pendentes, revogação de sessões, voluntariado único, rejeição de falsa imagem e política de URL dos apps.

## Limites da verificação

Os testes de integração usam SQLite. Não validam procedures/triggers no SQL Server, infraestrutura Azure, restauração, operação em múltiplas instâncias nem concorrência SQL real. O Chromium não iniciou devido ao bloqueio de sockets deste ambiente. Assim, não se declara revisão visual completa nem foram geradas capturas de tela. Os arquivos nativos foram compilados, mas ainda precisam ser executados nos sistemas de destino.

## Aceite no computador e celular

1. Iniciar API e site, verificar catálogo, navegação por teclado, foco, tema e telas pequenas.
2. Administrador cadastra animal e foto pelo site.
3. Adotante cria conta e completa CPF, telefone e endereço com dados fictícios.
4. Solicitar adoção no app e consultar o mesmo registro no site e no outro app.
5. Aprovar/concluir no site; atualizar histórico no app. Verificar que não há painel administrativo nos apps.
6. Registrar intenção de doação, confirmar recebimento no site e consultar histórico.
7. Inscrever/alterar/desativar voluntariado, alterar senha, sair e entrar novamente.
8. Testar texto ampliado e leitores de tela Android/Windows.
9. Repetir com SQL Server/Azure SQL e fotos no Blob; executar procedures, verificar auditoria e realizar restauração de teste.

O roteiro `tests/browser.cjs` é fornecido para execução em uma máquina com Playwright/Chromium. Ele usa credenciais fictícias fixas e deve rodar somente em uma base local de teste vazia. Não foi concluído neste ambiente.

## Repetir o teste funcional DOM

Na raiz do projeto: `dotnet build src/CoracaoAnimal.Api -c Release`, `npm install --no-save jsdom@26.1.0` e `node tests/site-dom.cjs`. O roteiro cria sua própria base SQLite temporária em artifacts e inicia a API na porta 5099. Precisa do .NET no PATH; use DOTNET_EXECUTABLE para indicar outro caminho.

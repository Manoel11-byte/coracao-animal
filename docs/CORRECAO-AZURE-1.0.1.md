# Integração Azure — versão 1.0.1

## Problema corrigido

O aplicativo esperava um catálogo paginado (`itens`, `total`, `paginaAtual`, `tamanho`), mas a API de outra assinatura retornava uma lista simples. O endereço usado como site também não hospedava uma página inicial.

## Implantação

O App Service `coracao-pkzqbhzfiqlry`, no grupo `rg-coracaoanimal`, agora publica a API compatível e a página pública do projeto original. O portal autenticado fica em `/portal.html`. Site, Android e Windows usam o mesmo serviço e o banco `coracaoanimal` no Azure SQL existente. As credenciais permanecem nas configurações do serviço e no Key Vault.

Os aplicativos migram automaticamente o endereço antigo e removem sessões pertencentes ao servidor anterior. O menu de conexão foi removido. O administrador é autorizado somente em uma sessão web.

Seis animais fictícios foram inseridos uma única vez com fotografias. Depois da inicialização, `Database__Initialize` e `Database__SeedDemo` foram desativados. Reinícios não restauram adoções nem recriam animais removidos.

## Validação

- 19 testes automatizados passaram: autorização web/app, isolamento de contas, reserva de animal, cancelamento, doações, voluntariado, sessões, contrato do catálogo, resumo e migração automática do endereço.
- `/health/ready` confirmou conexão com Azure SQL.
- Teste real por sessões distintas web/app confirmou a mesma conta, catálogo, adoção, doação e voluntariado.
- A solicitação de adoção e a intenção de doação criadas pelo teste foram canceladas; o interesse de voluntariado foi desativado. A conta é identificada como `Teste de integração Azure`.
- APK compilado em Release e assinatura v2/v3 validada. Windows x64 publicado com runtime incluído. A execução física desses binários depende de teste no Android e Windows do usuário.

## Recriar o site público

Instale as dependências com `pip install -r scripts/requirements-site.txt` e execute `python scripts/import-original-site.py`. O script usa o template original em `frontend/`, vincula as funções ao portal e baixa o vídeo de demonstração do repositório original quando necessário. Depois publique `src/CoracaoAnimal.Api` normalmente.

O script `tests/live-integration.py` cria dados fictícios em produção; execute apenas quando uma validação completa for desejada.

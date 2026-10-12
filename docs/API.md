# API HTTP

Base: `/api`. JSON camelCase, datas UTC. Os DTOs completos estão em `src/CoracaoAnimal.Core/Contracts.cs`. Erros de regra retornam `{ "mensagem": "...", "traceId": "..." }`. Respostas HTTP de autenticação/autorização podem não ter corpo. Clientes devem verificar o status.

| Método e rota | Acesso | Finalidade |
|---|---|---|
| POST /auth/cadastro | Público | Nome, email e senha; papel fixo de usuário |
| POST /auth/login | Público | Email, senha, cliente web/app |
| POST /auth/logout | Autenticado | Revoga a sessão atual |
| GET /animais | Público | Filtros busca/especie/porte/status/pagina/tamanho |
| GET /animais/{id} | Público | Perfil de animal não arquivado |
| GET /fotos/{nome} | Público | Foto do catálogo |
| GET, PUT /me/ | Autenticado | Perfil do próprio usuário |
| PUT /me/senha | Autenticado | Senha atual e nova, revogando todas as sessões |
| GET /me/recomendacoes | Autenticado | Até 12 animais disponíveis, ordenados por afinidade 0–100 |
| GET, POST /me/adocoes | Autenticado | Consultar histórico ou solicitar adoção |
| POST /me/adocoes/{id}/cancelar | Dono do registro | Cancelar solicitação ativa |
| GET, POST /me/doacoes | Autenticado | Consultar ou registrar intenção |
| POST /me/doacoes/{id}/cancelar | Dono do registro | Cancelar intenção pendente |
| GET, PUT, DELETE /me/voluntariado | Autenticado | Consultar, cadastrar/atualizar ou desativar inscrição |
| GET /admin/resumo | Admin web | Indicadores operacionais |
| POST /admin/animais | Admin web | Cadastro |
| PUT, DELETE /admin/animais/{id} | Admin web | Editar ou arquivar |
| POST /admin/animais/{id}/foto | Admin web | Multipart/form-data, campo foto |
| GET /admin/adotantes | Admin web | Lista paginada, CPF mascarado |
| GET /admin/adocoes, /admin/doacoes, /admin/voluntarios | Admin web | Listas paginadas |
| PUT /admin/adocoes/{id}/status | Admin web | Aprovar, concluir ou cancelar |
| PUT /admin/doacoes/{id}/status | Admin web | Confirmar recebimento ou cancelar |
| PUT /admin/voluntarios/{id}/status | Admin web | Ativar, desativar ou marcar pendente |

`/health/live` e `/health/ready` ficam fora de `/api`. O primeiro verifica processo; o segundo consulta conectividade com banco. Não há versão `/v1` nesta entrega; alterações incompatíveis exigem versionamento futuro.

## Exemplos

Login de app (cabeçalho `X-CSRF: 1`):
```json
{"email":"pessoa@example.com","senha":"senha-definida-pela-pessoa","cliente":"app"}
```

Retorna usuário, token e expiração. Nas chamadas seguintes use `Authorization: Bearer <token>`. O login web emite cookie; seu JSON não devolve token. Requisições web de escrita incluem `X-CSRF: 1` e a origem deve corresponder ao host. Não habilitar CORS irrestrito.

Solicitação de adoção:
```json
{"animalId":1,"observacoes":"Tenho espaço e disponibilidade para os cuidados.","aceitaTermos":true}
```

Intenção de doação:
```json
{"valor":25.50,"formaPagamento":"pix","descricao":"Ajuda com ração","anonima":false}
```

Listas paginadas retornam `itens`, `total`, `paginaAtual`, `tamanho`. Tamanho máximo público: 50. Listas administrativas: 50. Históricos individuais e recomendações retornam arrays. GET voluntariado pode retornar JSON null ou corpo vazio, interpretados como ausência de inscrição pelo cliente.

## Status

200 consulta/alteração, 201 criação, 204 operação sem corpo; 400 dados inválidos; 401 sessão ausente/expirada; 403 falta de permissão/origem inválida; 404 registro ausente ou de outro usuário; 409 conflito de negócio/dados; 429 limite de tentativas; 503 armazenamento não configurado. Erros inesperados não expõem a exceção ao cliente.

Não há garantia de idempotência para criação de doação. Após timeout, consultar o histórico antes de repetir; o cliente não repete POST automaticamente.

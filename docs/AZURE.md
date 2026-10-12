# Azure — implantação e operação

## Situação da entrega

Há código Bicep validado pelo compilador e scripts de implantação. **Nenhuma assinatura foi acessada, nenhum recurso criado e nenhum site publicado.** Validação de sintaxe não certifica quotas, políticas, disponibilidade regional, comportamento do SQL ou execução em nuvem.

## Recursos declarados

| Recurso | Configuração inicial | Uso |
|---|---|---|
| App Service Linux | B1, uma instância, .NET 10, HTTPS, TLS 1.2, health check | Site e API |
| Azure SQL Database | Basic, 5 DTU, 2 GB | Dados compartilhados |
| VNet/sub-rede | Integração App Service e service endpoints SQL/Storage | Restringe acesso a dados |
| Storage | Standard LRS, Blob privado, regras de rede | Fotos |
| Key Vault | RBAC, exclusão reversível | Conexões e bootstrap |
| Log Analytics | Retenção 30 dias | Logs e métricas |

SQL conserva endpoint público, porém aceita a sub-rede autorizada; não existe regra genérica “todos os serviços Azure”. Storage nega rede por padrão, autorizando a sub-rede. Não são private endpoints. Key Vault permite endpoint público, protegido por identidade/RBAC. Avaliar private endpoints e segmentação adicional conforme a operação real.

Os serviços são pagos. Não há orçamento/alerta financeiro criado automaticamente. Confira a calculadora Azure, a assinatura, a região e as quotas antes de executar. Não há promessa de faixa gratuita ou custo fixo.

## Antes da implantação

1. Instale Azure CLI, Bicep (`az bicep install`), SDK .NET 10 e PowerShell.
2. Execute `az login`; escolha a assinatura com `az account set --subscription <id>`.
3. A identidade precisa criar recursos e atribuições de papel no grupo. A política corporativa pode exigir intervenção do administrador da assinatura.
4. Execute os testes e teste o site local.
5. Use prefixo com letras minúsculas/números, começando por letra, de 3 a 11 caracteres.

## Implantação inicial

```powershell
./scripts/deploy-azure.ps1 -ResourceGroup rg-coracaoanimal -Location brazilsouth -Prefix coracao -AdminEmail seu-email@exemplo.com
```

O script mostra a assinatura e solicita confirmação antes de criar o grupo. Gera senhas aleatórias para SQL, pede senha administrativa por entrada protegida, executa what-if e solicita confirmação antes do deployment. O arquivo temporário de parâmetros contém segredos e é removido em finally; utilize uma máquina confiável. Não publique os parâmetros nem logs com segredos.

O Bicep cria o banco vazio; a primeira inicialização do aplicativo aplica `database/schema.sql`, cria `ca_runtime` e cadastra o administrador. As referências de segredos dependem da propagação do RBAC do Key Vault; se a primeira inicialização falhar, aguarde a propagação e reinicie o aplicativo, verificando os logs. Não habilite acesso SQL irrestrito para contornar falhas.

Confirme `/health/ready`, login administrativo, cadastro de animal com foto, registro de adotante, solicitação de adoção e visualização do mesmo registro em Android/Windows. Em seguida:

```powershell
./scripts/finalize-azure.ps1 -ResourceGroup rg-coracaoanimal -AppName <nome-retornado>
```

Esse script verifica saúde e solicita confirmação de acesso inicial. Remove as configurações de bootstrap e provisionamento e desativa a inicialização automática. Os segredos de provisionamento continuam no cofre: restringir/rotacionar conforme a política de operação. Não reaplique o Bicep inicial indiscriminadamente após a finalização, pois ele reintroduz configurações de inicialização; alterações posteriores devem preservar o estado final.

## Publicações seguintes

```powershell
./scripts/publish-web.ps1 -ResourceGroup rg-coracaoanimal -AppName <nome>
```

Esse comando atualiza somente o código. Não recria recursos. A aplicação não implementa migrações SQL automáticas futuras; scripts versionados devem preceder alterações de esquema. No plano B1 não há slot de staging configurado. Uma nova publicação pode causar breve indisponibilidade.

Nos aplicativos, configure a URL HTTPS retornada pelo deployment. A mesma conta passa a usar o banco central de produção. Dados SQLite locais não são enviados automaticamente ao Azure.

## CI/CD

`ci.yml` compila/testa API, empacota Windows e gera APK de teste. `deploy.yml` é manual e publica somente após os testes. Configure o ambiente GitHub `production`, suas revisões obrigatórias e as variáveis AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID e AZURE_APP_NAME. A identidade do Azure precisa de credencial federada associada ao repositório/ambiente e permissão de publicar no App Service. Nenhuma credencial de publicação está incluída.

Os workflows são entregues como código; não foram executados no GitHub nesta entrega. Não há publicação na Play Store nem assinatura Windows comercial. Para Android de produção, crie uma chave própria e utilize `build-android.ps1 -KeyStore <arquivo> -KeyAlias <alias>` com CA_SIGNING_PASSWORD no ambiente. Preserve a chave e a senha fora do repositório; atualizações precisam da mesma identidade de assinatura.

## Monitoramento e recuperação

- Consulte logs do App Service/Log Analytics, `/health/live` e `/health/ready`.
- Configure alertas de erro HTTP 5xx, indisponibilidade, consumo SQL e orçamento conforme os responsáveis definidos.
- Verifique backups e execute um teste de restauração para banco separado antes da operação real.
- Para rollback de código, publique o artefato anterior compatível com o esquema atual.
- Em falha de conexão SQL, verifique referências do Key Vault, usuário runtime, regras da sub-rede e integração VNet.
- Para fotos, confira referência do segredo, regras Storage e logs, mantendo o container privado.
- Para escalar, reavaliar consultas e limites do SQL; B1 inicial não implementa autoscaling.

## Referências oficiais

- https://learn.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore
- https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0

Documentação consultada durante o desenvolvimento. Valores, versões disponíveis e políticas da assinatura devem ser verificados na implantação.

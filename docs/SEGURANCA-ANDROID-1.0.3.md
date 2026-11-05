# Segurança e Android 1.0.3

## Escopo

Na revisão inicial, o site e o aplicativo Windows mantiveram a aparência anterior.
A revisão complementar de 05/11/2026 corrige contraste do portal e moderniza
a interface Windows; os detalhes estão em `REVISAO-WINDOWS-1.0.3.md`.

O Android usa quatro áreas persistentes: Explorar, Adoções, Ajudar e Conta.
O catálogo apresenta cartões, filtros expansíveis e termos legíveis. Formulários
têm rótulos, contraste e alvos de toque de pelo menos 48 unidades. A navegação
fica temporariamente indisponível durante solicitações para evitar ações
duplicadas. Tokens continuam no armazenamento seguro do Android.

## Provisionamento e publicação

`scripts/deploy-azure.ps1` destina-se exclusivamente a instalações novas.
O script interrompe a execução antes de gerar senhas ou alterar recursos quando
detecta App Service, SQL Server ou Key Vault já existentes no grupo. Falhas na
consulta também interrompem a execução. Isso evita trocar o segredo runtime sem
trocar a senha SQL e evita reaplicar o plano B1 a uma instalação F1.

Para atualizar uma instalação existente, use `scripts/publish-web.ps1` ou o
workflow `Deploy Azure`, que publicam código sem reaplicar o Bicep. Não execute
`infra/main.bicep` diretamente sobre o ambiente existente: ele continua sendo um
modelo de instalação nova com B1, rede virtual e credenciais de provisionamento.
Uma futura rotação de senha exige um procedimento coordenado e separado; esta
versão não faz rotação automática de credenciais.

## Configuração Azure

Na revisão de 03/11/2026 foram removidas do App Service as referências temporárias
`Database__AdminConnection`, `Bootstrap__Email` e `Bootstrap__Password`, mantendo
`Database__Initialize=false`. Os segredos do Key Vault não foram apagados.
Remover referências não revoga, por si só, permissões existentes da identidade
do aplicativo sobre o cofre; revisar esse escopo continua sendo uma etapa
separada de gestão de identidade.

O firewall SQL substituiu a exceção geral para serviços Azure por regras com
endereços IPv4 exatos de saída exibidos pelo App Service. O banco permanece em
redes selecionadas. Os IPs de saída são compartilhados pelo provedor, portanto
essa medida reduz a exposição, mas não substitui autenticação e TLS. Antes de
trocar plano, região ou ambiente de hospedagem, revise os IPs e as regras.

Após autorização específica do responsável, a auditoria SQL foi ativada com
identidade gerenciada, armazenamento existente e retenção de 7 dias. O portal
confirmou a gravação da configuração. O armazenamento de logs pode ter cobrança
por consumo.

## Verificação

- 39 testes .NET aprovados localmente, incluindo cookies HttpOnly/Secure/Strict,
  separação de token web/app, rejeição de sessão expirada e bloqueio de listas
  administrativas para usuários anônimos.
- Auditoria NuGet da API/testes e do Android, incluindo dependências transitivas,
  sem vulnerabilidades conhecidas reportadas na consulta de 03/11/2026.
- `/health/ready` e catálogo responderam HTTP 200 após a restrição do firewall.
- CI inclui teste offline de provisionamento, auditoria de dependências e teste
  de navegação Android em emulador. Artefatos de revisão incluem imagens e XML
  das telas; os fluxos de cadastro não são enviados ao servidor nesse teste.

A auditoria de dependências não é um teste de invasão nem garante ausência de
falhas desconhecidas. O teste em emulador não substitui testes em aparelhos
físicos. O APK gerado pelo CI usa assinatura de desenvolvimento; para atualizar
uma instalação distribuída, use o mesmo certificado da versão anterior por meio
de `scripts/build-android.ps1`, sem adicionar chaves ou senhas ao repositório.

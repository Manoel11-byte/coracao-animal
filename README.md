# Coração Animal — PIM IV

Versão 1.0.2 — API, site, Android e Windows integrados ao Azure SQL da assinatura do proprietário.

- Site: https://coracao-pkzqbhzfiqlry.azurewebsites.net/
- Conta e catálogo: https://coracao-pkzqbhzfiqlry.azurewebsites.net/portal.html#animais
- API: https://coracao-pkzqbhzfiqlry.azurewebsites.net/api/animais
- Saúde e banco: https://coracao-pkzqbhzfiqlry.azurewebsites.net/health/ready
- Downloads: https://github.com/Manoel11-byte/coracao-animal/releases/latest

Os apps conectam automaticamente ao endereço acima. Instale os pacotes da versão 1.0.2. Caso o Android recuse atualizar uma instalação antiga devido à assinatura, desinstale o APK anterior e instale o novo; os dados do servidor permanecem no Azure.

Luna, Nino, Mel, Thor, Amora e Bento são animais fictícios, com fotografias de demonstração. Cadastre uma conta e complete seu perfil para testar adoções. Doações registram intenções, sem cobrança automática. A área administrativa fica somente no site.

A página inicial mantém o layout e os materiais do projeto original de https://github.com/m4rcosJr/coracao-animal. As funções autenticadas utilizam o portal integrado, com a mesma paleta de cores.

Solução integrada baseada no projeto Coração Animal fornecido pelo usuário. Site ASP.NET Core, aplicativo Android .NET MAUI e aplicativo Windows WinForms usam a mesma API e o mesmo banco. **Administração exclusivamente pelo site.**

## Revisão 1.0.2

- Cards com fotos de altura uniforme e informações visíveis.
- Gráficos SVG responsivos de adoções e doações, escalas separadas, tabela acessível e indicação de ausência de registros.
- Catálogo preservado quando o painel falha, com nova tentativa.
- Android/Windows: leitura correta de valores com ponto ou vírgula decimal, sessão expirada encaminhada ao login e mensagens claras de erro.
- Windows com fotos no catálogo, controles adaptáveis e teste de inicialização aprovado.
- 37 testes .NET aprovados; testes Chromium de cadastro, login, perfil, adoção, doação, voluntariado e administração aprovados em ambiente local isolado, com revisão responsiva em 1440, 390 e 320 pixels.
- Publicação atualizada no Azure. A execução em dispositivos Android/Windows físicos continua sendo uma etapa de aceite.

Veja `docs/REVISAO-1.0.2.md` para detalhes e limites da validação.

## Comece aqui

1. Instale o SDK .NET 10. Para executar somente o EXE publicado, não é necessário instalar o SDK.
2. Extraia o código e dê dois cliques em `INICIAR-SERVIDOR.cmd`. Ele evita o bloqueio comum de scripts do PowerShell.
3. Informe e-mail e senha para criar o primeiro administrador. Abra `http://localhost:5080`.
4. Entre pelo site, abra Administração → Animais e cadastre um animal, incluindo foto se desejar.
5. Crie outra conta para testar os fluxos do adotante. Complete CPF, telefone e endereço antes de solicitar adoção. Utilize dados fictícios na demonstração.
6. No Windows, execute `CoracaoAnimal.exe`; a conexão com o Azure é automática.
7. No Android, instale `CoracaoAnimal-Android.apk`; a conexão com o Azure é automática.

A configuração Development cria um banco SQLite local. Em produção o provedor é SQL Server/Azure SQL. O catálogo começa vazio; não há senhas padrão nem dados reais embutidos. O SQL de demonstração opcional está em `database/seed-demo.sql` e destina-se ao SQL Server.

## Entregas e limites

- Site com área pública, conta, preferências, adoções, intenções de doação, voluntariado e administração.
- API .NET com sessões no servidor, autorização, validação e transações.
- Windows x64: publicação independente do runtime, em arquivo EXE. Interface nativa WinForms.
- Android 7/API 24 ou superior: interface nativa MAUI. APK assinado para avaliação; publicação oficial exige chave de assinatura própria.
- Azure: site e API publicados no App Service existente, dados no Azure SQL e fotos no Blob Storage.
- Documentação de arquitetura, banco, API, instalação, implantação e matriz do manual.

Os aplicativos exigem conexão com o servidor. Consultam os dados ao abrir cada seção; não há edição offline nem atualização por push. O aplicativo não processa pagamentos: registra intenções, cuja confirmação cabe à administração. Recomendações são similaridade matemática de características, não um modelo treinado nem garantia de compatibilidade.

## Estrutura

| Caminho | Finalidade |
|---|---|
| `src/CoracaoAnimal.Core` | Entidades, contratos e regras de negócio |
| `src/CoracaoAnimal.Api` | Aplicação, persistência, autenticação e site |
| `src/CoracaoAnimal.Client` | Cliente HTTP compartilhado pelos apps |
| `src/CoracaoAnimal.Windows` | Interface Windows |
| `src/CoracaoAnimal.App` | Interface Android |
| `database` | Modelo físico, índices, procedures, trigger e dados opcionais |
| `infra` | Recursos Azure declarados em Bicep |
| `scripts` | Execução, compilação e implantação |
| `tests` | Testes automatizados da API e cliente |
| `docs` | Documentação técnica e acadêmica de apoio |

## Compilar e testar

```powershell
dotnet test tests/CoracaoAnimal.Tests -c Release
./scripts/build-windows.ps1
dotnet workload install maui-android
./scripts/build-android.ps1
```

Para Android, instale também JDK 21 e Android SDK Platform 36/Build Tools 36.0.0. O Visual Studio com ferramentas Android pode gerenciar esses componentes. O build Windows pode ser gerado em outro sistema, mas a execução deve ser validada em Windows.

## Implantar no Azure

Leia **`docs/AZURE.md`**. Com Azure CLI autenticado na assinatura desejada:

```powershell
./scripts/deploy-azure.ps1 -ResourceGroup rg-coracaoanimal -Location brazilsouth -Prefix coracao -AdminEmail seu-email@exemplo.com
```

O script apresenta a assinatura e a previsão de recursos antes da criação. Há cobrança pelos serviços. Região, disponibilidade, quotas e valores devem ser conferidos na assinatura utilizada. Não envie senhas ou chaves pelo chat. Após validar o login inicial, execute o script de finalização indicado na documentação.

## Guias

- `docs/ARQUITETURA.md`: módulos, decisões e segurança.
- `docs/BANCO.md`: MER, tabelas, invariantes e programação SQL.
- `docs/API.md`: rotas e contratos.
- `docs/AZURE.md`: implantação, operação e CI/CD.
- `docs/PIM-IV.md`: rastreabilidade do manual e planejamento acadêmico.
- `docs/VALIDACAO.md`: evidências e validações pendentes.
- `docs/ORIGEM.md`: relação com o projeto original.

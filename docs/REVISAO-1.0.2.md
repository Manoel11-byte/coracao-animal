# Revisão 1.0.2

## Correções

O elemento `.animal-foto` passou a ter `display:block`, altura fixa e flex-shrink desabilitado. Antes era um link inline: a imagem com altura percentual não respeitava a área da fotografia.

O canvas 300x150 esticado por CSS foi substituído por dois gráficos SVG responsivos, um de quantidade de adoções concluídas e outro de valor de doações confirmadas. Cada um tem unidade, escala, meses, estado vazio e tabela acessível. O ano é informado pela API; doações são agrupadas pela criação do registro, conforme o modelo atual, e não pela confirmação. As consultas excluem datas de anos futuros.

As correções ficam em arquivos de integração separados e são preservadas pelo importador do site original. O catálogo e os indicadores são carregados independentemente e oferecem nova tentativa após falha.

O cliente compartilhado por Android/Windows trata sessão expirada, JSON incompatível e respostas de erro fora do formato esperado. Valores monetários aceitam vírgula ou ponto decimal; entradas ambíguas são rejeitadas em vez de multiplicar silenciosamente uma intenção de doação. Os apps não repetem automaticamente operações de escrita.

Windows exibe fotos no catálogo, ajusta campos ao espaço disponível e libera os controles da seção anterior. Ambos os apps encaminham sessões ausentes/expiradas à tela de login. Nenhum painel administrativo foi adicionado.

A revisão também fixa System.Security.Cryptography.Pkcs em 10.0.13, corrigida segundo https://github.com/advisories/GHSA-846f-f764-76mx, e usa runtime 10.0.13 na publicação Windows.

## Validação

- 37 casos .NET aprovados em ambiente local, incluindo o cliente HTTP real compartilhado, conta/histórico web-app, separadores monetários, mensagens de erro, sessão e séries mensais.
- Roteiro DOM com API/SQLite real aprovado para cadastro, login, perfil, adoção, doação, voluntariado e cadastro administrativo de animal.
- A API publicada respondeu HTTP 200 em /health/ready durante a revisão.
- O pipeline contém teste visual Chromium em 1440, 390 e 320 pixels; verificar o resultado da execução e seus artefatos antes de afirmar aprovação visual.
- Compilar pacotes não substitui instalá-los e executá-los em dispositivos Android e Windows físicos.

## Publicação

A alteração no GitHub aciona a compilação e o empacotamento. Isso não atualiza automaticamente o Azure: a implantação permanece no fluxo separado, com acesso autenticado à assinatura. Os pacotes antigos da release v1.0.1 permanecem antigos até nova distribuição.

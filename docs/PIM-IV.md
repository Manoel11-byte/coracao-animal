# Rastreabilidade e planejamento do PIM IV

Base: Manual do PIM IV — ADS — Versão 2, fornecido pelo usuário. O manual estabelece continuidade do PIM III, desenvolvimento Web/.NET e Mobile, APIs, banco, arquitetura, inclusão, plano Cloud/DevOps e gerenciamento ágil. Windows e implantação efetiva no Azure são acréscimos solicitados pelo usuário; a administração fica somente no site.

## Matriz do manual

| Etapa | Evidência disponível | Complementação pelo grupo |
|---|---|---|
| 1 — Organização | Contexto Coração Animal, fluxos de adoção/doação/voluntariado | Confirmar estrutura, processos e problemas com o PIM III final |
| 2 — Empreendedorismo | Proposta de centralização e atendimento multiplataforma | Canvas, viabilidade, custos pesquisados e proposta de valor fundamentada |
| 3 — Inclusão | Critérios de interface e ausência de atributos discriminatórios no cálculo | Pesquisa bibliográfica, avaliação com usuários e ações institucionais |
| 4 — Web/.NET | API ASP.NET, site, autenticação e administração | Evidências de uso e avaliação |
| 5 — Mobile | App Android integrado, navegação, sessão, atualização por consulta | Capturas e teste no dispositivo |
| 6 — Arquitetura | Core/Application/Infrastructure/Interfaces | Diagramas e justificativa apoiada nas disciplinas |
| 7 — Banco | MER, modelo físico, checks, índices, trigger e procedures | Executar e evidenciar no SQL Server/Azure SQL; medir consultas |
| 8 — Cloud/DevOps | Bicep, Dockerfile, scripts, CI/CD, logs | Implantar na assinatura, validar execução e custos |
| 9 — Ágil | Backlog e sprints propostos abaixo | Responsáveis, datas, reuniões e resultados reais |

A documentação técnica é material de apoio, não um relatório acadêmico final de 20–30 páginas de desenvolvimento. O grupo deve redigir e revisar a argumentação, inserir referências consultadas, identificar integrantes/RA/orientador e incluir evidências reais. Não registrar reuniões, testes ou implantação que não ocorreram.

## Product Backlog

| ID | História | Prioridade | Aceite |
|---|---|---|---|
| PB01 | Visitante consulta animais | Alta | Filtros e detalhes refletem o banco |
| PB02 | Pessoa cria conta e entra | Alta | Senha protegida e dados isolados |
| PB03 | Adotante completa perfil | Alta | Validação de CPF e campos obrigatórios da adoção |
| PB04 | Adotante solicita e acompanha adoção | Alta | Uma solicitação ativa por animal, estados consistentes |
| PB05 | Equipe administra pelo site | Alta | Usuário comum e sessão app recebem acesso negado |
| PB06 | Pessoa registra intenção de doação | Média | Permanece pendente até conferência |
| PB07 | Pessoa informa voluntariado | Média | Inscrição única, alteração e desativação |
| PB08 | Pessoa recebe sugestões | Média | Similaridade explicável conforme perfil |
| PB09 | Usuário realiza fluxos no Android | Alta | Teste integrado em aparelho, sem painel administrativo |
| PB10 | Usuário realiza fluxos no Windows | Alta | EXE executa os fluxos, sem painel administrativo |
| PB11 | Operação publica no Azure | Alta | HTTPS, DB central, fotos e segredos funcionando |
| PB12 | Grupo documenta e apresenta | Alta | Matriz completa, evidências, referências e formatação |

## Sprints propostas

Cronograma relativo, sem simular datas ou encontros já realizados. Ajustar à data de entrega.

| Sprint | Duração sugerida | Sprint Backlog | Entrega/aceite |
|---|---|---|---|
| 1 | Semana 1 | PB01–03, modelo de banco | Login, catálogo e perfil testados |
| 2 | Semana 2 | PB04–08 | Fluxos completos, administração e testes |
| 3 | Semana 3 | PB09–10, acessibilidade | APK/EXE e validação nos dispositivos |
| 4 | Semana 4 | PB11–12 | Implantação, demonstração e relatório revisado |

## Kanban da entrega técnica

| Concluído em código | A validar em ambiente de destino | Dependência externa |
|---|---|---|
| API, site, cliente compartilhado | Interfaces e instalação nativa Android/Windows | Aparelho Android e PC Windows |
| Banco, scripts e infraestrutura | Procedures/triggers no SQL Server e implantação | Assinatura Azure autenticada |
| Casos automatizados de integração | Acessibilidade assistiva e teste de restauração | Participantes para validação |
| Guias e backlog proposto | Relatório acadêmico final | Dados do grupo, prazo e bibliografia |

Definition of Done proposta: comportamento implementado, regra crítica testada, interface revisada, documentação atualizada e aceite no dispositivo/ambiente correspondente. “Compilado” não substitui “validado no dispositivo”.

## Roteiro da demonstração

1. Apresentar problema, organização, proposta de valor e arquitetura.
2. Administrador cadastra animal e foto pelo site.
3. Adotante cria conta e completa perfil.
4. Solicita adoção no Android ou Windows e acompanha pelo outro cliente.
5. Administração aprova/conclui pelo site e o usuário atualiza o histórico.
6. Demonstrar doação pendente e confirmação administrativa, voluntariado e afinidade.
7. Mostrar MER, uma procedure e trigger no SQL Server/Azure SQL.
8. Mostrar pipeline, configuração de nuvem, evidências e limitações.

## Estrutura acadêmica

O manual solicita capa, resumo, abstract, sumário, introdução, desenvolvimento (20–30 páginas), conclusão e referências. Organizar os capítulos pelas nove etapas e relacionar decisões ao conteúdo das disciplinas. A formatação e bibliografia devem ser revisadas conforme as orientações institucionais fornecidas ao grupo.

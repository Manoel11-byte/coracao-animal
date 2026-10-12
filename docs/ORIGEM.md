# Continuidade do projeto fornecido

O código deriva do domínio e do material de `coracao-animal-main.zip`: catálogo e gestão de animais, adotantes, adoções, doações, voluntários e recomendações. O ZIP original foi preservado. Esta entrega é uma nova versão separada para o PIM IV; não é migração automática de registros existentes.

O site foi reconstruído com uma API única e controle de acesso no servidor. O logo e a imagem ilustrativa do labrador vieram de `frontend/img/logo-coracao-animal.png` e `frontend/img/labrador_imagem.jpg` do projeto fornecido. A titularidade/licença dessas imagens não foi verificada externamente; o grupo deve preservar suas referências e confirmar o uso antes de publicação pública.

A recomendação por similaridade foi reimplementada em C# como regra compartilhada, sem servidor Python separado. A nova estrutura incorpora autenticação, sessões, consistência transacional, apps nativos e infraestrutura Azure. Senhas, conexões locais e dados de demonstração do projeto anterior não foram copiados para a entrega.

O esquema SQL antigo não é substituído in-place. Havendo dados reais a preservar, é necessário mapear campos, limpar duplicidades, revalidar CPFs e definir a migração e a redefinição segura das senhas antes de importar. Essa migração não foi executada.

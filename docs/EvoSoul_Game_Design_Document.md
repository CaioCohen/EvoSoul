# EvoSoul — Documento Geral de Game Design

> **Tipo de documento:** visão e regras de jogo (não técnico)  
> **Estado:** primeira versão consolidada  
> **Plataforma-alvo:** dispositivos móveis, inicialmente Android  
> **Apresentação:** 3D low poly, câmera em terceira pessoa  
> **Escopo:** conceitos confirmados e pontos intencionalmente abertos para decisões futuras

---

## 1. Visão geral

**EvoSoul** é um jogo de evolução de criaturas em terceira pessoa. O jogador começa como uma pequena **gota cinza**, ainda sem alinhamento ou identidade evolutiva. Ao explorar o ambiente e obter energia por meio de ações positivas ou negativas, transforma-se progressivamente em seres mais complexos, até atingir uma forma próxima à de uma divindade.

O princípio central é: **as ações do jogador moldam o próximo corpo que ele terá**. Não há seleção inicial de classe ou linhagem. As escolhas feitas durante cada estágio determinam a próxima evolução.

A experiência deve começar simples e crescer com o personagem. As possibilidades de interação e a complexidade do combate poderão se expandir nos estágios avançados, mas não precisam estar todas definidas na primeira versão.

### Pilares do jogo

1. **Evolução por comportamento:** consumir energia positiva ou negativa determina a trajetória evolutiva.
2. **Liberdade para ajustar a trajetória:** atingir o limite de energia habilita, mas não obriga, a evolução.
3. **Três linhagens distintas:** Celestial, Demoníaca e Dracônica.
4. **Progressão de ambientes:** cada estágio apresenta um novo mapa, de acordo com a linhagem atual.
5. **Escalada biológica e fantástica:** a criatura parte de uma forma extremamente simples e termina como uma entidade divina.
6. **Escopo controlado:** mapas podem compartilhar estruturas de exploração e sistemas de interação, mesmo quando seus visuais e atividades diferirem.

## 2. Experiência do jogador

- **Câmera:** terceira pessoa, acompanhando a criatura.
- **Exploração:** o jogador percorre o mapa, encontra fontes de energia e interage com elementos do ambiente ou outras criaturas.
- **Interação inicial:** ações simples, como atingir uma árvore, recolher/consumir frutas e atacar outras gotas.
- **Combate:** começa simples; sua evolução mecânica nos estágios posteriores será definida depois.
- **Progressão:** cada etapa possui uma quantidade de energia a obter e culmina em uma evolução escolhida pelo jogador.
- **Interface:** o acompanhamento da energia e da proporção de alinhamento acontece internamente, sem barras de progresso visíveis. Ao completar o limite do estágio, surge a opção **Evoluir**.

A aparência final da interface, os controles de toque, a apresentação de feedback durante a coleta e a forma de comunicar a tendência de alinhamento ainda serão definidos.

## 3. Energia e alinhamento

Existem dois tipos de energia:

- **Energia positiva:** obtida por uma ação positiva específica do mapa, como consumir frutos no cenário inicial.
- **Energia negativa:** obtida por uma ação negativa específica do mapa, como atacar e devorar uma criatura da própria espécie no cenário inicial.

Cada mapa oferecerá **duas formas principais de obter energia**, uma de cada tipo. Nos estágios posteriores, essas atividades mudarão conforme o estágio **e** a linhagem do jogador.

### 3.1. Progresso pertence ao estágio, não ao jogo inteiro

Cada estágio tem sua **própria meta de energia**. Os números 10, 50, 200, 1.000, 5.000 e 10.000 representam o que deve ser obtido **dentro de cada estágio** — não metas cumulativas desde o começo da partida.

A energia positiva e negativa utilizada para definir a evolução também considera **apenas o estágio atual**. Ao entrar em um novo estágio, essa contagem recomeça do zero. Energias de etapas anteriores não entram na proporção da nova evolução.

| Estágio | Energia a obter neste estágio | Evolução ao concluí-lo |
|---|---:|---|
| 1 — Inicial | 10 | Primeira forma da linhagem escolhida |
| 2 | 50 | Segunda forma |
| 3 | 200 | Terceira forma |
| 4 | 1.000 | Quarta forma |
| 5 | 5.000 | Quinta forma |
| 6 | 10.000 | Sexta forma: ascensão divina |

### 3.2. Determinação do alinhamento

Ao atingir o limite de energia do estágio, considera-se a proporção entre os dois tipos consumidos **naquele estágio**:

| Distribuição | Tendência resultante |
|---|---|
| **71% ou mais de energia positiva** | Positiva / Celestial |
| **71% ou mais de energia negativa** | Negativa / Demoníaca |
| **Nenhum tipo atinge 71%** | Equilibrada / Dracônica |

Esse resultado representa a **tendência** do estágio. A linhagem atual também influencia qual transformação é permitida, conforme a regra de transição descrita na seção 4.

Como as energias são contadas em unidades inteiras, a porcentagem real atingível depende da meta do estágio. Exemplo: com 10 energias, 7 positivas representam 70% (equilíbrio); 8 positivas representam 80% (tendência positiva).

### 3.3. Evolução voluntária

Chegar ao limite **não evolui automaticamente** o jogador.

1. O jogador atinge a meta de energia do estágio.
2. A opção **Evoluir** aparece na tela.
3. O jogador pode confirmar a evolução imediatamente **ou continuar realizando ações que concedem energia**.
4. Enquanto a evolução não for confirmada, o total de energias do estágio permanece fixado no limite. As novas energias passam a **substituir energias do tipo oposto**.
5. Ao selecionar **Evoluir**, a distribuição atual é utilizada para definir a próxima forma, respeitando as restrições de transição entre linhagens.

**Regra de substituição após o limite:** ao obter 1 energia positiva, 1 energia negativa existente é convertida em positiva; ao obter 1 negativa, 1 positiva existente é convertida em negativa. Se não houver energia do tipo oposto, a ação não altera a contagem. A energia do estágio nunca ultrapassa seu limite.

**Exemplo, no estágio inicial:** o jogador atinge 10 energias com 8 negativas e 2 positivas. A tendência é Demoníaca. Antes de evoluir, consome uma fruta: fica com 7 negativas e 3 positivas, mantendo o total em 10, agora com tendência Dracônica. Pode continuar consumindo frutas até atingir uma tendência Celestial, se desejar.

Este ajuste é possível **somente antes de confirmar a evolução**. Confirmada a transformação, encerra-se a obtenção de energia naquele mapa.

## 4. Linhagens evolutivas e transições

### 4.1. As três linhagens

- **Demoníaca — predação e dominação:** a criatura desenvolve chifres, presas, garras, traços infernais, asas e, ao fim, uma aparência divina maligna.
- **Celestial — vitalidade e harmonia:** a criatura adquire formas luminosas e aladas, tornando-se gradualmente uma entidade angelical e, por fim, uma divindade celestial.
- **Dracônica — equilíbrio e adaptação:** a criatura segue um desenvolvimento reptiliano/dracônico, ganhando escamas, membros, asas e múltiplas cabeças, culminando em um deus dragão.

As três linhagens devem parecer partes de um mesmo processo evolutivo, mas com silhuetas e identidades visuais claramente distintas.

### 4.2. Regra de transição entre caminhos

**Não é permitido passar diretamente de Celestial para Demoníaco, nem de Demoníaco para Celestial.** O caminho Dracônico é a passagem intermediária obrigatória.

Para conciliar essa restrição com a regra original de que uma linhagem estabelecida só pode ser abandonada após consumir **71% ou mais da energia do tipo oposto**, o comportamento fica assim:

| Linhagem atual | Energia do estágio | Próxima linhagem |
|---|---|---|
| Gota inicial | 71%+ positiva | Celestial |
| Gota inicial | 71%+ negativa | Demoníaca |
| Gota inicial | Equilibrada | Dracônica |
| Celestial | 71%+ negativa | **Dracônica** |
| Celestial | Qualquer outra distribuição | **Celestial** |
| Demoníaca | 71%+ positiva | **Dracônica** |
| Demoníaca | Qualquer outra distribuição | **Demoníaca** |
| Dracônica | 71%+ positiva | **Celestial** |
| Dracônica | 71%+ negativa | **Demoníaca** |
| Dracônica | Equilibrada | **Dracônica** |

**Exemplo de troca completa:** um Celestial precisa concluir um estágio com pelo menos 71% de energia negativa para evoluir em Dracônico. Em um **estágio posterior**, se continuar obtendo pelo menos 71% de energia negativa, poderá evoluir para Demoníaco. O mesmo vale no sentido inverso.

Isso impede a troca direta entre os dois extremos e faz do Dracônico uma linhagem própria, além de uma ponte entre eles.

## 5. Ciclo de jogo por estágio

1. **Explorar** o mapa com câmera em terceira pessoa.
2. **Interagir** com suas duas fontes principais de energia: uma positiva e uma negativa.
3. **Acumular** energia até a meta específica do estágio.
4. **Ajustar**, se quiser, a distribuição positiva/negativa após atingir o limite, substituindo energias do tipo oposto.
5. **Evoluir**, quando decidir, para a forma permitida pela distribuição e pela linhagem atual.
6. **Avançar**: após evoluir, surge no mapa a opção de seguir para o próximo estágio, em um mapa correspondente à nova linhagem.

Depois que o personagem evolui, **não é mais possível obter energia no mapa que acabou de concluir**, mesmo que permaneça nele por algum tempo antes de avançar.

**Exceção ao avanço:** a transformação do estágio 6 é a ascensão final. Não há um sétimo estágio ou novo mapa planejado neste documento.

## 6. Estrutura dos mapas

O jogo terá **16 mapas associados à progressão principal**:

- **1 mapa inicial compartilhado**, onde todos começam como gota cinza.
- **5 mapas de progressão para cada linhagem** (Celestial, Demoníaca e Dracônica), correspondentes aos estágios 2 a 6.

| Estágio | Mapa inicial | Mapa Celestial | Mapa Demoníaco | Mapa Dracônico |
|---|---|---|---|---|
| 1 | Zona inicial de grama e árvores | — | — | — |
| 2 | — | A definir | A definir | A definir |
| 3 | — | A definir | A definir | A definir |
| 4 | — | A definir | A definir | A definir |
| 5 | — | A definir | A definir | A definir |
| 6 | — | A definir | A definir | A definir |

### 6.1. Mapa inicial — zona de grama

**Ambiente:** campo gramado com árvores e várias outras criaturas iguais à gota cinza do jogador.

**Fonte positiva de energia:** o jogador atinge uma árvore; uma fruta cai; ao consumi-la, obtém **1 energia positiva**.

**Fonte negativa de energia:** o jogador ataca outra criatura da mesma espécie; ao consumir seu corpo, obtém **1 energia negativa**.

O mapa inicial apresenta o princípio do jogo de maneira concreta: alimentar-se de frutos e predar semelhantes são ações diferentes que constroem trajetórias evolutivas diferentes.

### 6.2. Mapas posteriores

A partir do segundo estágio:

- Cada linhagem possui seu próprio mapa para aquele estágio.
- Cada mapa sempre possui uma **atividade positiva** e uma **atividade negativa** de obtenção de energia.
- As atividades variam **tanto pelo estágio quanto pela linhagem**, em vez de apenas trocar a aparência dos objetos.
- O jogador deve poder praticar os dois tipos de ação **em qualquer linhagem**, inclusive ações negativas em mapas Celestiais e positivas em mapas Demoníacos. Essa liberdade é necessária para permitir mudanças de trajetória.
- As propostas concretas de biomas, criaturas, objetivos locais e fontes de energia de cada um dos 15 mapas posteriores **ainda não foram aprovadas** e serão detalhadas separadamente.

### 6.3. Reutilização de estruturas

Os 15 mapas posteriores podem compartilhar estruturas básicas de exploração e interação, adaptadas por estágio e linhagem. Isso reduz o trabalho de criação sem exigir que os mapas tenham a mesma aparência ou exatamente as mesmas atividades.

A intenção é reaproveitar fundamentos, **não** transformar os mapas em simples mudanças de cor.

## 7. Formas evolutivas e direção visual

O plano contempla **19 formas principais**: **1 gota inicial + 6 formas Celestiais + 6 Demoníacas + 6 Dracônicas**.

Essas são as formas exibidas **após a conclusão** de cada estágio. Durante um estágio, a criatura mantém a forma conquistada na evolução anterior. **Não serão criadas, por enquanto, variantes visuais intermediárias** para o começo, meio ou fim de cada estágio.

As descrições a seguir são **briefings de aparência**, destinados a orientar a modelagem futura. Não são modelos prontos e não definem ainda especificações técnicas de malha, rig ou animação.

### 7.1. Forma inicial — Gota Cinza

Uma gota cinza simples, de corpo arredondado com topo afunilado, sem marcas celestiais, demoníacas ou dracônicas. Deve parecer uma criatura primordial e neutra, visualmente frágil e muito mais simples do que suas futuras formas. Seu design precisa permitir que o jogador a reconheça como a origem das três linhagens.

### 7.2. Linha Demoníaca

| Evolução | Nome provisório | Descrição visual |
|---|---|---|
| 1 — após 10 | **Lesma Infernal** | Slime alongado, menos semelhante a uma gota; corpo rastejante com cabeça definida, dois chifres pequenos e boca capaz de morder. Primeiros traços agressivos. |
| 2 — após 50 | **Diabrete** | Pequeno imp bípede; braços e pernas definidos, chifres mais evidentes, expressão travessa e uma lança simples nas mãos. |
| 3 — após 200 | **Demônio Bestial** | Criatura maior e robusta, com silhueta mais animalesca, musculatura evidente, garras, presas e grandes chifres. Deve transmitir força bruta. |
| 4 — após 1.000 | **Diabo Alado** | Forma humanoide demoníaca, ainda com traços bestiais: garras, presas, chifres e asas de aparência membranosa. Postura mais ereta e imponente. |
| 5 — após 5.000 | **Rei Diabo** | Demônio de aspecto régio, com coroa, vestes de realeza e uma pequena aura infernal. Mantém características reconhecíveis do diabo anterior. |
| 6 — após 10.000 | **Deus Diabo** | Entidade divina maligna, majestosa, com **três pares de asas**, visual sobrenatural e símbolos de autoridade infernal. Forma final da linhagem. |

**Progressão visual:** o corpo parte de uma massa orgânica predatória, adquire membros e se aproxima de uma anatomia humanoide, passando de criatura bestial a soberano e divindade infernal.

### 7.3. Linha Celestial

| Evolução | Nome provisório | Descrição visual |
|---|---|---|
| 1 — após 10 | **Gota Luminosa** | Evolução da gota primordial com silhueta mais delicada, tons claros, brilho discreto e pequenos indícios de asas ou penas. |
| 2 — após 50 | **Querubim** | Pequena criatura angelical com corpo e membros mais definidos, asas emplumadas curtas e aparência juvenil. |
| 3 — após 200 | **Guardião Celestial** | Ser semi-humanoide, mais alto, com asas maiores, postura protetora e detalhes que lembram proteção ou armadura leve. |
| 4 — após 1.000 | **Anjo Guerreiro** | Figura celestial humanoide com asas amplas e presença mais solene; pode apresentar vestimentas ou armadura de aparência sagrada. |
| 5 — após 5.000 | **Soberano Celestial** | Entidade angelical de realeza, com coroa ou halo elaborado, vestes nobres e aura luminosa; equivalente de poder ao Rei Diabo. |
| 6 — após 10.000 | **Deus Celestial** | Divindade angelical com múltiplos pares de asas, halo destacado e aparência transcendente, mantendo uma silhueta legível em low poly. |

**Progressão visual:** começa com uma pequena criatura luminosa, ganha anatomia e penas, torna-se um ser alado protetor e culmina em uma divindade celestial.

### 7.4. Linha Dracônica

| Evolução | Nome provisório | Descrição visual |
|---|---|---|
| 1 — após 10 | **Salamandra Escamada** | Pequeno corpo alongado, baixo e reptiliano; cauda evidente, cabeça simples e primeiros indícios de escamas. |
| 2 — após 50 | **Lagarto Dracônico** | Réptil maior, com patas mais desenvolvidas, focinho alongado, pequenas protuberâncias ósseas e postura mais ágil. |
| 3 — após 200 | **Draco Jovem** | Criatura quadrúpede de aspecto claramente dracônico, com cauda longa, chifres, garras e estrutura corporal mais poderosa. |
| 4 — após 1.000 | **Dragão Alado** | Dragão completo com grandes asas, corpo robusto, pescoço alongado e silhueta imediatamente reconhecível. |
| 5 — após 5.000 | **Dragão Ancestral** | Dragão monumental com **três cabeças**, asas desenvolvidas e aparência antiga e majestosa. |
| 6 — após 10.000 | **Deus Dragão** | Divindade dracônica de **múltiplas cabeças**, porte extraordinário e presença sobrenatural. Suas cabeças e sua forma devem comunicar a ideia de equilíbrio entre forças distintas. |

**Progressão visual:** segue um eixo predominantemente biológico — anfíbio/reptiliano, lagarto, draco, dragão e entidade dracônica de múltiplas cabeças — sem exigir uma transformação humanoide.

### 7.5. Diretrizes artísticas comuns

- **Estética geral:** 3D low poly, adequada a dispositivos móveis.
- **Leitura à distância:** cada evolução precisa ter silhueta reconhecível na câmera em terceira pessoa.
- **Continuidade biológica:** as formas de uma mesma linhagem preservam traços reconhecíveis de suas predecessoras, em vez de parecerem personagens completamente desconectados.
- **Escala de poder perceptível:** as silhuetas ganham presença e complexidade conforme avançam os estágios, sem depender de excesso de detalhes.
- **Linhagens visualmente distintas:** chifres e anatomia infernal, penas e aspectos angelicais, escamas e anatomia dracônica são sinais principais.
- **Cores, proporções, materiais e elementos finos:** serão refinados na etapa de direção de arte/modelagem com Blender.

## 8. Progressão e conclusão

- As **evoluções 1 a 5** habilitam a opção de avançar para um novo mapa da linhagem resultante.
- A **evolução 6**, após adquirir **10.000 energias naquele estágio**, representa a forma divina final.
- O jogo principal não prevê, por enquanto, um mapa extra depois da ascensão divina.
- O modo de apresentar o encerramento, uma possível continuação após a evolução final e a existência de novas partidas são decisões futuras.

## 9. Escopo conscientemente adiado

As ideias abaixo **não fazem parte das decisões de implementação inicial**:

- Aparências intermediárias dentro de um mesmo estágio (por exemplo, início, meio e fim).
- Animações especiais e elaboradas de metamorfose.
- Catálogo ou coleção de formas evolutivas descobertas.
- Sistema completo de combate dos estágios avançados.
- Descrição definitiva dos 15 mapas posteriores, de seus biomas e suas atividades positivas/negativas.
- Identidade visual final de cada modelo, controles móveis detalhados, interface definitiva e narrativa de encerramento.

Esses itens poderão ser revisitados posteriormente, sem mudar as regras centrais descritas neste documento.

## 10. Resumo das decisões confirmadas

| Tema | Decisão |
|---|---|
| Gênero central | Evolução e exploração de criatura em 3D |
| Plataforma | Mobile, inicialmente Android |
| Câmera | Terceira pessoa |
| Visual | Low poly |
| Forma inicial | Gota cinza neutra |
| Linhagens | Celestial, Demoníaca e Dracônica |
| Estágios | 6, incluindo o inicial |
| Mapas | 16: 1 inicial + 5 por linhagem |
| Formas principais | 19: 1 inicial + 6 por linhagem |
| Energia necessária **em cada estágio** | 10, 50, 200, 1.000, 5.000 e 10.000 |
| Alinhamento | Calculado apenas com energias obtidas no estágio atual |
| Limite para tendência forte | 71% ou mais de um tipo |
| Evolução automática | Não; o jogador decide quando evoluir |
| Energia após atingir a meta | Substitui, unidade por unidade, energia do tipo oposto; o total fica fixo |
| Mudança Celestial ↔ Demoníaca | Somente passando pela linha Dracônica em uma evolução intermediária |
| Fontes de energia | Uma positiva e uma negativa por mapa, variando entre estágios e linhagens |
| Depois de evoluir | É possível avançar; o mapa já concluído não gera mais energia |
| Combate inicial | Simples; complexidade posterior ainda aberta |
| Reutilização de mapas | Compartilhar fundamentos e estruturas, preservando variações de cada caminho |

---

*Este documento registra o design geral do EvoSoul. Seu objetivo é servir como referência criativa única antes da escrita de especificações técnicas e da produção de modelos no Blender. Os nomes de formas, com exceção dos conceitos explicitamente definidos, são provisórios.*

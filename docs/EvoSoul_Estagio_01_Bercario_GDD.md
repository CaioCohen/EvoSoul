# EvoSoul — Estágio 1: Jardim-Berçário

> **Documento:** game design não técnico de uma fase  
> **Status:** especificação inicial para implementação e testes  
> **Abrangência:** primeiro mapa, compartilhado pelas três linhagens  
> **Plataforma:** mobile, orientação horizontal, câmera em terceira pessoa  
> **Referência:** `EvoSoul_Game_Design_Document.md` (regras gerais de progressão e evolução)

## 1. Conceito e objetivo

O primeiro estágio é um **jardim que funciona como berçário de criaturas recém-nascidas**. Nele, o jogador controla uma **gota cinza** (Virgin Drop), sem alinhamento evolutivo definido, e explora um ambiente pacífico à primeira vista. Outras gotas semelhantes circulam lentamente pelo jardim.

A fase apresenta uma escolha simples: **alimentar-se de frutos** ou **atacar outras gotas**. A primeira ação concede energia positiva; a segunda concede energia negativa. A proporção entre as energias obtidas nesse estágio define a primeira linhagem evolutiva do personagem: Celestial, Demoníaca ou Dracônica.

A intenção é que o jogador entenda essa relação pelo que faz no mundo, sem precisar receber uma explicação longa ou enfrentar combates complexos.

## 2. Identidade visual do jardim

### 2.1. Direção artística

- Visual **3D low-poly**, com formas simples, silhuetas claras e poucos detalhes.
- Cores vivas e agradáveis, com predominância de verdes, flores e vegetação colorida.
- Atmosfera acolhedora de jardim/berçário, não de floresta hostil ou arena de combate.
- Terreno gramado com relevo discreto, arbustos frutíferos e árvores que dão volume ao cenário.
- Elementos distribuídos de forma a favorecer a exploração e a leitura dos alvos à altura da pequena gota.
- Iluminação clara e suave. As frutas, os NPCs e o jogador devem se distinguir visualmente do ambiente.

Os arbustos e árvores podem reutilizar modelos e elementos de composição para manter consistência e viabilizar a produção. O mapa deve parecer um lugar habitado pelas gotas, e não apenas um terreno vazio com objetos interativos.

### 2.2. Elementos do cenário

| Elemento | Papel no estágio |
|---|---|
| Grama e terreno | Espaço de locomoção e exploração |
| Arbustos frutíferos | Identificam os pontos onde o jogador encontra energia positiva |
| Frutos | Alvos consumíveis, que concedem energia positiva |
| Árvores, flores e outras plantas | Ambientação e variedade visual |
| Outras gotas cinzas | Habitantes do berçário e alvos de ataque |

**Em aberto:** desenho exato do mapa, quantidade de plantas e NPCs, tipos de fruta, pontos de reaparecimento e eventuais limites físicos do jardim. Não estabelecer números arbitrários como requisito definitivo.

## 3. Personagens e comportamento

### 3.1. Jogador — Virgin Drop

- O jogador começa sempre com a forma de **gota cinza**, já modelada para o projeto.
- A experiência é vista por uma **câmera em terceira pessoa**, acompanhando a criatura.
- Pode se deslocar pelo jardim e orientar-se na direção de objetos ou criaturas.
- Usa, nesta fase, ações simples: **comer fruto** e **atacar outra gota**.
- As animações existentes da gota (movimento, ataque e morte) devem orientar a apresentação das ações, sem exigir novas transformações intermediárias.

### 3.2. Gotas NPCs

- Possuem aparência equivalente à gota cinza inicial, permitindo reutilizar seu visual.
- **Vagam lentamente e sem um destino aparente** por áreas acessíveis do jardim.
- Devem ser visivelmente **mais lentas que o jogador**.
- Seu movimento deve parecer casual e tranquilo, como o de criaturas que vivem no berçário.
- Funcionam como alvos para a interação **Atacar**.
- Ao serem derrotadas, deixam de se mover e passam pela sequência de morte descrita adiante.

Não está definido, nesta primeira versão, que os NPCs ataquem, persigam ou revidem. A fase pode ser implementada sem comportamento de combate defensivo.

## 4. Exploração e interação contextual

### 4.1. Regra geral

O jogador **não vê comandos de interação permanentemente para todos os objetos**. A opção adequada só aparece quando existe um alvo elegível:

1. O jogador está **próximo o suficiente** do alvo.
2. O jogador está **voltado para a direção do alvo**.
3. O alvo continua disponível para aquela ação.

Estar apenas próximo não basta: a orientação do personagem é parte da escolha do alvo. A interface deve tornar perceptível com qual objeto ou criatura o jogador está prestes a interagir.

A distância exata, o ângulo de orientação e os controles de toque serão calibrados durante os testes; não constituem números fixados por este documento.

### 4.2. Frutos: ação **Comer**

- O jogador encontra frutas associadas aos arbustos frutíferos do jardim.
- Ao aproximar-se de uma fruta **e apontar na direção dela**, surge a opção **Comer**.
- Ao confirmar, a gota consome o fruto.
- O fruto consumido deixa de estar disponível naquele momento.
- O jogador recebe **1 ponto de energia positiva** (observadas as regras de limite e substituição do estágio).
- A interface de depuração atualiza imediatamente a contagem.

**Não é obrigatório atacar árvores ou arbustos para liberar as frutas nesta versão.** A interação diretamente com um fruto próximo é suficiente para a primeira implementação. A necessidade de colher, derrubar ou regenerar frutos poderá ser definida depois.

### 4.3. Outras gotas: ação **Atacar**

- Ao aproximar-se de uma gota NPC **e apontar na direção dela**, surge a opção **Atacar**, em vez de **Comer**.
- Ao confirmar, o jogador executa a animação de ataque.
- O NPC atingido inicia a animação de morte.
- Com a derrota efetiva da gota NPC, o jogador recebe **1 ponto de energia negativa**, conforme as regras de energia do estágio.
- O NPC derrotado não pode ser atacado novamente para gerar energia adicional.

Nesta primeira implementação, **o ataque bem-sucedido é suficiente para conceder a energia negativa**; não é exigida uma segunda interação para consumir o corpo. Isso simplifica o ciclo inicial de gameplay. A eventual separação entre derrotar e consumir pode ser revista futuramente.

### 4.4. Quando há vários alvos

Se houver uma fruta e uma gota próximas, deve aparecer **uma ação contextual para o alvo efetivamente escolhido pela direção do jogador**, e não dois comandos conflitantes. A indicação visual de qual alvo está selecionado pode ser discreta; sua forma definitiva ainda será escolhida.

## 5. Sequência de morte de uma gota NPC

A morte deve ser legível, coerente com o corpo gelatinoso da criatura e sem violência gráfica.

1. **Impacto:** o ataque do jogador atinge a gota NPC.
2. **Dissolução:** a gota reproduz a animação de morte existente, perdendo a forma e espalhando-se pelo chão.
3. **Poça:** ao chegar ao último quadro da animação, a criatura **permanece como uma poça**, sem retornar à forma original.
4. **Permanência:** a poça permanece visível por **alguns segundos**.
5. **Desaparecimento:** a poça desaparece gradualmente por **fade-out**.
6. **Remoção:** ao término do fade-out, não permanece nenhum alvo ativo no local.

A animação de morte **não deve reiniciar ou entrar em loop**. O tempo exato de permanência e de desaparecimento deverá ser ajustado visualmente durante os testes.

## 6. Energia e progresso neste estágio

A meta do primeiro estágio é **10 pontos de energia**, obtidos exclusivamente dentro dele.

| Ação concluída | Resultado |
|---|---|
| Consumir 1 fruto | +1 energia positiva |
| Derrotar 1 gota NPC | +1 energia negativa |

O jogo mantém internamente a contagem de energia positiva, negativa e total do estágio. **A contagem do estágio, e não o histórico das etapas futuras, determina a primeira evolução.**

### 6.1. Indicador de depuração

O produto final **não deve exibir permanentemente uma barra numérica de progresso**. Entretanto, durante o desenvolvimento, haverá um **indicador de debug visível** para acompanhar, no mínimo:

- Energia positiva acumulada no estágio.
- Energia negativa acumulada no estágio.
- Total do estágio em relação ao limite de **10**.
- Tendência evolutiva atual: Celestial, Demoníaca ou Dracônica.

Esse indicador é uma ferramenta de testes e deverá poder ser ocultado ou removido das apresentações destinadas ao jogador.

### 6.2. Limite e ajuste antes da evolução

Ao obter a décima energia:

1. A energia total do estágio fica fixada em **10/10**.
2. Aparece a opção **Evoluir**.
3. O jogador **não evolui automaticamente** e pode continuar explorando.
4. Se ganhar uma energia de um tipo, **uma energia existente do tipo oposto é substituída** pela nova, mantendo o total em 10.
5. Se não houver energia do tipo oposto, a distribuição permanece inalterada.
6. A opção **Evoluir** continua disponível até ser selecionada.

**Exemplo:** com 8 energias negativas e 2 positivas, comer uma fruta transforma a distribuição em 7 negativas e 3 positivas, sem passar de 10 pontos totais.

### 6.3. Escolha da primeira linhagem

Ao confirmar **Evoluir**, a distribuição do estágio inicial determina:

- **71% ou mais positivas:** primeira forma **Celestial**.
- **71% ou mais negativas:** primeira forma **Demoníaca**.
- **Nenhum tipo com 71%:** primeira forma **Dracônica**.

Como o limite é de dez unidades, na prática **8 ou mais energias de um tipo** são necessárias para seguir a respectiva linhagem. Distribuições de 3/7 até 7/3 levam à linhagem Dracônica.

Depois da confirmação, a obtenção de energia no berçário se encerra e o jogo disponibiliza o avanço ao próximo mapa, próprio da linhagem escolhida. A implementação dos outros mapas não faz parte deste documento.

## 7. Interface e feedback

A interface do primeiro estágio deve ser discreta, permitindo enxergar o jardim e o personagem:

- **Comer** aparece apenas diante de uma fruta válida e alcançável.
- **Atacar** aparece apenas diante de uma gota NPC válida e alcançável.
- **Evoluir** aparece ao atingir o limite de 10 energias e permanece disponível enquanto não for acionado.
- O ganho de energia deve produzir algum feedback perceptível, sem exigir que o jogador veja os contadores de debug.
- O indicador de debug será visível somente durante os testes de desenvolvimento.

O layout dos botões para touchscreen, as cores dos indicadores e os efeitos de feedback ainda serão definidos. A experiência final é **mobile em orientação horizontal**, mas também precisa ser testável no Unity Editor no computador.

## 8. Ciclo de jogo esperado

1. O jogador entra no jardim como uma gota cinza.
2. Explora o ambiente com câmera em terceira pessoa, enquanto outras gotas caminham devagar.
3. Encontra um fruto, aproxima-se, aponta para ele e usa **Comer** para receber energia positiva.
4. Encontra outra gota, aproxima-se, aponta para ela e usa **Atacar** para receber energia negativa após derrotá-la.
5. A gota derrotada vira poça, permanece brevemente e desaparece por fade-out.
6. O jogador repete as interações e acompanha o progresso pelo indicador de debug.
7. Ao alcançar **10/10**, a opção **Evoluir** aparece.
8. O jogador pode continuar trocando a proporção de energias antes de confirmar.
9. Ao confirmar, evolui para a primeira forma Celestial, Demoníaca ou Dracônica e fica apto a avançar ao mapa seguinte.

## 9. Critérios de aceitação da primeira versão jogável

- [ ] O jardim low-poly está presente, com cores vivas, vegetação e arbustos frutíferos.
- [ ] O jogador controla a Virgin Drop em terceira pessoa.
- [ ] Gotas NPCs circulam sem rumo evidente e são mais lentas que o jogador.
- [ ] A opção **Comer** só aparece quando uma fruta válida está próxima e à frente do personagem.
- [ ] Consumir uma fruta concede exatamente **1 energia positiva**.
- [ ] A opção **Atacar** só aparece quando uma gota NPC válida está próxima e à frente do personagem.
- [ ] O ataque executa a animação do jogador e provoca a morte do alvo.
- [ ] Cada gota NPC derrotada concede exatamente **1 energia negativa**, sem permitir ganhos repetidos.
- [ ] O alvo executa a animação de morte até o fim, permanece como poça por alguns segundos e desaparece gradualmente.
- [ ] Os contadores de energia positiva, negativa e total podem ser vistos no modo de debug.
- [ ] Ao atingir **10/10**, o botão **Evoluir** aparece, sem evolução automática.
- [ ] Após atingir o limite, novas energias **substituem** energias opostas, sem elevar o total.
- [ ] A evolução inicial respeita a distribuição de energia e as três linhagens.
- [ ] A fase pode ser testada no Editor e foi concebida para dispositivos móveis em orientação horizontal.

## 10. Pontos intencionalmente em aberto

Estes detalhes **não devem ser tratados como decisões fechadas**:

- Layout exato e tamanho do jardim.
- Quantidade e distribuição de gotas NPCs, arbustos e frutos.
- Regra de surgimento ou reaparecimento de frutos e NPCs, necessária para manter disponíveis as formas de obter energia.
- Distância e ângulo precisos para reconhecer um alvo.
- Formato dos controles de movimento, câmera e interação na tela sensível ao toque.
- Tempo exato da poça antes do fade-out e duração do próprio fade-out.
- Feedback audiovisual de cada interação.
- Apresentação visual da evolução e da passagem ao mapa seguinte.

**Diretriz de escopo:** esta primeira versão deve provar o ciclo central de explorar → interagir → obter energia → ajustar alinhamento → evoluir. Evitar acrescentar sistemas de combate complexos, formas intermediárias ou mecânicas de estágios futuros neste momento.

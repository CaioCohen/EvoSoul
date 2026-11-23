# EvoSoul — Estágio 02: Primeiras Linhagens

**Tipo:** Documento de design de jogo (não técnico)  
**Status:** Planejamento — mecânicas centrais definidas; detalhes em aberto  
**Pré-requisito:** Conclusão do Jardim-Berçário e primeira evolução  
**Personagens jogáveis:** Gota Profana, Lumigota e Drakeling Slime

## 1. Visão geral

O Estágio 02 é o primeiro estágio com três mapas distintos, um para cada linhagem evolutiva. Após evoluir da Gota Virgem, o jogador acessa o ambiente correspondente à sua nova forma.

Cada mapa oferece atividades que alteram a distribuição de **energia positiva** e **energia negativa** do estágio. As ações devem ser simples, com interações contextuais que dependem da proximidade e da orientação do personagem. Os ambientes, os NPCs e os objetos interativos são exclusivos de suas respectivas linhagens: a proposta é incentivar novas partidas para descobrir os três caminhos.

A câmera permanece em **terceira pessoa**, acompanhando o jogador por trás. A direção visual continua sendo **3D low-poly, estilizada e otimizada para celulares**, com tela horizontal.

## 2. Progressão do estágio

- **Meta de energia neste estágio:** 50 pontos.
- **Contagem:** a energia positiva e negativa deste estágio começa do zero, independentemente da distribuição de energia do estágio anterior.
- **Determinação da próxima linhagem:** considera somente a proporção de energia obtida neste estágio, obedecendo à regra de que a transição direta entre Celestial e Demoníaco não é permitida; a passagem precisa ocorrer pela linhagem Dracônica.
- **Evolução manual:** ao chegar a 50 pontos, a opção **Evoluir** fica disponível; a transformação não acontece automaticamente.
- **Ajuste após atingir o limite:** novas energias não aumentam o total. Cada ponto obtido substitui um ponto da energia oposta, se houver, permitindo ajustar a distribuição antes de confirmar.
- **Após confirmar a evolução:** o jogador poderá seguir para o próximo estágio quando a transição de mapa estiver disponível; não continuará acumulando energia no estágio concluído.

As regras gerais de transição entre linhagens e de evolução permanecem descritas no documento geral do EvoSoul.

## 3. Caminho Demoníaco — Jardim Corrompido

**Personagem do jogador:** Gota Profana  
**Tema:** corrupção da natureza, agressividade e possibilidade de restauração

### Ambiente

Um jardim de aparência hostil, mas ainda colorido e cartunesco. Possui vegetação escura, arbustos retorcidos, manchas de solo deteriorado e plantas visivelmente corrompidas. Gotas Profanas NPCs vagam pelo mapa.

A apresentação deve deixar clara a diferença visual entre uma planta corrompida e sua versão saudável.

### Obtenção de energia

| Ação | Resultado visual e de gameplay | Energia |
|---|---|---|
| **Restaurar planta corrompida** | O jogador ataca a planta; ela deixa de estar corrompida e muda para sua versão visual saudável, permanecendo no cenário | **+1 positiva** |
| **Atacar outra Gota Profana** | O NPC sofre o ataque, executa sua animação de morte e desaparece após a apresentação correspondente | **+1 negativa** |

### Regras de interação

- A planta corrompida oferece a ação contextual **Golpear** quando está ao alcance e na direção do jogador.
- O golpe restaura a planta, não a destrói. A alteração visual deve ser perceptível.
- Uma mesma planta só concede energia positiva **uma vez** enquanto estiver restaurada.
- Gotas Profanas NPCs circulam pelo ambiente e podem ser atacadas quando selecionadas.
- Cada NPC concede energia negativa somente uma vez ao ser derrotado.
- O ataque a NPCs segue o fluxo já estabelecido no jogo: animação de morte, permanência breve no estado final e desaparecimento gradual.

## 4. Caminho Celestial — Jardim Celestial

**Personagem do jogador:** Lumigota  
**Tema:** vida, florescimento e a possibilidade de ferir seres da própria linhagem

### Ambiente

Um jardim claro e acolhedor, com cores suaves e luminosas, vegetação saudável e plantas que ainda não desenvolveram frutos. Outras Lumigotas vagam pelo local. As plantas que podem florescer devem ser distinguíveis de elementos puramente decorativos.

### Obtenção de energia

| Ação | Resultado visual e de gameplay | Energia |
|---|---|---|
| **Fazer frutos crescerem** | O jogador interage com uma planta que possui somente folhas; o objeto visual muda para sua versão com frutos, que permanecem na planta. Não é necessário comê-los | **+1 negativa, conforme a definição atual — pendente de confirmação** |
| **Atacar outra Lumigota** | O NPC executa a animação de morte e desaparece após a apresentação correspondente | **+1 negativa** |

### Regras de interação

- A planta sem frutos apresenta uma ação contextual de **Fazer crescer** (nome provisório), distinta de **Golpear**.
- A interação altera o objeto de sua versão com folhas para a versão com frutos; não cria uma segunda etapa de colher ou comer.
- Cada planta concede energia somente uma vez ao florescer.
- Lumigotas NPCs circulam lentamente e podem ser atacadas, seguindo a lógica geral de combate e morte.
- Cada Lumigota NPC concede energia negativa somente uma vez ao ser derrotada.

**Ponto pendente:** foram atribuídos pontos negativos tanto ao ataque às Lumigotas quanto ao crescimento de frutos. Como cada mapa precisa oferecer uma forma de ganhar energia positiva e outra negativa, é necessário confirmar se **Fazer frutos crescerem** deveria conceder **+1 positiva**. Até essa confirmação, este documento mantém literalmente a atribuição de energia negativa especificada e não presume uma fonte positiva alternativa.

## 5. Caminho Dracônico — Vale Primordial

**Personagem do jogador:** Drakeling Slime  
**Tema:** natureza selvagem, instinto e preservação ambiental

### Ambiente

Um vale de aspecto primitivo, com vegetação, pedras e arbustos. Alguns arbustos estão pegando fogo e contrastam claramente com a vegetação ao redor. Outros Drakelings vagam pelo mapa como NPCs.

O mapa **não contém Lumigotas nem Gotas Profanas como NPCs**. Cada linhagem deve preservar a descoberta de suas próprias criaturas e ambientes para outras partidas.

### Obtenção de energia

| Ação | Resultado visual e de gameplay | Energia |
|---|---|---|
| **Apagar arbusto em chamas** | O jogador interage com um arbusto em chamas; o fogo se extingue e o arbusto permanece no cenário, sem as chamas | **+1 positiva** |
| **Atacar outro Drakeling** | O NPC executa a animação de morte e desaparece após a apresentação correspondente | **+1 negativa** |

### Regras de interação

- Arbustos em chamas oferecem a ação contextual **Apagar fogo** (nome provisório).
- A ação remove o efeito visual das chamas e preserva o arbusto.
- Cada arbusto incendiado concede energia positiva somente uma vez ao ser apagado.
- Outros Drakelings circulam lentamente e podem ser atacados.
- Cada Drakeling NPC concede energia negativa somente uma vez ao ser derrotado.

### Direção visual do fogo

O fogo deve ser **simples, estilizado e leve**, adequado para dispositivos Android. É aceitável usar chamas low-poly animadas ou partículas simples, evitando simulação realista de fogo, fumaça volumétrica ou iluminação dinâmica pesada. O efeito visual precisa deixar claro quando o arbusto está incendiado e quando foi salvo.

A escolha exata da implementação visual será feita durante a produção; não exige simulação física de propagação de incêndio.

## 6. Padrões compartilhados entre os três mapas

### Interação contextual

- As ações são disponibilizadas quando o jogador está **próximo** do alvo e **voltado na direção dele**.
- O jogo deve distinguir claramente alvos interativos de elementos apenas decorativos.
- Cada ação contextual executa somente a atividade apropriada ao alvo selecionado.
- Não deve haver ganho repetido de energia pela mesma transformação de planta ou pelo mesmo NPC derrotado.

### NPCs

- Cada mapa apresenta **NPCs da própria linhagem jogável**, sem misturar as criaturas dos outros caminhos.
- Os NPCs se movimentam de forma lenta, vagando pelo ambiente, em velocidade inferior à do jogador.
- Ao serem derrotados, executam sua animação de morte, permanecem brevemente na pose final e desaparecem gradualmente.
- Quantidade, distribuição e eventual reaparecimento dos NPCs permanecem em aberto.

### Objetos transformáveis

- Uma planta restaurada, frutificada ou apagada visualmente **permanece no mapa** após a ação.
- A alteração deve ser persistente durante a visita ao estágio, não apenas um efeito momentâneo.
- Os objetos precisam ter estados visuais claramente distinguíveis antes e depois da interação.
- Quantidade e eventual renovação desses objetos permanecem em aberto.

### Interface e depuração

- O sistema de energias permanece oculto na interface final.
- Durante o desenvolvimento, uma interface temporária deve permitir acompanhar energia positiva, negativa, total do estágio e tendência evolutiva.
- Ao atingir **50/50**, deve aparecer a opção **Evoluir**, sem impedir a substituição de energias opostas até a confirmação.

## 7. Reutilização de estrutura

Os três mapas devem compartilhar conceitos de movimentação, seleção de alvos, interação contextual, concessão de energia e progressão, sem perder suas características visuais e atividades próprias.

A restauração das plantas, o surgimento de frutos e a extinção do fogo são variações do mesmo conceito de design: **interagir com um objeto do ambiente para transformá-lo de um estado inicial para um estado final**.

## 8. Critérios de aceitação

- [ ] O jogador entra no mapa correto de acordo com sua primeira evolução.
- [ ] A câmera permanece em terceira pessoa e o cenário é navegável no celular.
- [ ] Cada mapa contém somente NPCs da própria linhagem.
- [ ] Atacar um NPC concede **+1 negativa**, sem recompensas duplicadas.
- [ ] No Jardim Corrompido, atacar uma planta corrompida a restaura e concede **+1 positiva**.
- [ ] No Jardim Celestial, interagir com uma planta sem frutos faz os frutos crescerem, sem exigir que sejam comidos.
- [ ] A classificação energética da ação **Fazer frutos crescerem** foi confirmada antes da implementação final.
- [ ] No Vale Primordial, apagar o fogo de um arbusto concede **+1 positiva**.
- [ ] Os objetos transformados mantêm o novo visual e não concedem pontos repetidos.
- [ ] As ações só aparecem para alvos alcançáveis e corretamente selecionados.
- [ ] A interface de debug indica corretamente o progresso até **50 energias**.
- [ ] Ao atingir 50 energias, a evolução continua opcional e novas energias substituem as opostas quando possível.
- [ ] O jogo respeita a proibição de transição direta entre Celestial e Demoníaco.

## 9. Decisões ainda em aberto

- Confirmar a energia concedida por **Fazer frutos crescerem** no caminho Celestial (atualmente registrada como negativa, embora falte uma fonte positiva).
- Quantidade de NPCs e de objetos interativos em cada mapa.
- Tamanho, disposição e ambientação detalhada de cada mapa.
- Reaparecimento ou não de NPCs e de objetos interativos transformados.
- Efeitos sonoros e efeitos visuais específicos de cada interação.
- Intensidade e comportamento do fogo estilizado no Vale Primordial.
- Complexidade futura do combate; neste estágio, permanece simples.

---

Este documento descreve **o Estágio 02**, isto é, os três primeiros mapas após o Jardim-Berçário. Não define os mapas dos estágios posteriores nem altera as regras gerais de evolução do EvoSoul.

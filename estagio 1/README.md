# EvoSoul — Estágio 1: Berçário

Este diretório contém somente o planejamento da primeira versão jogável do jardim-berçário descrito em `docs/EvoSoul_Estagio_01_Bercario_GDD.md`.

## Ordem de execução

1. [`plano_fase_1`](plano_fase_1/README.md) — produzir e validar o cenário low-poly no Blender.
2. [`plano_fase_2`](plano_fase_2/README.md) — implementar a lógica de interação, energia e evolução em componentes testáveis.
3. [`plano_fase_3`](plano_fase_3/README.md) — importar, montar e validar a experiência integrada no Unity.

Cada fase só avança quando os critérios de saída indicados no respectivo `README.md` forem atendidos. Não há implementação, assets ou código neste diretório.

## Limites de escopo

- O alvo é o ciclo: explorar → escolher alvo → comer/atacar → receber energia → ajustar alinhamento → evoluir.
- O estágio não inclui IA de combate, perseguição, resposta ofensiva dos NPCs ou os mapas posteriores.
- Valores de escala, densidade, distância, ângulo, tempos de poça/fade e controles móveis são decisões de calibração; devem ser registrados durante os testes, não assumidos agora.
- O projeto final é mobile horizontal, mas todas as etapas devem permitir validação no Unity Editor.

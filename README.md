# Zombie Arcade

Juego de acción y supervivencia en tercera persona, creado en Unity. Controla al jugador, esquiva a los zombis, recoge objetos, consigue puntos y sobrevive a las oleadas hasta enfrentarte al jefe.

## Controles

| Acción | Control |
|---|---|
| Moverse | `W`, `A`, `S`, `D` o flechas |
| Correr | Mantén `Shift` izquierdo mientras te mueves |
| Saltar | `Espacio` (solo cuando estás en el suelo) |
| Apuntar | Mueve el ratón; el jugador apunta hacia el cursor |
| Disparar | Mantén pulsado el botón izquierdo del ratón |
| Volver a jugar tras ganar o perder | `Enter`, `R` o el botón **VOLVER A JUGAR** |

## Cómo funciona

- La partida empieza con **5 puntos de vida**. El HUD muestra la vida, los puntos y los zombis eliminados.
- Dispara a los enemigos para derrotarlos. Los zombis normales tienen **3 puntos de vida** y hacen daño al tocarte.
- Tras recibir un golpe, tienes un segundo de invulnerabilidad antes de poder recibir otro.
- Recoge objetos para cambiar tu puntuación: el objeto normal suma **1 punto**, el especial suma **3** y la trampa resta **1** si aparece en el nivel.
- Alcanzar **12 puntos** activa la victoria.
- La partida incluye **4 oleadas**: las tres primeras aumentan la cantidad de zombis y la última presenta al jefe. Si derrotas al jefe, también ganas.
- El jefe tiene una barra de vida, persigue al jugador y puede invocar zombis de apoyo.
- Si la vida llega a cero, aparece **GAME OVER**. Tras ganar, la partida se reinicia automáticamente al cabo de **5 segundos**; también puedes reiniciarla antes con `Enter`, `R` o el botón.


## Tecnologías

- Unity `6000.2.3f1`.
- Universal Render Pipeline (URP).
- Input System de Unity.
- Escena, modelos, materiales, interfaz y efectos generados mediante el constructor y scripts del proyecto.

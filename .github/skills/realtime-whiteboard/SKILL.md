# Skill — Realtime / Pizarra digital

## Cuándo usarla
Al diseñar o implementar la sala de sesión en vivo: pizarra colaborativa, presencia, señalización, grabación/exportación de apuntes.

## Estado
No implementado. La landing promete: pizarra digital sincronizada, video HD, PDF de apuntes y grabaciones ilimitadas. Cuando se construya, elegir tecnología con ADR (señalado aquí como decisión abierta: SignalR/WebSockets + servicio de señalización, o WebRTC puro + SFU).

## Patrones recomendados

### Arquitectura de la sala
```
Estudiante ⇄ API (auth JWT, reserva validada) ⇄ Servidor realtime (rooms por sesión)
                                              ⇄ Media (video/grabación) — decisión con ADR
```
- Acceso a la sala **validado en backend**: solo participantes de la reserva `CONFIRMED`/`IN_PROGRESS` obtienen token de sala de corta vida.
- Pizarra: eventos de dibujo (strokes) como secuencia inmutable por sesión → reconstruible para el PDF y la grabación de apuntes.
- Throttling/batching de eventos de dibujo (p. ej. máx. 30-60 eventos/s por cliente) para no saturar.

### Eventos de pizarra (mínimo)
```ts
type StrokeEvent =
  | { type: "begin"; point: Point; tool: Tool; color: string; width: number; authorId: string; seq: number }
  | { type: "extend"; point: Point; seq: number }
  | { type: "end"; seq: number }
  | { type: "clear"; seq: number };
// seq = orden determinista; el servidor es la autoridad del orden final
```

### Persistencia de la sesión
- Al finalizar: exportar strokes a PDF (los apuntes) y guardar el media (grabación) — ambos con URLs firmadas para el repositorio del estudiante.
- Reconstrucción del historial: los eventos ordenados por `seq` bastan; no depender del estado volátil del canvas.

## Anti-patrones
- Confianza en el cliente para autorizar la sala (token de sala siempre emitido/validado por backend).
- Difundir eventos sin `seq` (reordenado impredecible corrompe el historial).
- Broadcast a todos: filtrar por room; nunca una conexión global.
- Guardar canvas como bitmap único (pierde edición/exportación vectorial de apuntes).
- Transporte sin fallback (WebSockets con fallback SSE si la red lo exige).

## Reglas del repo
- Alta latencia/CPU en realtime → revisar `performance` skill y presupuesto de mensajes.
- Si la sala maneja cobro por tiempo (garantía 15'), los eventos de tiempo son de backend (confiables), no del cliente.
- Toda decisión de infra realtime (SFU, TURN/STUN, CDN) → ADR + `STACK.md`.

## Checklist final
- [ ] Token de sala de corta duración emitido por backend tras validar reserva
- [ ] Eventos con `seq`; servidor como autoridad del orden
- [ ] Throttle de eventos de dibujo
- [ ] Export de apuntes (PDF) y grabación al cerrar la sesión
- [ ] Decisión de tecnología realtime documentada en ADR
- [ ] Plan de prueba con dos clientes concurrentes

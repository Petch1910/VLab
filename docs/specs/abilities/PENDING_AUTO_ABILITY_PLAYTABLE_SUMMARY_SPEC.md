# Pending Auto Ability PlayTable Summary Spec

## Status

Implemented in `M10-52`.

## Purpose

Surface received pending AUTO ability queue payload diagnostics on the PlayTable
side panel without opening resolution prompts or mutating game state.

This is a read-only scaffold for future ability prompt UI.

## Behavior

- Local PlayTable shows zero pending ability payloads.
- Online PlayTable shows received pending ability payload count.
- The latest payload displays its payload id and source queue id.
- Invalid latest payloads show a deterministic invalid message.
- Empty decoded queues show an empty message.

## Boundary

This milestone must not:

- resolve pending ability effects
- pay costs
- commit pending queues into `GameState`
- add ability prompt buttons
- publish pending ability payloads
- mutate `GameState`

## Acceptance Tests

- formatter returns the zero-payload text for null/empty input
- formatter shows latest payload id and queue id
- formatter reports invalid latest payloads
- local PlayTable summary displays zero pending payloads
- online PlayTable summary displays received count/latest payload
- rendering the summary does not mutate `GameState`

## Future Extensions

- pending ability prompt UI
- owner-private ability choice payloads
- explicit RulesCore command to commit selected pending resolution
- PlayTable controls for choosing simultaneous AUTO order

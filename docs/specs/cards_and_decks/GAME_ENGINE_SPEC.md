# Game Engine Spec

## Source Of Truth

System-wide architecture:

- `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`

Core implementation checklist:

- `docs/CORE_DEVELOPMENT_GUARDRAILS.md`

This file focuses on game-engine-specific behavior.

## Design Principle

Manual play stays supported, but the current priority is hardening the core so
manual play, bot play, replay, simulation, and future online mode use the same
legal command/event path.

## Zones

- deck
- hand
- ride deck
- vanguard circle
- rear-guard circles
- guardian circle
- damage
- drop
- bind
- soul
- trigger zone
- order zone/set order
- G zone

## Action Model

Every state change is an action:

```json
{
  "action": "ride",
  "actor": "player1",
  "from": "hand",
  "to": "vanguard",
  "card_id": "BT01-001TH",
  "turn": 2,
  "phase": "ride"
}
```

## Required Actions MVP

- shuffle
- draw
- move card
- ride
- call
- attack
- guard
- drive check
- damage check
- add/remove power
- pay counter blast
- pay soul blast
- end phase
- undo

## Reducer Rule

Live game, replay, undo, and bot simulation must use the same reducer/action application path.

## Current Priority

M5.5 Rules Core Hardening:

- RulesCore command/query facade
- shared legal action mask
- hidden state / observation masking
- seeded RNG service
- snapshot / rollback
- AbilityCore foundation
- trigger resolver foundation
- resource ledger foundation
- guardrail test fixtures

## Ability Core Direction

The engine should grow from manual actions into structured ability actions
before the CPU becomes complex.

Core concepts:

- ability definitions are structured data, not runtime LLM decisions
- triggers produce pending ability actions
- costs, targets, and effects are resolved as explicit steps
- continuous abilities are recalculated through a dedicated update pass
- unsupported cards fall back to manual resolution

Important action categories to support over time:

- ability activate
- cost payment
- target selection
- ability resolve
- update continuous abilities
- trigger resolve
- persona ride
- order play
- marker placement

See `docs/VGDD_RESEARCH_NOTES.md`.

## Modern Standard Mechanics To Reserve For

Static inspection of Dear Days 2 reinforces that the engine should reserve
explicit model space for newer Standard mechanics instead of treating them as
manual note text.

Mechanics to model as first-class state/actions over time:

- Energy
- Energy charge
- Energy blast
- Crest
- Crest zone
- Persona ride
- Ride deck
- Order zone
- Marker placement
- OverDress
- X-overDress
- Stride and unstride for supported formats
- Replay files as event logs

See `docs/VGDD2_STEAM_STATIC_NOTES.md`.

## Master Duel-Informed Command Core

Safe static inspection of local Master Duel files reinforces a stronger command
core boundary. Its visible native duel plugin exposes card query functions,
state query functions, command mask functions, replay mode checks, backup/restore
hooks, seeded random control, CPU parameters, and effect delegates.

Our engine should mirror the architecture pattern without copying code or data:

- rules core is separate from Unity UI
- UI and bot consume the same legal command/query API
- command masks/legal actions are the only way to mutate gameplay state
- seeded RNG is required for deterministic replay and bot evaluation
- snapshot/restore exists for undo, replay, and simulation
- card-specific logic is attached through an ability/effect delegate registry
- card images and future packs use manifest plus content-addressed cache

See `docs/MASTER_DUEL_RESEARCH_NOTES.md`.

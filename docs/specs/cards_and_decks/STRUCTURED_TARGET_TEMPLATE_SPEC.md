# Structured Target Template Spec

## Status

Implemented in `M12-05`.

## Purpose

Resolve safe `ability_schema_v1` target declarations into read-only target
candidates before effect execution exists. The resolver must not reveal hidden
information or mutate `GameState`.

## Runtime Surface

`StructuredTargetTemplate.Resolve(state, playerIndex, target)` returns a
`StructuredTargetTemplateResult` with:

- `accepted`
- `rejection_reason`
- `requires_manual_resolution`
- `requested_count`
- `candidates`
- `summary`

Each `StructuredTargetCandidate` contains:

- `player_index`
- `zone`
- `instance_id`
- `card_id`
- `face_up`

## Supported v1 Scope

Supported target types:

- `none`
- `self`
- `unit`
- `card`

Supported owners:

- `self`
- `opponent`
- `any`

Supported zones are current `GameZone` values when they do not expose hidden
information:

- public zones such as `Vanguard`, `RearGuard`, `Drop`, `Damage`, `Bind`,
  `Trigger`, and `Order`
- self-owned `Hand` and `RideDeck` are allowed for local target preview

Manual placeholders:

- `circle`
- `Deck`
- `Soul`
- `GZone`
- opponent or any-player private `Hand` / `RideDeck`

Face-down cards are skipped.

## Rejections

The resolver rejects:

- missing state
- missing target
- negative count
- unsupported target type
- unsupported zone
- hidden/unsupported zone
- required target count unavailable

Optional targets accept fewer candidates than requested.

## Boundary

M12-05 does not:

- select targets interactively
- apply effects
- inspect deck order
- expose opponent private zones
- model circle positions
- add Soul/GZone to `PlayerGameState`

Unsupported target shapes continue to manual resolution until the relevant zone
and circle models exist.

## Verification

EditMode coverage verifies:

- self rear-guard visible unit resolution
- any-owner public-zone resolution
- face-down card skipping
- optional fewer-candidate acceptance
- required count rejection
- hidden/unsupported zone manual placeholders
- circle target manual placeholder
- source `GameState` no-mutation
- result JSON round-trip

## Next Work

`M12-06` adds Effect template draw/move.

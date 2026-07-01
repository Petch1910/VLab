# Structured Cost Template Spec

## Status

Implemented in `M12-04`.

## Purpose

Convert `ability_schema_v1` structured costs into `ResourceLedger` validation
requests before any live cost payment or effect execution exists.

## Runtime Surface

`StructuredCostTemplate.BuildRequest(playerIndex, turnNumber, ability)`:

- reads `StructuredAbility.costs`
- aggregates ledger-backed costs into `ResourceCostRequest`
- creates once-per-turn and once-per-fight keys
- rejects unsupported or unsafe cost shapes before ledger validation

`StructuredCostTemplate.ValidateAgainstLedger(ledger, playerIndex, turnNumber,
ability)`:

- builds the request
- validates it through `ResourceLedger.ValidateCost`
- does not mutate live `GameState`
- does not mutate the input ledger

## Supported v1 Costs

Ledger-backed:

- `counter_blast`
- `soul_blast`
- `energy_blast`
- `once_per_turn`
- `once_per_fight`
- `none`

Manual placeholder:

- `discard`

`discard` currently rejects with `requires_manual_resolution = true` because
there is no event-sourced hand-discard cost payment command yet.

## Rejections

The template rejects:

- missing ability
- missing costs list
- negative amount
- unsupported cost type
- multiple once-per-turn entries
- multiple once-per-fight entries

## Boundary

M12-04 does not:

- flip damage cards face-down
- remove soul cards
- spend energy on live state
- discard from hand
- append `GameEvent`
- execute effects
- choose targets

Cost payment commits must later go through RulesCore/event-sourced mutation.

## Verification

EditMode coverage verifies:

- CB/SB/Energy cost aggregation
- once key generation and custom once key preservation
- ResourceLedger validation without ledger mutation
- discard placeholder requires manual resolution
- negative cost rejection before ledger validation
- duplicate once flag rejection
- result JSON round-trip

## Next Work

`M12-05` adds Target template v1.

# Resource Ledger Spec

## Status

Implemented in `M11-10` as a pure validation ledger.

## Purpose

Prepare cost validation before structured ability execution expands in `M12`.
The ledger validates whether a player can pay common Vanguard resources without
mutating live `GameState` or resolving card text.

## Runtime Surface

`ResourceLedgerState` stores a resource snapshot:

- `player_index`
- `available_counter_blast`
- `available_soul`
- `available_energy`
- `used_once_per_turn_keys`
- `used_once_per_fight_keys`

`ResourceLedgerState.FromGameState(state, playerIndex, availableSoul,
availableEnergy, ...)` derives CounterBlast from face-up damage and derives Soul
from `PlayerGameState.soul`. The explicit `availableSoul` argument remains as an
override for offline previews and fixtures. Energy is still explicit until the
core has a first-class Energy zone/resource model.

`ResourceCostRequest` stores a cost request:

- `counter_blast`
- `soul_blast`
- `energy_blast`
- `once_per_turn_key`
- `once_per_fight_key`

`ResourceLedger.ValidateCost(ledger, request)` returns a
`ResourceLedgerValidationResult` with:

- `accepted`
- `rejection_reason`
- `before_state`
- `after_state`
- cloned request

Accepted requests produce an `after_state` with resources reduced and once keys
recorded. Rejected requests leave `after_state` equal to `before_state`.

## Rejections

The ledger rejects:

- missing ledger
- missing request
- player mismatch
- negative costs
- unavailable CounterBlast
- unavailable SoulBlast
- unavailable EnergyBlast
- duplicate once-per-turn key
- duplicate once-per-fight key

## Boundary

The original `M11-10` milestone did not:

- add soul or energy fields to `PlayerGameState`
- flip damage face-down
- remove soul cards
- spend energy on live state
- append `GameEvent`
- resolve card text or choose targets

`M21-04b` supersedes the Soul part of that boundary by wiring
`PlayerGameState.soul` into ledger derivation. Live SoulBlast/SoulCharge
execution belongs to `M21-04d` and must commit through RulesCore/GameEvent.

## Transaction Rule

```text
Validate all costs -> produce after_state -> later commit through RulesCore/event path
```

No UI, bot, network, or ability code should directly mutate `GameState` to pay
resources.

## Verification

EditMode coverage verifies:

- CounterBlast derivation from face-up damage
- accepted CB/SB/Energy costs produce expected `after_state`
- ledger and source `GameState` are not mutated
- unavailable CB/SB/Energy costs reject transactionally
- duplicate once-per-turn and once-per-fight keys reject
- negative costs and player mismatch reject
- missing inputs reject
- state/result JSON round-trip

## Next Work

`M21-04d` adds event-sourced SoulCharge/SoulBlast command coverage while keeping
ResourceLedger as the pure preview/validation layer.

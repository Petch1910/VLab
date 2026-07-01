# Structured Effect Draw/Move Template Spec

## Status

Implemented in `M12-06`.

## Purpose

Execute the first safe structured effect templates through existing
`RulesCore` command paths. This proves schema v1 effects can become gameplay
commands without letting effect code mutate `GameState` directly.

## Runtime Surface

`StructuredEffectTemplate.Preview(state, playerIndex, effect)`:

- clones the state
- applies the effect to the clone through `RulesCore`
- returns generated events
- does not mutate live state

`StructuredEffectTemplate.Apply(state, playerIndex, effect)`:

- applies the effect to live state through `RulesCore`
- returns generated events
- rejects if no legal command exists

## Supported v1 Effects

- `draw`
- `move_zone`

`draw` repeats the legal Draw command `amount` times.

`move_zone` repeats the first legal MoveCard command matching `from_zone` and
`to_zone`.

## Manual Placeholders

The following currently reject with manual-resolution behavior:

- `manual`
- unsupported effect types such as `power_plus`
- unsupported or unparseable zones

Stat modifiers and resource ops are later M12 tasks.

## Boundary

M12-06 does not:

- bypass `RulesCore`
- execute arbitrary effect text
- choose targets interactively
- apply power/critical modifiers
- execute CounterCharge/SoulCharge/SoulBlast/CounterBlast effects
- publish network payloads

## Verification

EditMode coverage verifies:

- draw preview does not mutate live state
- draw apply mutates through RulesCore and event log
- move-zone apply uses RulesCore MoveCard command
- unsupported/manual effects require manual resolution
- invalid move rejects without live-state mutation
- result JSON round-trip

## Next Work

`M12-07` adds resource operation effect templates.

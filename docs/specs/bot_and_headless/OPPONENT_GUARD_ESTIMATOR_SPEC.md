# Opponent Guard Estimator Spec

## Status

Implemented in `M9-04`.

## Purpose

Estimate how much shield an opponent may have from public/masked information so
the bot can choose attack order and risk level without reading hidden hand card
ids.

This is a heuristic estimator, not a rules engine and not an anti-cheat system.

## Inputs

- `GameState` or masked view selected by the caller
- opponent player index
- optional `ICardRepository` for visible known card shield values
- estimator options:
  - average shield per unknown hand card
  - maximum shield per unknown hand card
  - confidence values for visible versus hidden information

## Output

Return:

- hand count
- visible known shield
- unknown hand card count
- conservative shield estimate
- expected shield estimate
- maximum shield estimate
- confidence score
- explanation text

## Boundary

The estimator must not:

- inspect true opponent hand if the caller should not have it
- infer hidden card ids from masked instance ids
- mutate `GameState`
- consume RNG
- claim exact guard availability

Hidden cards are counted as unknown cards and assigned configurable heuristic
shield values.

## Acceptance Tests

- masked hidden hand cards count as unknown shield
- visible known hand cards use repository shield values
- missing card stats count as unknown
- output is deterministic
- state JSON is unchanged after estimation

## Future Extensions

- revealed-card memory from public event logs
- perfect guard likelihood from deck profile and cards already seen
- trigger/shield density by remaining public deck estimate
- player-specific guard tendency from opponent profiler

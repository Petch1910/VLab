# Trigger Resolver Spec

## Status

Implemented in `M10-01`.

## Purpose

Introduce structured trigger categories and a pure resolver result model before
implementing real drive/damage check automation.

This scaffold describes what a trigger would grant. It does not move cards,
choose trigger allocation targets, or mutate `GameState`.

## Supported Trigger Types

MVP:

- `None`
- `Critical`
- `Draw`
- `Front`
- `Heal`
- `Over`
- `Unknown`

Default D-era values:

- normal trigger power bonus: `+10000`
- critical trigger: `+10000` and `+1 critical`
- draw trigger: `+10000` and draw `1`
- front trigger: `+10000` to front row
- heal trigger: `+10000` and heal attempt flag
- over trigger: over-trigger flag plus large power placeholder

## Boundary

The resolver must not:

- mutate `GameState`
- perform drive/damage card movement
- allocate trigger bonuses to specific units
- parse Thai card text at runtime
- consume RNG

Actual check automation will later compose this resolver with legal card movement
and event logging.

## Acceptance Tests

- critical trigger returns power and critical bonus
- draw trigger returns power and draw count
- front trigger returns front-row bonus
- heal trigger returns heal-attempt flag
- over trigger returns over-trigger flag
- none returns accepted zero bonuses
- unknown returns manual/unsupported status
- repeated resolution is deterministic

## Future Extensions

- rule-set-specific values by format/era
- trigger allocation target validation
- damage check heal legality
- over trigger nation-specific extra effects
- event-log integration for drive/damage checks

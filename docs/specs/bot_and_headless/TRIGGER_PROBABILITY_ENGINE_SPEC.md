# Trigger Probability Engine Spec

## Status

Implemented in `M9-01`.

## Purpose

Provide exact trigger probability numbers for bot planning and future battle
search without consuming live RNG or revealing concrete deck order.

This engine answers questions such as:

- probability of at least one trigger in one drive check
- probability of exactly zero, one, or two triggers in twin drive
- probability distribution for an arbitrary number of checks from a known count
  pool

## Boundary

This is an AI planning helper, not a game resolver.

It must not:

- decide the actual card checked
- mutate `GameState`
- consume or advance `SeededRandomService`
- inspect opponent private zones unless the caller already has a legal masked or
  owner-private view
- replace the real trigger resolver

Actual drive/damage check outcomes still come from the live core RNG and are
recorded in the event log.

## Input Model

MVP input is count-based:

```text
total_cards
trigger_cards
check_count
```

The engine treats the unknown deck segment as an unordered pool. It calculates a
hypergeometric distribution:

```text
P(k triggers in n checks) =
  C(trigger_cards, k) * C(non_trigger_cards, n - k) / C(total_cards, n)
```

## Output Model

Return a result object with:

- validity flag
- error code for invalid input
- total cards
- trigger cards
- check count
- probability of at least one trigger
- exact hit distribution from 0 through `check_count`

Invalid input should fail clearly instead of silently clamping values.

## Acceptance Tests

- one check from 50 cards with 16 triggers gives `16 / 50`
- two checks from 50 cards with 16 triggers gives exact 0/1/2 hit distribution
- zero-trigger pools return 100% for zero hits
- invalid counts fail with a clear error code
- changing card order does not affect count-based output

## Future Extensions

- trigger-type distribution, for example critical/draw/front/heal/over
- drive versus damage check policy helpers
- known-card exclusion from public information
- guard-break probability helpers for battle search
- Monte Carlo fallback for ability-heavy branches that are too complex for exact
  closed-form calculation

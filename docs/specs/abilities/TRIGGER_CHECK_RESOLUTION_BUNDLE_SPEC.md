# Trigger Check Resolution Bundle Spec

## Status

Implemented in `M10-07`.

## Purpose

Bundle the advisory outputs for one resolved drive/damage check into a single
serializable result. This gives UI, replay tooling, and bot planning one object
that links:

- check metadata
- checked card identity
- structured trigger resolver output
- advisory trigger allocation plan
- generated combat modifier ledger
- notes for side effects that remain manual

This scaffold does not reveal a card from deck, move cards between zones, spend
resources, or mutate `GameState`.

## Inputs

- `GameState`
- player index
- check source: drive, damage, or manual
- check index
- checked card instance id
- checked card id
- `TriggerType`
- modifier expiration timing

## Output

- accepted/manual status
- rejection reason
- check metadata
- checked card ids
- resolved trigger result
- allocation plan
- combat modifier ledger
- notes
- explanation

## Boundary

The helper must not:

- inspect or pop the top card of a deck
- move checked cards to hand, damage, trigger zone, or drop
- validate live allocation clicks
- mutate `GameState`
- apply generated modifiers to `GameState`

## Acceptance Tests

- critical trigger bundle contains resolver, allocation, and modifier outputs
- no-trigger bundle is accepted and produces no modifiers
- bundle JSON round-trips
- repeated bundles are deterministic
- input `GameState` is unchanged

## Future Extensions

- RulesCore command that performs an actual checked-card reveal
- damage check movement and heal validation
- drive check trigger-zone/drop movement
- player-selected trigger allocation targets
- event log entries for trigger check resolution

# Hidden State View Hardening Spec

## Status

Implemented in `M11-09`.

## Purpose

Make hidden-state masking a repeatable correctness gate before resource-ledger,
structured ability, bot, and multiplayer work expands. Player, spectator, and
future bot views must never receive private card identity data that belongs to
hidden zones or hidden source events.

## Runtime Surface

`HiddenStateViewHardeningVerifier.Verify(trueState)` creates:

- a true-state clone view
- one player view for each player index
- one spectator view

It then checks those views without mutating the source `GameState`.

## Masking Rules

Player views:

- hide every player's deck contents
- show only the viewing player's face-up hand and ride deck cards
- hide opponent hand and ride deck cards
- hide face-down public-zone cards
- keep face-up public-zone cards visible
- mask opponent private event card ids
- preserve own private event card ids only when the viewer is the actor

Spectator views:

- hide every player's deck, hand, and ride deck contents
- hide face-down public-zone cards
- keep face-up public-zone cards visible
- mask all private event card ids

True-state views:

- preserve complete source state in a clone
- are not sent to UI, bot, opponent, spectator, or network surfaces

## Rejections

The verifier rejects:

- missing state
- missing players
- view factory failures
- hidden-state leaks or over-visible private events
- source-state mutation while creating views

## Boundary

The verifier is a guard/checking tool. It does not:

- change game rules
- publish network payloads
- mutate `GameState`
- make bot decisions
- replace public-event masking for multiplayer delivery

Bot observation should continue to use player-scoped views plus legal action
masks. Advanced bot/search work must not read true-state opponent hands or deck
order.

## Verification

EditMode coverage verifies:

- current player and spectator masking policy passes the verifier
- verifier does not mutate true state
- direct views mask private zones/events and preserve visible public cards
- missing state/player rejection paths
- result JSON round-trip

## Next Work

`M11-10` adds resource-ledger validation coverage for CounterBlast, SoulBlast,
Energy, and once-per-turn/once-per-fight flags.

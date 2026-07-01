# Trigger Check Draft Selector Cycle Helper Spec

## Status

Implemented in `M10-33`.

## Purpose

Extract manual trigger-check draft selector cycling rules into a pure UI helper.

This keeps PlayTable selector behavior unchanged while making the draft selector
rules reusable for future compact or mobile trigger-check controls.

## Inputs

- current draft trigger type
- current draft check source
- current draft check index

## Output

- next trigger type in the current cycle:
  `Unknown -> Critical -> Draw -> Front -> Heal -> Over -> None -> Unknown`
- next check source in the current cycle:
  `Manual -> Drive -> Damage -> Manual`
- next check index using modulo `0..3`

## Boundary

The helper must not:

- know about Unity UI objects
- send payloads
- move cards
- inspect card identity
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- trigger type completes the existing full cycle
- check source completes the existing full cycle
- check index wraps from `3` to `0`
- negative check indexes normalize into the `0..3` range
- PlayTable uses the helper without changing selector behavior
- Unity compile and EditMode tests pass

## Future Extensions

- configurable max check count per check source
- disabled selector states for non-manual phases
- localized selector labels

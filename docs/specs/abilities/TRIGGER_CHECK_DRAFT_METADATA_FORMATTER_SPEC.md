# Trigger Check Draft Metadata Formatter Spec

## Status

Implemented in `M10-28`.

## Purpose

Extract trigger-check draft metadata text into a small pure UI helper so
PlayTable and future trigger-check UI surfaces can share the same labels for
trigger type, check source, and check index.

This keeps `DraftTrig` publishing behavior unchanged while reducing formatting
logic inside PlayTable.

## Inputs

- draft trigger type
- draft check source
- draft check index

## Output

- summary metadata text: `<TriggerType> / <TriggerCheckSource> / idx <index>`
- type selector button label: `Type <short-trigger-type>`
- source selector button label: `Src <TriggerCheckSource>`
- index selector button label: `Idx <index>`

## Boundary

The helper must not:

- send payloads
- move cards
- inspect selected card identity
- inspect hidden card identity
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- default metadata formats as `Unknown / Manual / idx 0`
- cycled metadata formats as `Critical / Drive / idx 1`
- selector button labels are deterministic
- trigger type short labels match the existing PlayTable labels
- PlayTable summary behavior remains unchanged
- Unity compile and EditMode tests pass

## Future Extensions

- pure formatter for the full draft summary line
- localized labels when the UI text layer is ready
- compact draft status component shared by manual draft and real trigger-check UI

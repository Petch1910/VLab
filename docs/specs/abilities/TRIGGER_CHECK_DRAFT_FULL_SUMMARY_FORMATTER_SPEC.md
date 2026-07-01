# Trigger Check Draft Full Summary Formatter Spec

## Status

Implemented in `M10-29`.

## Purpose

Extract the full trigger-check draft summary line into a pure UI helper that
combines local-mode text, draft metadata text, and selected-card status text.

This keeps PlayTable behavior unchanged while making the full draft summary
reusable for future trigger-check UI components.

## Inputs

- online/local mode flag
- draft trigger type
- draft check source
- draft check index
- selected card id
- selected card instance id
- selected card zone

## Output

- local mode: `Draft: local mode`
- online mode: `Draft: <metadata> / <selected-card-status>`
- metadata is provided by `TriggerCheckDraftMetadataFormatter`
- selected-card status is provided by `TriggerCheckDraftStatusFormatter`

## Boundary

The helper must not:

- know about multiplayer sessions or transports
- send payloads
- move cards
- inspect hidden card identity beyond public hidden labels
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- local mode formats as `Draft: local mode`
- online default summary matches existing PlayTable text
- online selected-card status summary matches existing PlayTable text
- online metadata changes match existing PlayTable text
- PlayTable uses the helper without behavior change
- Unity compile and EditMode tests pass

## Future Extensions

- localized draft summary labels
- compact status rows for mobile/tablet layouts
- shared summary component for manual draft and real trigger-check display

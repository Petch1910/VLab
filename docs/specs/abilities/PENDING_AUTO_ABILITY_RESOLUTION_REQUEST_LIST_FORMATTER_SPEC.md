# Pending Auto Ability Resolution Request List Formatter Spec

## Goal

Render a bounded newest-first list of selected pending auto ability resolution
request payloads for debugging and online intent visibility.

This is a pure UI formatter. It must not resolve abilities, mutate payloads, or
touch `GameState`.

## Contract

- No payloads format as `Pending resolve request list: 0`.
- Valid payloads render newest-first.
- `maxItems` bounds the rendered list and reports older remaining payloads.
- Invalid payloads render a safe line with the decoder rejection reason.
- Hidden source-card identity remains hidden.

## Verification

- Unity compile passes.
- EditMode tests cover no-payload, newest-first ordering, bounded output,
  invalid payloads, hidden-source safety, and no payload mutation.

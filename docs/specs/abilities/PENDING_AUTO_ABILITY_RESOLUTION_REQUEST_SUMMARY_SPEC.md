# Pending Auto Ability Resolution Request Summary Spec

## Goal

Show a read-only summary of pending auto ability resolution request payloads
that have been received or locally published by the multiplayer session.

This is an observation surface only. It must not resolve abilities, mutate
`GameState`, or alter pending queues.

## Formatter Contract

- `PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest` accepts a
  list of `NetworkPendingAutoAbilityResolutionRequestPayload`.
- No payloads format as `Pending resolve requests: 0`.
- Invalid latest payloads format a safe rejection summary with the decoder
  reason.
- Valid latest payloads show count, latest payload id, selected index, pending
  id, player index, timing event, and source visibility.
- Hidden source-card identity must remain hidden.

## PlayTable Contract

- PlayTable includes a read-only `Pending Ability Resolution Request Summary`
  text surface.
- Local mode shows the no-payload summary.
- Online mode reads from
  `MultiplayerGameSessionController.PendingAutoAbilityResolutionRequestPayloads`.
- Publishing or receiving a request refreshes the summary through normal
  PlayTable refresh.

## Verification

- Unity compile passes.
- EditMode tests cover no-payload, invalid-payload, visible latest request,
  hidden-source safety, online received summary, online local publish summary,
  and no `GameState` mutation.

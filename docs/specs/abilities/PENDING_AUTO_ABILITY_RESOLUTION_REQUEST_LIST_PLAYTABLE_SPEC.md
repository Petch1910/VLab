# Pending Auto Ability Resolution Request List PlayTable Spec

## Goal

Expose the bounded newest-first selected resolution request list on PlayTable as
a read-only diagnostic surface.

This surface shows online intent history only. It must not resolve abilities or
mutate true gameplay state.

## UI Contract

- PlayTable includes `Pending Ability Resolution Request List`.
- Local mode shows `Pending resolve request list: 0`.
- Online mode reads
  `MultiplayerGameSessionController.PendingAutoAbilityResolutionRequestPayloads`.
- The list uses `PendingAutoAbilityResolutionRequestListFormatter`.
- The list is refreshed through the existing PlayTable refresh path.

## Verification

- Unity compile passes.
- EditMode tests cover local no-list text, online newest-first list after
  multiple received payloads, hidden-source safety via the formatter, and no
  `GameState` mutation.

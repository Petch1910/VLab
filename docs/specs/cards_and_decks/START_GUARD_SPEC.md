# Start Guard Spec

## Status

Implemented through `M13-07`.

## Purpose

Before a multiplayer game state is created, the lobby must verify that the room
pack metadata and player deck metadata still match the local runtime inputs.

## Shared Deck-Code Rooms

`MultiplayerLobbyController.TryCreateInitialGameState(...)` now:

1. validates room readiness and pack hash through `MultiplayerProtocol`
2. imports host and guest deck codes
3. recomputes each imported deck hash with
   `DeckCommitmentService.ComputeDeckHash(...)`
4. rejects start if `RoomPlayerInfo.deck_hash` is missing or mismatched

Rejection reasons:

- `HOST_DECK_HASH_MISSING`
- `HOST_DECK_HASH_MISMATCH`
- `GUEST_DECK_HASH_MISSING`
- `GUEST_DECK_HASH_MISMATCH`

## Commitment Rooms

Commitment rooms remain blocked by deck privacy gameplay policy until the later
owner-private gameplay gates are complete. Commitment metadata is still checked
by `MultiplayerProtocol.ValidateRoomReady(...)`.

## Verification

EditMode tests cover:

- shared deck-code start succeeds with matching canonical deck hash
- shared deck-code start rejects guest deck hash mismatch
- existing pack hash mismatch validation remains in the lobby/protocol tests

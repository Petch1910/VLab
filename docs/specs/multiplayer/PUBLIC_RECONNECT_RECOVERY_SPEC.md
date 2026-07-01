# Public Reconnect Recovery Spec

## Status

Implemented through `M13-06` as public event batch creation/application and a
commitment-mode true reconnect block.

## Purpose

Commitment-only rooms cannot replay true `NetworkEventEnvelope` batches because
those events may contain private card instance ids. They need a public reconnect
path based on `NetworkPublicGameEvent`.

## Runtime Model

`NetworkPublicEventBatch` contains:

- `protocol_version`
- `room_id`
- `from_event_index`
- `events`

`NetworkPublicReconnectRecovery.CreateBatch(...)` slices a public event log
from a requested cursor.

`NetworkPublicReconnectRecovery.ApplyBatch(...)` applies the batch to a
`LocalOwnerPrivateSession` through `NetworkPublicGameEventApplier`.

`MultiplayerGameSessionController.CreateReconnectBatch(...)` now returns an
empty true-event batch and sets `COMMITMENT_TRUE_RECONNECT_BLOCKED` when the room
uses a non-shared deck privacy mode.

## Boundaries

- Public reconnect updates only public/opponent views.
- Public reconnect does not append to `GameState.event_log`.
- True reconnect events remain allowed for shared-deck-code rooms.
- Photon dispatch for public event batches remains later work.

## Verification

EditMode tests cover:

- public batch slicing from cursor
- Photon payload round-trip for public batches
- applying a public batch to owner-private session view/cursor/log
- cursor mismatch rejection without session mutation
- commitment-mode true reconnect batch emits no true events

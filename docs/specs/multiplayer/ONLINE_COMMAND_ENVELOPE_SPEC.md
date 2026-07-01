# Online Command Envelope Spec

## Status

Implemented through `M13-04` as a payload model, factory, shape validator,
state validator, and Photon codec. Transport dispatch and gameplay execution
are intentionally left for later milestones.

## Purpose

Online gameplay needs a command-level payload that records who requested an
action and which state cursor they acted from before that command becomes a
committed `GameEvent`.

This is separate from `NetworkEventEnvelope`, which carries already-committed
events.

## Runtime Model

`NetworkCommandEnvelope` contains:

- `protocol_version`
- `command_id`
- `room_id`
- `room_game_id`
- `player_id`
- `player_index`
- `sequence`
- `state_cursor`
- `sent_at_utc`
- `LegalGameAction action`

`NetworkCommandEnvelopeFactory.Create(...)` clones the supplied
`LegalGameAction` and captures `GameState.event_log.Count` as `state_cursor`.

`NetworkCommandEnvelopeValidator.TryValidateShape(...)` validates basic
envelope shape.

`NetworkCommandEnvelopeValidator.TryValidateForState(...)` validates:

- `room_game_id` matches the current `GameState.game_id`
- `state_cursor` matches `GameState.event_log.Count`
- `player_id` and `player_index` match room owner metadata
- action `actor_index` matches the envelope player index
- current turn-owner-gated actions are sent by `GameState.turn_player_index`

## Photon Payload

`PhotonRealtimePayloadCodec.CommandEnvelopeEventCode = 12`.

The codec can encode/decode `NetworkCommandEnvelope` but the Photon transport
does not dispatch this event code yet.

## Non-Goals

- replacing current committed `NetworkEventEnvelope` sync
- executing remote command envelopes
- server-authoritative command validation

## Verification

EditMode tests cover:

- factory metadata capture
- JSON round-trip
- Photon payload round-trip
- wrong event-code rejection
- basic shape validation
- stale cursor rejection without mutating state
- out-of-turn rejection
- actor/player ownership mismatch rejection

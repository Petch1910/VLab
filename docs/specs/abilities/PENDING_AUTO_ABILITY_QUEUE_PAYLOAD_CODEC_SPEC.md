# Pending Auto Ability Queue Payload Codec Spec

## Status

Implemented in `M10-48`.

## Purpose

Define a deterministic network-ready payload model for already-masked pending
AUTO ability queues.

This milestone only prepares serialization boundaries. It does not publish
payloads over Photon, mutate gameplay state, or open pending ability UI.

## Payload Fields

- `protocol_version`
- `payload_id`
- `room_id`
- `sender_player_index`
- `source_queue_id`
- `pending_count`
- `perspective`
- `viewer_player_index`
- `pending_auto_ability_queue_json`

## Boundary

The codec must not:

- mutate the source queue
- mutate `GameState`
- mask raw queues automatically
- resolve ability text or effects
- publish transport events
- accept mismatched protocol versions
- accept empty or invalid queue JSON

## Acceptance Tests

- payload round-trips a masked pending ability queue
- wrong protocol version is rejected
- empty queue JSON is rejected
- invalid queue JSON is rejected
- encoding is deterministic
- encoding does not mutate the source queue

## Future Extensions

- Photon payload wrapper/event code
- transport send/receive hook
- session storage for received pending ability prompts
- PlayTable pending ability prompt surface

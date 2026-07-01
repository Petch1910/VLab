# Trigger Check Replay Payload Codec Spec

## Status

Implemented in `M10-11`.

## Purpose

Wrap an already-masked `TriggerCheckReplayLog` into a deterministic network-
ready payload for future replay and multiplayer transport.

This scaffold prepares the data contract only. It does not add a Photon event
code, dispatch through `IMultiplayerTransport`, or perform implicit masking.

## Inputs

- room id
- sender player id
- already-masked `TriggerCheckReplayLog`
- perspective
- viewer player index

## Output

- protocol version
- deterministic payload id
- room/sender/source log metadata
- perspective metadata
- trigger check replay log JSON

## Boundary

The codec must not:

- mutate the source log
- perform masking implicitly
- send Photon payloads
- apply replay entries to `GameState`
- accept wrong protocol versions

## Acceptance Tests

- encoded payload decodes back into the masked log
- wrong protocol version is rejected with a reason
- empty or invalid log JSON is rejected with a reason
- encoding is deterministic
- encoding does not mutate the source log

## Future Extensions

- Photon event code for trigger check log payloads
- `IMultiplayerTransport` send/receive hooks
- replay UI hydration from trigger check log payloads
- public/private transport policy integration

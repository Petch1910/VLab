# Pending Auto Ability Resolution Request Payload Codec Spec

## Status

Implemented in `M10-65`.

## Purpose

Add a network payload model and codec for a selected pending AUTO ability
resolution request.

This creates the serialization boundary only. It does not reserve a Photon event
code, send transport messages, resolve abilities, or mutate `GameState`.

## Behavior

- Encodes room id, sender player index, selected index, pending id,
  perspective, viewer player index, and request JSON.
- Payloads include the current multiplayer protocol version.
- Decoding rejects null/empty request JSON and protocol mismatches
  deterministically.
- Invalid request JSON returns a parse-failed rejection.
- Encoding is deterministic for the same request and metadata.
- Encoding clones the request and sanitizes hidden source identity before JSON
  leaves the process.
- Encoding does not mutate the source request.

## Boundary

This milestone must not:

- add a Photon wrapper or event code
- publish request payloads over transport
- add a PlayTable resolve button
- resolve pending abilities
- validate costs or targets against live ability rules

## Acceptance Tests

- visible request payload round-trips
- wrong protocol version is rejected
- empty request JSON is rejected
- invalid request JSON is rejected
- encoding is deterministic
- hidden request source identity is sanitized without mutating the source

## Future Extensions

- Photon payload wrapper
- transport hook
- session storage
- PlayTable resolve-selected request button

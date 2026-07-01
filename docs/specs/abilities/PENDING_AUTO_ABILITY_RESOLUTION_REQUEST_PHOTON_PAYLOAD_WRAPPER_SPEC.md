# Pending Auto Ability Resolution Request Photon Payload Wrapper Spec

## Status

Implemented in `M10-66`.

## Purpose

Reserve and test the Photon Realtime wrapper for selected pending AUTO ability
resolution request payloads.

This only wraps and unwraps the existing request payload. It does not dispatch
through transport, store session payloads, add PlayTable buttons, or resolve
abilities.

## Behavior

- Uses Photon event code `10`.
- Encodes sender player id from the request payload sender player index.
- Decoding rejects wrong event codes or empty wrapper JSON deterministically.
- Decoding validates the inner request payload and rejects protocol mismatches.
- Wrapper JSON is deterministic for the same inner payload.
- Wrapping does not mutate the source request payload.

## Boundary

This milestone must not:

- update transport dispatch
- publish request payloads
- add session storage
- add PlayTable resolve controls
- resolve pending abilities

## Acceptance Tests

- Photon wrapper round-trips the inner resolution request payload
- wrong Photon event code is rejected
- inner protocol mismatch is rejected
- wrapper JSON is deterministic
- wrapping does not mutate the source payload

## Future Extensions

- transport hook dispatch
- session storage
- PlayTable resolve-selected request publishing

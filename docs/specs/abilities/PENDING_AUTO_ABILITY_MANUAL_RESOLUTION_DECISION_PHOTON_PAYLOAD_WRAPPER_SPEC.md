# Pending Auto Ability Manual Resolution Decision Photon Payload Wrapper Spec

## Status

Implemented in `M10-76`.

## Purpose

Reserve and test the Photon Realtime wrapper for pending AUTO ability manual
resolution decision payloads.

This only wraps and unwraps the existing decision payload. It does not dispatch
through transport, store session payloads, add PlayTable controls, or resolve
abilities.

## Behavior

- Uses Photon event code `11`.
- Encodes sender player id from the decision payload sender player index.
- Decoding rejects wrong event codes or empty wrapper JSON deterministically.
- Decoding validates the inner decision payload and rejects protocol mismatches.
- Wrapper JSON is deterministic for the same inner payload.
- Wrapping does not mutate the source decision payload.

## Boundary

This milestone must not:

- update transport dispatch
- publish decision payloads
- add session storage
- add PlayTable decision controls
- resolve pending abilities

## Acceptance Tests

- Photon wrapper round-trips the inner decision payload
- wrong Photon event code is rejected
- inner protocol mismatch is rejected
- wrapper JSON is deterministic
- wrapping does not mutate the source payload

## Future Extensions

- transport hook dispatch
- session storage
- PlayTable manual resolution decision publishing

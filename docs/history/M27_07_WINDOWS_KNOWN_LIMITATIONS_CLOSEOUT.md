# M27-07 Windows Known Limitations Closeout

## Status

Complete.

## What Changed

- Added `docs/WINDOWS_LOCAL_BUILD_KNOWN_LIMITATIONS.md`.
- Added `docs/specs/ci_and_qa/WINDOWS_KNOWN_LIMITATIONS_SPEC.md`.
- Documented current limits for product scope, card data, PlayTable, online,
  bot/automation, performance, and release boundary.

## Verification

Docs-only. Unity tests were not rerun because no runtime code changed after
M27-06.

Checklist:

- Windows-only scope is stated.
- Public release is blocked until explicit user instruction.
- Photon trusted-client limitation is stated.
- Ability automation limitation is stated.
- Performance measurement limits are stated.

## Next Target

`M27-08`: Do not make a public release until the user asks.

# M27-01 Windows Stability Smoke Coverage Spec

## Goal

Extend the Windows player smoke so it covers the main Windows workflow before
any public release or mobile work resumes.

## Required Coverage

The smoke must cover:

- Home Dashboard status and Solo setup readiness
- Deck Builder validation and deck-code round trip
- PlayTable command path
- Manual content readiness
- Settings default formatting and option cycling
- Online Room trusted-client usability guard
- Windows PlayTable board-first layout QA

## Non-Goals

- Do not run Android, APK, LDPlayer, mobile QA, app packaging, or public release
  verification.
- Do not connect to live Photon in this smoke.
- Do not execute a full interactive match.

## Verification

- Unity compile.
- Unity EditMode.
- Windows build and `-vanguardPlayerSmoke` because the player smoke runtime path
  changed.

# Windows-Only Verification Profile

## Purpose

This profile defines what verification is allowed while the project is in the
Windows-first program-completion phase.

## Allowed Verification

Run these when relevant:

- Unity batchmode compile for C#/Unity changes.
- Unity EditMode tests for C#/Unity changes.
- Windows player smoke for runtime flow changes.
- Python unit/data/validator tests for data, import, schema, or tooling changes.
- Static documentation/link checks for docs-only changes.

Windows player smoke is allowed because it verifies the local Windows program.
It is not a release package, public distribution, or mobile workflow.

## Deferred Verification

Do not run these until the user explicitly re-enables Android/mobile/release
work:

- Android build.
- APK generation.
- APK install smoke.
- LDPlayer smoke.
- ADB/emulator smoke.
- Android touch/layout QA.
- Mobile QA.
- App packaging.
- Release candidate packaging.
- Public distribution checks.

## Verification By Change Type

Docs-only change:

- Run a targeted `rg`/link-presence check.
- Do not run Unity unless the docs change also changes generated assets or
  runtime files.

C#/Unity logic or UI change:

- Run Unity compile.
- Run Unity EditMode.
- Run Windows player smoke if the change touches runtime flow, scene boot,
  Home, Deck Builder, PlayTable, Manual, Settings, or Online Room.

Python/data/import change:

- Run the affected Python validator or unit tests.
- Run Unity compile/EditMode only if runtime Unity loading behavior changes.

Network/Photon change:

- Run Unity compile.
- Run Unity EditMode.
- Run the relevant Photon/local smoke where available.
- Run Windows player smoke if the player-facing room flow changes.

## Current Active Gate

The active verification gate for M20-M27 is:

```text
Unity compile + EditMode + Windows player smoke when code/runtime changes need it.
No Android/mobile/APK/release packaging unless the user explicitly reopens it.
```

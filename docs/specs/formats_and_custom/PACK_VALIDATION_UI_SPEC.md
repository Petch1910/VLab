# Pack Validation UI Spec

Milestone: `M24-07`

## Purpose

Expose pack validation status inside the Windows Deck Builder so players can
understand whether a runtime pack or local custom import report is safe to use.

This is a UI/status milestone only. It must not import, stage, download, or
mutate packs.

## UI Entry

Deck Builder -> Deck Tools adds a `Pack Check` action.

The action shows:

- active runtime pack validation status when the Deck Tools input is empty or
  does not contain a local custom import validator report
- pasted `validate_local_custom_import.py --json` report when the input contains
  local validator JSON

## Display Requirements

The status text must include:

- accepted/blocked state
- pack id
- schema/source schema or adapter
- series/set count when available
- card count
- image count and missing image count
- ability count when available
- unsupported field count for local import reports
- bounded warnings/errors
- source/copyright boundary note

## Boundaries

- Do not run Python from Unity.
- Do not auto-download public CGS or comparator data.
- Do not copy or extract third-party assets.
- Do not mutate `GameState`, decks, runtime packs, or custom pack staging.
- Local import JSON parsing is read-only and best-effort.

## Verification

EditMode tests should cover:

- runtime pack status formatting
- local validator report formatting
- invalid/missing local report fallback
- bounded warning/error output
- source boundary note always visible


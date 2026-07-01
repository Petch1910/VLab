# User Deck Asset Slot Spec

Milestone: `M22-06`

## Purpose

Support user-provided deck accessory asset slots through manifest, hash
validation, and fallback only.

## Scope

`M22-06` is a safe local validator slice:

- Define a user deck asset manifest schema.
- Validate slot names for deck appearance assets.
- Validate file paths stay inside the manifest root.
- Validate SHA-256 hashes before accepting files.
- Missing files and hash mismatches fall back safely.
- Path traversal rejects the manifest.

## Supported Slots

- `sleeve`
- `card_back`
- `playmat`
- `crest`
- `persona_shield`
- `gift_marker`
- `quick_shield`

## Non-Goals

- No automatic download.
- No official/comparator asset extraction.
- No image loading into UI yet.
- No Deck Builder visual preview yet.
- No online payload, Photon change, RulesCore change, deck validation change,
  Android work, app packaging, or release work.

## Acceptance

- Manifest JSON round-trips.
- Existing files resolve only when their SHA-256 matches the manifest.
- Missing files and hash mismatches create fallback warnings, not crashes.
- Path traversal creates an error and rejects the manifest.
- Unknown slots are ignored with warnings.

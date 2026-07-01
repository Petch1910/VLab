# Count-Line Deck Text Spec

Milestone: `M24-02`

## Purpose

Add a human-readable deck export/import format for local sharing and debugging.
This format complements the existing compact `VGTH1.` deck code and does not
replace it.

## Format

```text
# Vanguard Thai Sim Deck List
Name: Deck Name
Format: D
PackId: vanguard_th
PackVersion: 2026.06
PackDefinitionHash: hash-from-current-pack

[Main]
4 CARD-001

[Ride]
1 RIDE-001

[G]
1 G-001
```

## Rules

- Blank lines and comment lines starting with `#` are ignored.
- Zone headers are `[Main]`, `[Ride]`, and `[G]`.
- Card lines are `<quantity> <card_id>`.
- Quantity must be positive.
- `PackDefinitionHash` is optional for older text exports, but new exports
  should include it when the current manifest is available.
- Import creates a new `VanguardDeck` only after the full text validates.
- Import must not mutate the active deck when parsing fails.

## UI

Deck Tools keeps existing compact deck code buttons and adds:

- `Copy Text`
- `Apply Text`

Both use the same multi-line input box.

## Acceptance

- Codec round-trips Main/Ride/G entries and metadata.
- Invalid text rejects without producing a partial deck.
- Deck Tools can copy/apply count-line text.
- Existing `VGTH1.` deck code behavior remains unchanged.

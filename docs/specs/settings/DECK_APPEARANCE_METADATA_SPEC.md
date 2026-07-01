# Deck Appearance Metadata Spec

Milestone: `M22-02`

## Purpose

Add a serializable deck appearance metadata model for Windows-first deck
accessories. This is data/model work only. The Deck Builder accessories dialog
is `M22-04`, and user-provided asset manifests are `M22-06`.

## Fields

`DeckAppearanceMetadata` stores semantic asset keys, not filesystem paths:

- `sleeve_key`: sleeve selection key.
- `card_back_key`: card back selection key.
- `playmat_key`: playmat selection key.
- `crest_key`: crest selection key.
- `persona_shield_key`: persona shield selection key.
- `gift_marker_key`: gift marker selection key.
- `quick_shield_key`: quick shield selection key.

Default value for all fields: `default`.

## Rules

- Appearance metadata is cosmetic only and must not affect deck legality.
- Keys are local semantic ids, not paths or URLs.
- Keys must be trimmed, bounded, and limited to safe key characters.
- Invalid, blank, path-like, or traversal-like keys fall back to `default`.
- The model must support JSON round-trip through Unity `JsonUtility`.
- Loading empty or invalid JSON must fall back to defaults.
- Normalization must not mutate the source object.
- Existing legacy `DeckCosmetics` data remains loadable; it is not removed in
  this milestone.
- No UI screen, asset loading, online payload, bot logic, Android flow, app
  packaging, or release work is part of `M22-02`.

## Acceptance

- `DeckAppearanceMetadata.CreateDefault()` returns valid defaults.
- `DeckAppearanceMetadata.Normalize()` trims safe keys and falls back unsafe
  keys.
- `DeckAppearanceMetadata.FromJson()` round-trips valid settings and falls back
  for missing/invalid JSON.
- `VanguardDeck` can carry `appearance` metadata through JSON/deck-code
  round-trip.
- Deck validation output is unchanged when only appearance metadata changes.

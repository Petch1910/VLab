# Player Settings Spec

Milestone: `M22-01`

## Purpose

Add a small, serializable player settings model for Windows-first local
preferences. This is data/model work only. The visible Settings screen is
`M22-03`.

## Fields

`PlayerSettings` stores:

- `player_name`: local display name. Default: `Player`.
- `default_deck_id`: optional local deck id/path key. Default: empty.
- `preferred_format`: preferred rules/deck format. Default: `D`.
- `ui_scale`: local UI scale multiplier. Default: `1.0`, clamped to `0.75..1.5`.
- `image_cache_mode`: local image/cache behavior. Default: `Balanced`.

## Rules

- Settings are cosmetic/local preferences and must not affect deck legality.
- The model must support JSON round-trip through Unity `JsonUtility`.
- Loading empty or invalid JSON must fall back to defaults.
- Normalization must not mutate the source object.
- No UI screen, filesystem storage, online payload, bot logic, Android flow,
  app packaging, or release work is part of `M22-01`.

## Acceptance

- `PlayerSettings.CreateDefault()` returns valid defaults.
- `PlayerSettings.Normalize()` clamps scale, trims text, and falls back invalid
  enum values.
- `PlayerSettings.FromJson()` round-trips valid settings and falls back for
  missing/invalid JSON.
- EditMode tests cover defaults, JSON round-trip, normalization, and source
  non-mutation.

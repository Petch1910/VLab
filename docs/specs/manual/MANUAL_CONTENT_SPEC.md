# In-App Manual Content Spec

Milestone: `M23-01`

## Purpose

Define original in-app manual and tutorial content for the Windows program.
This spec prepares the content structure for `M23-02` Manual screen
implementation.

## Content Principles

- Write original text for this project.
- Use comparator products only as workflow references, not text sources.
- Keep sections short enough for in-app reading.
- Prefer player-facing language over developer terminology.
- Do not expose hidden implementation details, payload ids, private card ids,
  or debug terminology in the player manual.
- Keep Windows-first scope. Do not add Android/mobile instructions in this
  milestone.

## Manual Structure

The manual has two top-level groups:

1. App Guide
2. Vanguard Rules Basics

Each section must have:

- stable `section_id`
- `title`
- `category`
- short body text
- optional related screen name
- optional loading tip candidates

## App Guide Sections

### Home

Purpose:

- Explain that Home is the entry point for Solo Play, Deck Builder, Card
  Browser, Online Room, Settings, and later Replay/CPU features.

Must mention:

- Use Deck Builder first when no playable deck exists.
- Settings are local/session-oriented until persistence is added.
- Online Room is casual trusted-client mode.

### Card Browser

Purpose:

- Explain searching, series filters, clan/nation filters, card details, and
  image fallback.

Must mention:

- Card data comes from the local Vanguard TH runtime pack.
- Some image surfaces may show fallback if an image path is missing or not
  provisioned.

### Deck Builder

Purpose:

- Explain adding/removing cards, Main/Ride/G zones, deck status, validation
  warnings/errors, Deck Tools, and Deck Type / Accessories.

Must mention:

- Cosmetics do not affect legality.
- Deck code import/export is for sharing local deck lists.
- Start Game requires a playable deck.

### PlayTable

Purpose:

- Explain the manual table flow: setup, first Vanguard, mulligan, phase
  buttons, hand strip, selected-card preview, board zones, common actions,
  battle flow status, match log, and Advanced drawer.

Must mention:

- Most card effects still require manual resolution unless structured support
  exists.
- Advanced drawer contains diagnostics/automation helpers and is hidden by
  default.

### Online Room

Purpose:

- Explain casual friend-room flow: connect, host/join, trust warning, deck/pack
  readiness, room status, start table, reconnect, rematch later.

Must mention:

- Current room mode is trusted-client and not ranked-secure.
- Private deck/hand information must remain hidden from opponent/spectator
  views.

### Replay

Purpose:

- Explain that replay/log viewing is based on player-readable event logs and
  will expand in later UI milestones.

Must mention:

- Logs should avoid private ids and raw protocol payloads.

### Custom Packs

Purpose:

- Explain local custom pack validation, isolated imports, unsupported field
  warnings, and copyright note.

Must mention:

- No auto-download of public third-party pack data.
- User-provided assets require manifest/hash/fallback validation.

## Vanguard Rules Basics Sections

### Playing Field

Cover:

- Vanguard circle.
- Rear-guard circles.
- Guardian circle.
- Deck, hand, drop, damage, soul, bind, order, trigger zone, ride deck, G zone.

### Turn Flow

Cover:

- Stand / Draw.
- Ride.
- Main.
- Battle.
- End.
- First-player attack restriction at a high level.

### Deck And Setup

Cover:

- Main deck, ride deck, G zone where relevant by format.
- Opening hand and mulligan.
- First Vanguard setup.

### Combat Basics

Cover:

- Attack declaration.
- Target selection.
- Boost.
- Guard step.
- Drive check.
- Damage check.
- Battle resolution.
- Close step.

### Triggers

Cover:

- Critical.
- Draw.
- Front.
- Heal.
- Over.
- Legacy Stand when relevant.
- Trigger power/critical allocation at a high level.

### Resources

Cover:

- Counter-Blast.
- Counter-Charge.
- Soul.
- Soul-Blast.
- Energy where relevant.
- Once-per-turn / once-per-fight concepts at a high level.

### Formats

Cover:

- Standard / D.
- V-Premium.
- Premium.
- Format features are controlled by ruleset profiles, not ad hoc UI toggles.

### Markers And Tokens

Cover:

- Gift markers: Force, Accel, Protect.
- Quick Shield.
- Crest and persona-related UI concepts.
- Explain only at a high level; card-specific text still decides exact behavior.

## Loading Tips

Initial tip candidates:

- "A playable deck is required before Solo Play can start."
- "Use the selected-card preview to confirm what the next action will affect."
- "Cosmetic accessories do not change deck legality."
- "Online rooms are casual trusted-client rooms in this build."
- "Missing private assets fall back instead of blocking play."
- "The Advanced drawer is hidden until diagnostics are needed."
- "Card images come from the local runtime pack."
- "Manual resolution is expected for unsupported card effects."

## Data Shape For M23-02

The runtime content can use a plain serializable model:

```text
ManualSection
- section_id
- category
- title
- body
- related_screen
- loading_tip
```

`M23-02` may start with embedded C# section definitions before moving to
external JSON if needed.

## Acceptance

- Manual content categories and section ids are defined.
- App Guide and Vanguard Rules Basics are both covered.
- Loading tip candidates are defined.
- Non-goals and copyright/comparator boundaries are explicit.
- Next milestone can implement a Manual screen without re-deciding content
  scope.

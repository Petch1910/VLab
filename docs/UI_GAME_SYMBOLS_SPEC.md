# UI Game Symbols Spec

## Purpose

This spec defines how Vanguard-specific symbols should appear in the UI without
extracting proprietary assets from VangPro, Vanguard Area, Dear Days, Dear Days
2, or official websites.

The goal is recognizable gameplay language, not copied artwork.

## Legal And Asset Boundary

Do not extract, copy, trace, recreate pixel-for-pixel, or import:

- trigger icons from VangPro
- sprites/icons/backgrounds from Vanguard Area
- Dear Days or Dear Days 2 UI assets
- official website logos, markers, icon art, or card-frame artwork

Allowed:

- study which symbol categories exist
- use official rule terms in text where needed
- create original icons in our own visual style
- use open-source icons documented in `docs/UI_ICON_SYSTEM_SPEC.md`
- use English text labels such as `CRITICAL`, `DRAW`, `FRONT`, `HEAL`, `OVER`
  as the default trigger symbols
- support private user-provided overrides through
  `docs/UI_ICON_OVERRIDE_SPEC.md`, without extracting or committing those assets

## Symbol Style

- Primary trigger treatment: text-only English badges.
- Optional UI icon base: Lucide line icons from
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/`.
- Treatment: our own badge frame, color token, and English label.
- Avoid copying official shape silhouettes. If a mechanic has a famous official
  silhouette, use a different abstract metaphor plus a text label.
- Every symbol should work at 24px, 32px, and 48px.

## Trigger Symbols

Use text labels as the default visual representation. Icons are optional
secondary decoration only.

| Symbol Key | Default Text Label | Optional Lucide Base | Intended Meaning |
| --- | --- | --- | --- |
| `trigger_critical` | `CRITICAL` | `swords.svg` or `flame.svg` | Extra pressure / critical damage |
| `trigger_draw` | `DRAW` | `download.svg` or `card-sim.svg` | Add card to hand |
| `trigger_front` | `FRONT` | `arrow-up.svg` | Front row power increase |
| `trigger_heal` | `HEAL` | `heart-pulse.svg` or `plus.svg` | Heal / recovery |
| `trigger_over` | `OVER` | `sparkles.svg` or `circle-star.svg` | Over trigger / major swing |
| `trigger_stand_legacy` | `STAND` | `rotate-cw.svg` | Legacy stand/restand concept |

Do not use official trigger icons. The English label is the symbol. This is
clearer for Thai/English mixed UI, easier to render on every platform, and does
not depend on protected icon artwork.

## Marker And Rule Symbols

| Symbol Key | UI Label | Lucide Base | Intended Meaning |
| --- | --- | --- | --- |
| `marker_force` | `FORCE` | `swords.svg` | Offensive gift marker |
| `marker_accel` | `ACCEL` | `zap.svg` or `activity.svg` | Extra attack/tempo marker |
| `marker_protect` | `PROTECT` | `shield.svg` | Defensive gift marker |
| `marker_quick_shield` | `Q-SHIELD` | `shield-plus.svg` | Quick Shield ticket/card |
| `marker_persona` | `PERSONA` | `star-check.svg` | Persona Ride state |
| `marker_crest` | `CREST` | `badge-check.svg` or `crown.svg` | Crest/rules object |
| `marker_energy` | `ENERGY` | `zap.svg` or `circle-gauge.svg` | Energy/resource |
| `marker_gauge` | `GAUGE` | `gauge.svg` | Generic gauge/counter |

## Card Type And Zone Symbols

| Symbol Key | UI Label | Lucide Base |
| --- | --- | --- |
| `card_unit` | `UNIT` | `card-sim.svg` |
| `card_order` | `ORDER` | `scroll-text.svg` |
| `card_trigger` | `TRIGGER` | `sparkles.svg` |
| `zone_deck` | `DECK` | `wallet-cards.svg` |
| `zone_hand` | `HAND` | `hand.svg` |
| `zone_drop` | `DROP` | `trash-2.svg` |
| `zone_damage` | `DAMAGE` | `shield-alert.svg` |
| `zone_soul` | `SOUL` | `orbit.svg` |
| `zone_bind` | `BIND` | `target.svg` |
| `zone_replay` | `REPLAY` | `scroll-text.svg` |

## UI Implementation Guidance

The runtime UI should not hard-code filenames or display strings everywhere.
Add a small symbol registry in M19 that maps semantic keys such as
`trigger_critical` to:

- default text label
- optional icon asset name
- color token
- tooltip text
- optional accessibility label

Recommended first-pass color tokens:

- Critical: red/orange accent
- Draw: blue accent
- Front: gold/green accent
- Heal: green accent
- Over: violet/gold accent
- Protect/guard: cyan/steel accent
- Debug/advanced: neutral gray

Current implementation:

- `UiGameSymbolRegistry` maps trigger, marker, card-type, and zone semantic keys
  to safe default text labels, optional Lucide fallback assets, color tokens,
  and tooltips.
- `UserIconPackValidator` validates private user-provided override manifests
  from `client/unity/VanguardThaiSim/Assets/UI/Icons/UserProvided/`.
- Missing private files keep the UI on default English text badges; invalid
  paths outside the user-provided icon folder are rejected.
- Home/Lobby trigger badges now use semantic keys instead of hard-coded
  trigger strings.

## Acceptance Criteria

- No proprietary icon or sprite is imported.
- Trigger, marker, card type, and zone symbols have semantic keys.
- Icons can be swapped later without changing game logic.
- Any future custom-drawn or generated symbol set must be visually distinct from
  official/comparator assets and documented here before import.

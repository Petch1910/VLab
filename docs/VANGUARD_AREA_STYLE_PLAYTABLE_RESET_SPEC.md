# Vanguard Area-Style PlayTable Reset Spec

Milestone: `M32`

## Purpose

User/team feedback is that the current Windows PlayTable is still hard to use
because it feels like a dense dashboard instead of a manual Vanguard table.
This spec pivots the active UI work away from small Card Workshop polish and
back to the PlayTable.

The new baseline is a Vanguard Area / Dear Days-style manual simulator mental
model: the field/table is the product, while commands and diagnostics stay
secondary.

Active UX blueprint: `docs/VANGUARD_DIGITAL_CLIENT_UX_BLUEPRINT.md`.

## Non-Copy Boundary

Vanguard Area can be used only as a UX/layout reference.

Do not copy:

- playmat image files
- sprites, icons, logos, frames, or backgrounds
- source code or updater data
- card data or deck data

The implementation must use original Unity UI panels, our KK Card Fight Thai
runtime pack, and our existing command facade.

## Current Problem

The M31 visual evidence shows the PlayTable still has these issues:

- Primary action rows are wider than the available table area.
- Fixed-width buttons push the side panel into a thin unreadable strip.
- Board zones are visually secondary to large green command buttons.
- The first visible surface is not a recognizable Vanguard field.
- Player guidance text, setup status, battle status, zone status, and logs are
  crowded into one side panel.
- The right-side inspect panel still behaves like a dashboard column instead of
  an overlay/HUD, so it competes with the field.
- The current M32-01 evidence improves the field shell, but the command dock
  and inspect overlay still dominate the board more than the comparator
  gameplay references.
- Hand cards can be clipped at the bottom in the 1280x720 evidence.
- Trigger check and order zones are not yet explicit enough as first-class
  table surfaces.

## Target Layout

### Field First

The central area becomes a freeform field mat:

- opponent vanguard near top center
- opponent rear-guard row near top
- local vanguard near lower center
- local rear-guard row near lower area
- damage, deck, ride deck, soul, drop, bind, and gift markers around the field
- local hand strip under the field
- phase rail on the left side of the field: Stand, Draw, Ride, Main, Battle,
  End

The field mat can be simple dark panels and labels. It must not use copied
Vanguard Area art.

### Compact Commands

Commands move into a compact dock under the field/above the hand strip. Phase
commands belong in the left field rail, not in the bottom command dock.

Rules:

- two short rows max
- short labels where needed (`RG`, `DmgChk`, `AtkVG`)
- no command row may force the field wider than the screen
- commands continue to call the existing RulesCore/session paths

### Inspect Overlay

The inspect surface becomes an overlay/HUD, not a main layout column. It must
not shrink the field.

Primary contents:

- selected card / selection status
- selected card preview
- next action
- compact match log
- Advanced drawer button/details

Move these to Advanced:

- action group legend
- setup detail
- battle flow detail
- manual notes
- zone status text
- bot plan
- online debug
- trigger/ability diagnostics
- full match log

## Implementation Slices

- `M32-01`: PlayTable field-mat shell and compact command dock.
- `M32-01b`: UX blueprint and reference-capture inventory.
- `M32-02`: Zone placement polish and board-card sizing.
- `M32-03`: PlayTable de-dashboard field/HUD pass.
- `M32-04`: Playmat slot layout and Windows visual evidence closeout.
- `M32-05`: Hand strip and compact pile interaction polish.

## Acceptance Criteria For M32-05

- The local hand strip is fully visible at 1280x720 and reads as cards in hand,
  not clipped debug buttons.
- The field remains clean while compact pile zones such as Deck and Ride Deck
  have a usable expanded/overlay path for manual selection.
- Pile markers do not print overflowing card ids or counts into adjacent zones.
- The implementation uses original Unity UI only; comparator screenshots are
  layout references, not asset sources.
- Existing session/RulesCore command behavior is preserved.
- Unity EditMode tests, Windows build, Windows player smoke, and refreshed
  visual evidence pass.

## Acceptance Criteria For M32-04

- Rear-guard presentation is no longer a long bar; the field shows visible
  front/back playmat slots for both players.
- Local and opponent VG slots are distinct from RG slots.
- Deck, Drop, Damage, Order, Bind, Ride Deck, Trigger Zone, Gift Marker, and
  Soul are placed around the field as table markers rather than dashboard
  panels.
- Compact pile zones do not overflow into adjacent zones.
- Comparator playmats, official icons, code, and extracted data are not copied
  into the project.
- Existing session/RulesCore command behavior is preserved.
- Unity EditMode tests, Windows build, Windows player smoke, and refreshed
  visual evidence pass.

## Acceptance Criteria For M32-03

- The main field/table no longer reserves horizontal space for a permanent
  side dashboard.
- The default Inspect surface is a small overlay/HUD, not a full-height column.
- Recent/full logs, setup detail, zone status, bot, online, trigger, pending
  AUTO, and ability diagnostics are hidden in Advanced or collapsible surfaces.
- The default view prioritizes field zones, phase rail, hand strip, and
  contextual action bar.
- Right-side field zones reserve a gutter for the compact Inspect HUD; Trigger,
  Drop, Bind, and Gift Marker zones must not sit under the HUD.
- Gift marker counts fit inside the marker panel and do not overflow into the
  Trigger Zone.
- Story, character, dialogue, campaign, copied assets, official icons, playmats,
  code, and extracted data remain out of scope.
- Existing session/RulesCore command behavior is preserved.
- Unity compile, EditMode tests, Windows player smoke, and refreshed visual
  evidence pass.

## Acceptance Criteria For M32-02

- Field mat becomes the dominant screen area at 1280x720.
- Command dock is visually secondary and does not dominate the board.
- Trigger check zone and order zone are visible as explicit table zones.
- Hand strip is fully visible and not clipped.
- Inspect overlay is narrower or less visually dominant than M32-01.
- Existing command/session/RulesCore behavior is preserved.
- Unity compile, EditMode, Windows build, player smoke, and refreshed visual
  evidence pass.

## Acceptance Criteria For M32-01

- PlayTable visual evidence at 1280x720 shows the field/zone area first.
- Primary command rows fit within the available table width.
- Inspect overlay remains readable and does not consume field layout width.
- Phase controls are visible as a left-side field rail.
- Debug/automation/network details remain hidden in Advanced by default.
- No RulesCore, card data, deck validation, or Photon protocol changes.
- Unity compile, EditMode tests, client smoke, Windows build, player smoke, and
  refreshed visual evidence pass.

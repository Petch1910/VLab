# PlayTable Windows UX V2 Spec

## Purpose

`M21-01` defines the next PlayTable pass for Windows. The table must become a
player-facing Vanguard play surface first, with debug, automation, and network
diagnostics available only through Advanced surfaces.

This is a spec-only task. Implementation starts in `M21-02`.

## Design Baseline

Use these references as mental models only:

- Vanguard Area: board/table first, visible zones, manual freedom, replay files.
- VangPro: clear player status, mode separation, focused dialogs, readable deck
  readiness.

Do not copy comparator assets, exact layouts, code, sprites, icons, playmats,
pack files, or proprietary data.

## Primary Goal

The first thing a player sees in PlayTable should be the game board, not the
toolbar.

The table should answer these questions without opening debug panels:

- What phase/turn/player is active?
- Where are my main zones?
- How many cards are in each zone?
- What card is selected?
- What can I do next manually?
- What just happened in the match log?

## Required Layout

### Board Area

The central board/table area must be the dominant visual element.

Required visible regions:

- Opponent field summary at the top.
- Opponent front/back row circles when available.
- Local front/back row circles near the bottom.
- Vanguard circle for each player.
- Local hand strip.
- Zone piles/counts for deck, drop, damage, soul, bind, order, ride deck, and
  trigger zone.
- Selected-card preview.

The board can use simple flat panels and text labels for now. Custom playmat
art is not required for M21 and must not be copied from comparator products.

### Zone Counts And Status

Every zone panel must make count/status readable without selecting the zone.

Minimum fields:

- Zone label.
- Card count.
- Top/selected card label if visible to the viewer.
- Hidden placeholder if the viewer should not know the card identity.

Private or hidden zones must use masked display state, not true-state data.

### Hand Strip

The local hand should be a horizontal strip near the player's side of the board.

Minimum behavior:

- Cards are selectable.
- Selected card updates the selected-card preview.
- Long names may truncate visually, but the preview must show the full card id
  and known display name where available.

### Selected-Card Preview

The selected-card preview should be readable without opening the card browser.

Minimum fields:

- Card id.
- Display name if available.
- Current zone.
- Known type/grade/power/shield if available from runtime card data.
- Current action hint if a common action can use the selected card.

## Common Actions

Primary PlayTable actions should use player-facing labels:

- Draw.
- Ride.
- Call.
- Move.
- Drive Check.
- Damage Check.
- Guard / Manual Note.
- End Phase or Next Phase.
- Undo only when legal and local.

Action controls should be near the board or selected-card preview, not mixed
with network payloads or automation internals.

Rejected actions must show player-facing reasons and must not mutate
`GameState`.

## Advanced Drawer

These surfaces must stay hidden by default:

- Pending AUTO queue details.
- Trigger draft payload details.
- Manual resolution payload details.
- Apply preview internals.
- Photon/public event payloads.
- Network cursor raw diagnostics.
- Developer-only replay/event JSON.

Advanced can expose diagnostics for development, but the default player flow
must remain clean.

## Event / Replay Log

The default log should read like a match record, not a reducer trace.

Preferred examples:

- `P1 drew 1 card.`
- `P1 moved BT01-001TH from hand to vanguard.`
- `P2 checked a trigger.`
- `P1 ended the turn.`

Avoid primary display text such as:

- raw payload names
- event protocol ids
- private card instance ids
- hidden source ids
- internal queue ids

Developer details may remain in Advanced only.

## Rules, Safety, and Navigation Flow

- **Full-Screen Transition:** Starting a match (Solo, Online, Replay) automatically triggers a borderless full-screen transition, unloading standard menu groups to maximize space for zones, card visuals, and hand strip. Conceding or ending the match returns the player to the Battle Center lobby cleanly.
- **Manual Drawer Overlay:** The game manual can be opened directly over the PlayTable (by clicking a top menu icon or pressing `ESC`). It slides out as an overlay drawer without pausing the network session or disconnecting from Photon.
- **No Direct GameState Mutation:** The UI reads from the masked display state/view models and sends commands via the RulesCore command facade.
- - Online display must not leak private/opponent information.
- - Deck Validation, Photon network protocols, and structured triggers remain unaffected.

## M21 Implementation Slices (Aligned V2)

- `M21-02`: Expand board/table area so it is more prominent than the toolbar. **Done.**
- `M21-03`: Add zone count/status for deck, hand, drop, damage, soul, bind, order, ride deck, and trigger zone. **Done.**
- `M21-04`: Add hand strip and selected-card preview. **Done.**
- `M21-04b`: Audit and wire the existing Soul zone model into status/rules paths
  that still treat Soul as unavailable. **Done.** Scope: zone status, visible
  Soul panel, and ResourceLedger Soul count previews.
- `M21-04d`: Add event-sourced ride-to-soul and live SoulBlast/SoulCharge command
  coverage before visual thumbnail polish. **Done.**
- `M21-04c`: Render minimal card image/thumbnail on board circles. **Done.**
- `M21-05`: Convert common actions to phase-enforced controls (Stand, Draw, Ride, Main, Battle with Attack sequence flow, End). **Done through setup guidance.**
- `M21-05a`: Common action availability model and phase buttons. **Done.**
- `M21-05a2`: Player-facing Check/Guard surface. **Done.**
- `M21-05a3`: Drive/Damage trigger-check source split. **Done.**
- `M21-05a4`: Attack selected Vanguard/rear-guard to opponent Vanguard
  shortcut. **Done.**
- `M21-05a5`: Opponent target selection and `Atk Target` action. **Done.**
- `M21-05a6`: Battle Flow status guidance panel. **Done.**
- `M21-05a7`: Manual Note local PlayTable surface. **Done.**
- `M21-05b`: Add Game Setup Wizard (deck selection, vanguard placement, mulligan loop). **Done through setup guidance.**
- `M21-05b1`: Setup readiness guard before PlayTable entry. **Done.**
- `M21-05b2`: First Vanguard setup from Ride Deck. **Done.**
- `M21-05b3`: Selected hand-card mulligan surface. **Done.**
- `M21-05b4`: Stand/Ride phase legal action completion. **Done.**
- `M21-05b5`: Setup status and finish guidance. **Done.**
- `M21-06`: Keep debug/automation/network payload in Advanced only. **Done.**
- `M21-07`: Make event/replay log player-readable. **Done.**
- `M21-08`: Add tests and Windows player smoke. **Done.**
- `M21-09`: Close out the Windows PlayTable UX pass. **Done.**

## Verification For M21-01

This task is docs-only. Required verification:

- Spec exists.
- Index references the spec.
- Current implementation plan marks `M21-01` complete. After `M21-05a`, the
  current next target is continuing `M21-05`.
- No Unity compile/EditMode/Windows player smoke is required until code changes
  begin in `M21-02`.

## Verification For M21-02

See `docs/history/M21_02_PLAYTABLE_BOARD_FIRST_CLOSEOUT.md`.

## Verification For M21-03

See `docs/history/M21_03_PLAYTABLE_ZONE_STATUS_CLOSEOUT.md` and
`docs/history/M21_04_HAND_STRIP_SELECTED_PREVIEW_CLOSEOUT.md`.

## Verification For M21-04b

See `docs/history/M21_04B_SOUL_STATUS_LEDGER_CLOSEOUT.md`.

## Verification For M21-04d

See `docs/history/M21_04D_RIDE_SOUL_RESOURCE_COMMAND_CLOSEOUT.md`.

## Verification For M21-04c

See `docs/history/M21_04C_BOARD_THUMBNAIL_CLOSEOUT.md`.

## Verification For M21-05a

See `docs/history/M21_05A_ACTION_AVAILABILITY_CLOSEOUT.md`.

## Verification For M21-05a2

See `docs/history/M21_05A2_CHECK_GUARD_SURFACE_CLOSEOUT.md`.

## Verification For M21-05a3

See `docs/history/M21_05A3_TRIGGER_SOURCE_SPLIT_CLOSEOUT.md`.

## Verification For M21-05a4

See `docs/history/M21_05A4_ATTACK_VANGUARD_SURFACE_CLOSEOUT.md`.

## Verification For M21-05a5

See `docs/history/M21_05A5_ATTACK_TARGET_SELECTION_CLOSEOUT.md`.

## Verification For M21-05a6

See `docs/history/M21_05A6_BATTLE_FLOW_STATUS_CLOSEOUT.md`.

## Verification For M21-05a7

See `docs/history/M21_05A7_MANUAL_NOTE_SURFACE_CLOSEOUT.md`.

## Verification For M21-05b1

See `docs/history/M21_05B1_SETUP_READINESS_GUARD_CLOSEOUT.md`.

## Verification For M21-05b2

See `docs/history/M21_05B2_FIRST_VANGUARD_SETUP_CLOSEOUT.md`.

## Verification For M21-05b3

See `docs/history/M21_05B3_MULLIGAN_SELECTED_CLOSEOUT.md`.

## Verification For M21-05b4

See `docs/history/M21_05B4_PHASE_ACTIONS_CLOSEOUT.md`.

## Verification For M21-05b5

See `docs/history/M21_05B5_SETUP_STATUS_GUIDANCE_CLOSEOUT.md`.

## Verification For M21-06

See `docs/history/M21_06_ADVANCED_DEBUG_SURFACE_CLOSEOUT.md`.

## Verification For M21-07

See `docs/history/M21_07_PLAYER_READABLE_EVENT_LOG_CLOSEOUT.md`.

## Verification For M21-08

See `docs/history/M21_08_TESTS_WINDOWS_SMOKE_CLOSEOUT.md`.

## Verification For M21-09

See `docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md`.

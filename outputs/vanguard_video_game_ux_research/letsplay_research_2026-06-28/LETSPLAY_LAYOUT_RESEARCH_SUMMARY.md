# Vanguard Letsplay Layout Research Summary

Date: 2026-06-28

Scope: gameplay layout, card placement, field zones, hand presentation,
phase/action prompts, online table status, and deck-builder structure only.

Out of scope for the current Windows PlayTable reset:

- Story scenes.
- Character portraits.
- Dialogue windows.
- Campaign/progression structure.
- Proprietary assets, official icons, playmats, card art, fonts, frames, logos,
  code, or extracted game data.

## Tooling

`yt-dlp` was installed and used only for playlist/video metadata indexing.

- Version: `2026.06.09`
- Executable:
  `C:\Users\Phet\AppData\Roaming\Python\Python314\Scripts\yt-dlp.exe`
- Python module: `python -m yt_dlp`

Reference screenshots were captured from public YouTube playback in the browser
for UX study only. They are evidence, not source assets.

## Sources Indexed

Playlist/video metadata is stored in:

- `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/yt_dlp_metadata/summary.json`

Captured frame manifest:

- `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/representative_frame_manifest.json`

Indexed public sources:

- `https://www.youtube.com/playlist?list=PL2svYrQjTcDEdBCsX5ZjJRh2TH5TR_dRm`
  - Dear Days 2 playthrough, 22 entries.
- `https://www.youtube.com/playlist?list=PLszdFHLerl-Svw3DeSjkcWa7KRD2Cl-wr`
  - Dear Days, 100 entries in flat metadata.
- `https://www.youtube.com/playlist?list=PLRZKUkcKaqulY3P6mosmcAb7Lkkf1UlyL`
  - Ride to Victory, 100 entries in flat metadata.
- `https://www.youtube.com/playlist?list=PL3-yd1T-iOeWF_HZJ6w4GxXaTP14kRz2h`
  - Lock on Victory, 11 entries.
- `https://www.youtube.com/playlist?list=PLXPDvkva8Ps63Fn5pIaf7tFMyPDMuGgqB`
  - Stride to Victory, 100 entries in flat metadata.
- `https://www.youtube.com/playlist?list=PLZU6EU91STfUcLSt1kCLAdDwnN_rzzu-8`
  - Vanguard EX, 38 entries.
- `https://www.youtube.com/watch?v=diNDfXkEn5g`
  - Cardfight Online closed beta stream.
- `https://www.youtube.com/watch?v=7VrwUcPQW8c`
  - Cardfight Online no-commentary stream.

## Useful Captures

Useful for PlayTable layout:

- `captures/frame_dear_days_tutorial_300s.png`
- `captures/frame_ride_first_fight_300s.png`
- `captures/frame_cardfight_online_beta_300s.png`
- `captures/frame_cardfight_online_stream_300s.png`

Less useful for the current layout pass because they are story/title/dialogue
frames:

- `captures/frame_dd2_part1_300s.png`
- `captures/frame_lock_part1_300s.png`
- `captures/frame_stride_stream1_300s.png`
- `captures/frame_vanguard_ex_part1_300s.png`

## Layout Findings

### Battle Field

The battle field should own the screen. Dear Days and Cardfight Online both
place the field/playmat as the primary object, with HUD and actions around it.
The current Vanguard Thai Sim direction should stop treating the PlayTable as a
right-panel dashboard.

Implementation target:

- Field/playmat area should take roughly 75-85 percent of vertical attention.
- Permanent side panels should be minimized or removed.
- Status and action surfaces should be edge HUDs or temporary overlays.

### Zone Placement

Observed pattern:

- Vanguard and rear-guard circles are centered on the mat.
- Deck, ride deck, drop, damage, order, and trigger zones are anchored as piles
  near board edges.
- Counts appear on or next to the zone pile.
- Trigger check zone is a physical table zone, not just log text.

Implementation target:

- Every zone must look like a place on the table, not only a list label.
- The player should understand where a card will move before pressing an action.
- Damage, drop, deck, ride deck, order, and trigger zones should be edge
  clusters.

### Phase And Turn Status

Observed pattern:

- Dear Days uses a left phase rail and large current-phase emphasis.
- Cardfight Online uses a compact left phase stack plus top waiting/status
  overlay.
- 3DS titles use a central prompt for important state changes.

Implementation target:

- Current phase must be visible near the field.
- Phase rail/status should be compact and persistent.
- Major state changes can use temporary field-centered prompts.

### Hand

Observed pattern:

- The local hand is at the bottom and visually attached to the local player.
- Cards are large enough to identify.
- Selected cards are highlighted in the hand, not only described in text.

Implementation target:

- Keep a wide bottom hand strip.
- Avoid clipping card faces.
- Selected-card preview can be temporary, but the selected card must remain
  obvious in the hand.

### Actions

Observed pattern:

- Actions are contextual: ride, attack, guard, no guard, drive check, damage
  check, confirm, cancel.
- The action UI appears near the current decision, not as a permanent large
  toolbar.

Implementation target:

- Replace always-visible command rows with a compact contextual action bar.
- Guard/no-guard and trigger/check actions must surface as player choices.
- Advanced/debug/automation/network payloads stay hidden.

### Inspect And Logs

Observed pattern:

- Card details appear when selected or requested.
- Battle logs are not the primary surface during live play.
- The table remains readable even without reading a long side panel.

Implementation target:

- Inspect should be an overlay or drawer, not a full-height permanent column.
- Match log should move to Advanced or become a small collapsible recent-event
  strip.
- Default view should answer: whose turn, what phase, what can I click, where
  are my cards.

### Online Table

Observed pattern:

- Player/opponent name, damage, hand/deck/drop counts, timer, and turn state are
  edge HUD elements.
- Waiting state is a center/top overlay.
- Online status does not expose transport payloads.

Implementation target:

- Online PlayTable should keep Photon/debug details out of the default view.
- Show player-facing connection/turn/waiting state only.
- Keep public/private state masking unchanged.

## Consequence For M32

The next implementation target should not be a narrow inspect-text tweak. It
should be a PlayTable de-dashboard pass:

1. Remove or collapse the permanent right Inspect column.
2. Move selected-card detail, match log, setup guidance, bot explanation,
   trigger diagnostics, pending AUTO, and network status into overlays,
   collapsible drawers, or Advanced.
3. Promote field zones, hand strip, phase rail, and contextual action bar.
4. Keep all actions routed through existing session/RulesCore paths.
5. Do not introduce story, character, dialogue, campaign, or copied asset scope.

## Acceptance Gate For The Next Visual Pass

At 1280x720:

- The first impression is a Vanguard table, not a developer dashboard.
- The board is readable before reading any side text.
- Hand, deck, ride deck, damage, drop, order, and trigger zone are visible.
- Current phase and next action are visible near the field.
- Default view has no transport payload, raw event ids, or long debug logs.
- Story/character/dialogue UI is absent from this milestone.

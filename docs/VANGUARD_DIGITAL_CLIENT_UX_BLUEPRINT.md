# Vanguard Digital Client UX Blueprint

Date: 2026-06-28

Purpose: convert observed UX patterns from Vanguard Area, VangPro,
Cardfight Connect, Vanguard EX, Vanguard Dear Days, and Vanguard Dear Days 2
into implementable guidance for Vanguard Thai Sim.

This is a reference blueprint, not an asset source. Use it to shape layout,
flow, information hierarchy, and task design. Do not copy proprietary assets,
icons, playmats, card art, logos, executable code, packed content, or private
game data from any comparator product.

Current M32 scope is layout-only. Use the references for card placement,
field zones, hand presentation, phase/action prompts, trigger/check/guard
surfaces, online table status, and deck-builder structure. Do not use story
scenes, character portraits, dialogue windows, or campaign presentation as
targets for the current PlayTable reset.

## Source Boundary

Allowed:

- Public screenshots and video frames as UX references.
- User-provided screenshots as UX references.
- Local screenshot captures stored under `work/` for analysis only.
- Existing safe inventory notes from comparator studies.
- Public product pages and public video pages as source links.

Not allowed:

- Extracting or redistributing assets from commercial games.
- Debugging, decompiling, patching, or unpacking commercial game binaries.
- Importing comparator card data as our source of truth.
- Recreating exact proprietary art direction, frames, icons, or UI chrome.

Current project source of truth remains:

- Card data: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- Card images: `outputs/kk_cardfight_export/data/images/`
- Runtime pack: `data/packs/vanguard_th/`

## Reference Captures

Local captures for current M32 comparison:

- Dear Days 2 gameplay reference:
  `work/youtube_vgdd2_ui_reference_-hoylnIvn0I/after_ad_wait.png`
- Dear Days 2 additional timestamp captures:
  `work/youtube_vgdd2_ui_reference_-hoylnIvn0I/gameplay_793s.png`
  through `gameplay_1740s.png`
- Current Vanguard Thai Sim PlayTable evidence:
  `client/unity/VanguardThaiSim/work/m32_03_de_dashboard_field_hud_visual_evidence_r4/play_table.png`

Some YouTube timestamp captures are not usable gameplay frames because the
clip moves into a deck-profile title card. Keep them only as audit evidence.
Use `after_ad_wait.png` as the main captured gameplay reference from the
current source.

Primary public references:

- Vanguard Dear Days 2 Steam:
  `https://store.steampowered.com/app/2457540/Cardfight_Vanguard_Dear_Days_2/`
- Vanguard Dear Days Steam:
  `https://store.steampowered.com/app/1881420/Cardfight_Vanguard_Dear_Days/`
- Vanguard EX official:
  `https://www.cs.furyu.jp/vanguard-ex/`
- Dear Days 2 gameplay/deck-profile video studied:
  `https://www.youtube.com/watch?v=-hoylnIvn0I&t=793s`
- Comparator study:
  `outputs/comparator_study/COMPARATOR_SYSTEM_UX_STUDY_TH.md`
- VangPro study:
  `outputs/vangpro_study/VANGPRO_STUDY_TH.md`
- Official games study:
  `outputs/vanguard_official_games_study/VANGUARD_DIGITAL_GAMES_STUDY_TH.md`
- Current public video-game reference capture manifest:
  `outputs/vanguard_video_game_ux_research/source_manifest.json`
- Current public video-game reference captures:
  `outputs/vanguard_video_game_ux_research/reference_captures/00_fandom_video_games_category.png`
  through
  `outputs/vanguard_video_game_ux_research/reference_captures/19_dear_days_2_google_images.png`
- Current letsplay layout research summary:
  `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/LETSPLAY_LAYOUT_RESEARCH_SUMMARY.md`
- Current letsplay metadata summary:
  `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/yt_dlp_metadata/summary.json`

The current public capture set covers:

- `Cardfight!! Vanguard: Ride to Victory!!`
- `Cardfight!! Vanguard: Lock on Victory!!`
- `Cardfight!! Vanguard G: Stride to Victory!!`
- `Cardfight!! Online`
- `Cardfight!! Vanguard EX`
- `Cardfight!! Vanguard Dear Days`
- `Cardfight!! Vanguard Dear Days 2`

## System Roles

### Legacy Portable Games

Covered titles:

- `Cardfight!! Vanguard: Ride to Victory!!`
- `Cardfight!! Vanguard: Lock on Victory!!`
- `Cardfight!! Vanguard G: Stride to Victory!!`

Use as compact information-design references.

Patterns to adopt:

- A dense board can still be readable if the current phase, target prompt, and
  selected card detail have strong priority.
- The selected card detail panel can be large and explicit without turning the
  whole screen into a debug dashboard.
- Phase banners and action prompts should appear close to the field, not only in
  a side log.
- Target selection needs visible feedback on the board itself.

Implementation impact:

- Keep PlayTable prompts short and field-adjacent.
- Preserve a compact selected-card inspect surface, but reduce permanent side
  text.
- Use player-facing action labels such as `Ride`, `Call`, `Attack`, `Guard`,
  `Drive Check`, and `Damage Check`.
- Do not copy 3DS UI art, frames, fonts, icons, or layouts exactly.

### Cardfight Online

Use as the online-table and lobby/status reference.

Patterns to adopt:

- The online table puts the board in the center and surrounds it with player
  HUD, deck/damage/drop/hand counts, and contextual response buttons.
- Guard decisions and no-guard/cancel choices are surfaced as player actions,
  not hidden in log text.
- Tutorial and preparation screens are separated from the live match.

Implementation impact:

- Online Room should show connection/readiness/cursor status, but the PlayTable
  should still look like a table first.
- Guard/response controls belong near battle flow status.
- Debug payloads and transport details remain Advanced-only.

### Vanguard Area

Use as the manual simulator mental model.

Patterns to adopt:

- Board-first play surface.
- Player artifacts are first-class: decks, replays, manual, settings.
- Manual correction must be easy.
- Clan/nation grouping is acceptable for the current deck/card browsing model.
- Table must not feel like a developer dashboard.

Implementation impact:

- PlayTable should prioritize the field, zones, piles, hand, phase, and counts.
- Advanced/debug panels must be hidden unless requested.
- Replay and manual notes are player tools, not just logs.

### VangPro

Use as the product workflow and deck-tool reference.

Patterns to adopt:

- Home/lobby shows selected deck, format, readiness, online status, and room
  actions.
- Deck Builder is a clear workbench: selected-card preview, searchable card
  pool, deck list, and rule-aware counters.
- Deck code/share workflow is explicit.
- Custom pack import uses data file + images + manifest/hash checks.
- Accessories are deck metadata: sleeves, playmat, crest/persona shield,
  markers.

Implementation impact:

- Keep Deck Builder as a dense but task-focused tool.
- Add useful import validation/status text.
- Do not mix cosmetic metadata with deck legality.

### Cardfight Connect

Use as an online-client reference only.

Patterns to adopt:

- Room/client status should be visible and player-facing.
- Image import/user-provided customization can be a future feature.
- Unity networking stack alternatives exist, but transport switching needs ADR.

Implementation impact:

- Keep Photon for now.
- Improve trusted-client room UX before considering Netcode/UGS.
- Online debug payloads stay hidden by default.

### Vanguard EX

Use only for battle/deck UI layout references in the current Windows-first
pass. Story, character, and campaign presentation are out of scope for M32.

Patterns to adopt:

- Tutorial missions can be small and measurable.
- The deck builder and battle table both keep a strong selected-card/detail
  surface while the board or deck grid stays central.

Implementation impact:

- Later tutorial lessons can be built from scripted mechanics:
  ride curve, call, attack order, guard, drive check, damage check, triggers.
- Deck Builder should not hide the active deck behind search controls.
- PlayTable can use compact field prompts for teaching without blocking manual
  freedom.

### Dear Days 1

Use as a tutorial and battle-table UX reference. Story/campaign mode structure
is deferred and not part of the current PlayTable reset.

Patterns to adopt:

- Tutorial is point-by-point and tied to real board actions.
- Cosmetics and collection are not deck legality.

Implementation impact:

- Keep one shared command/event/state system across modes.
- Manual/tutorial actions should reuse RulesCore commands.

### Dear Days 2

Use as the modern table/HUD baseline.

Observed from gameplay capture:

- The battle field owns almost the whole screen.
- Hand strip sits at the bottom and stays visually attached to the player.
- Deck, ride deck, damage, drop, order, and trigger zones are visible as piles
  around the board.
- Player and opponent HUDs show damage/deck/hand-like critical counts near the
  screen edges.
- Phase status is large and immediate, not buried in a side panel.
- Overlay prompts are centered only for major state changes.
- Command prompts are compact and contextual.

Implementation impact:

- PlayTable must become a field mat, not a dashboard.
- Top toolbar should be small or collapsible.
- The right inspect panel must shrink to an overlay/HUD and must not consume
  board width.
- Action buttons should be contextual or compact; they should never dominate
  the table.

### Dear Days 2 Deck Builder

Use as the strongest current Deck Builder density reference.

Patterns to adopt:

- Left: large selected-card preview and deck stats.
- Center: ride deck and main deck sections with visible card thumbnails and
  counts.
- Right: searchable card pool grid.
- Bottom or edge commands: add/remove/detail/save, kept separate from the deck
  surface.

Implementation impact:

- If Deck Builder is reopened after M32, keep the Dear Days 2 / Vanguard EX
  three-column workbench structure as the target.
- Rule counters should be visual and immediately adjacent to the deck list.
- Search/filter should never cover the current deck.

## UX Blueprint

### Home

Required visible information:

- selected deck name and validity
- selected format/ruleset
- pack/version status
- local player name
- quick actions: Deck Builder, PlayTable/Solo Test, Room, Replay, Manual,
  Settings

Avoid:

- developer status walls
- showing transport payloads by default

### Deck Builder

Target structure:

```text
-------------------------------------------------------------+
| Back | DECK BUILD | Format | Deck Name | Save/Load/Tools   |
+---------------+----------------------------+----------------+
| Card Preview  | Ride Deck / Main Deck      | Search/Filter  |
| Detail        | Card thumbnails + counts   | Card Pool Grid |
| Deck stats    | Rule counters              |                |
+---------------+----------------------------+----------------+
| Actions: add/remove/detail/save/import/export              |
+-------------------------------------------------------------+
```

Rules:

- selected-card preview should be large enough to inspect card art/text.
- ride/main/G or other format-specific sections must be separated.
- counters must be visible and rule-aware.
- search/filter should not block the deck list.

### PlayTable

Target structure:

```text
+-------------------------------------------------------------+
| Compact top HUD: phase, turn, local/opponent summary        |
+-------------------------------------------------------------+
| Left phase rail |        Full board / playmat field         |
|                 |  Opp RG row / Opp VG / Trigger zone       |
|                 |  Local VG / Local RG row / Soul           |
|                 |  Deck/Ride/Drop/Damage/Bind/Order piles   |
|                 |                                           |
|                 |  Small inspect HUD overlay when needed    |
+-------------------------------------------------------------+
| Compact command dock: contextual actions only               |
+-------------------------------------------------------------+
| Local hand strip, larger cards, selected-card highlight     |
+-------------------------------------------------------------+
```

Required zones:

- Vanguard
- Rear-guard front/back rows or grouped local rear-guard row
- Deck
- Ride Deck
- Hand
- Damage
- Drop
- Soul
- Bind
- Order
- Trigger Check Zone
- Gift/Crest/marker area

Required visible counts:

- deck count
- ride deck count
- hand count
- damage count
- drop count
- soul count
- bind count
- trigger/check zone state when relevant

Rules:

- The field must occupy most of the screen.
- The first impression must be "card table", not "debug app".
- The right inspect surface is temporary/overlay; it should never squeeze the
  board.
- Advanced/debug/automation/network details are hidden by default.
- Commands call the existing session/RulesCore paths; UI does not mutate
  `GameState` directly.

### Online Room

Target structure:

- Create/join/ready/start/rematch/back home.
- Show connection status, player count, deck hash, pack hash, public cursor.
- Explain trusted-client mode clearly.
- Hide payloads and low-level transport details by default.

### Tutorial / Manual

Target lessons:

- Board zones.
- Ride deck and normal ride.
- Main phase call/move.
- Battle declaration.
- Guard step.
- Drive check.
- Damage check.
- Trigger allocation.
- Gift marker / crest / persona shield concepts where applicable.

## Current Gap From Reference

Current M32 visual evidence after M32-02 shows these gaps:

- The field is improved and zones are separated, but the art direction is still
  utilitarian.
- Command dock is reduced but still more visible than an ideal contextual
  action surface.
- The right Inspect HUD no longer covers the table, but it is still too tall and
  text-heavy.
- Center zone card alignment still needs a later polish pass if it remains hard
  to read during real manual play.
- Top toolbar still feels like an app control strip rather than game HUD.

## M32 Implementation Direction

Immediate next slices:

- `M32-02`: Reposition zones and resize card placeholders so the table reads
  as a Vanguard board at first glance. Done.
- `M32-02b`: Align the PlayTable reset with public letsplay layout evidence.
  Done.
- `M32-03`: Convert the PlayTable from a dashboard into a field/HUD layout.
  Done.
- `M32-04`: Replace bar-like rear-guard presentation with playmat-style
  front/back slots and capture Windows visual evidence. Done.
- `M32-05`: Polish the hand strip and compact pile interactions.

Concrete M32-02 changes:

- Increase field mat height and reduce non-field chrome.
- Reduce command dock button color intensity.
- Move deck/ride/drop/damage piles closer to Dear Days-style edge clusters.
- Add trigger check zone and order zone panels.
- Make hand strip taller and keep cards fully visible.
- Add compact top/bottom player HUD labels for damage/deck/hand/drop counts.

Concrete M32-03 changes:

- Treat M32-03 as a de-dashboard field/HUD pass, not only an inspect text pass.
- Remove or collapse the permanent right Inspect column.
- Keep selected-card summary, target summary, and next action as small overlays
  or edge HUDs.
- Move log, setup, zone status, bot, online, trigger, and ability diagnostics
  into Advanced or collapsible drawers.
- Keep phase rail, bottom hand strip, visible zone piles, trigger zone, and
  contextual action bar as the primary default surfaces.
- Add a small `Inspect` toggle later if card-detail reading needs a larger
  temporary overlay.

Concrete M32-04 changes:

- Use visible slot skeletons for opponent/local front and back rear-guards.
- Keep local/opponent vanguard slots centered and separate from rear-guards.
- Treat Deck, Drop, Ride Deck, Bind, Trigger Zone, Gift Marker, Damage, Order,
  and Soul as compact field markers.
- Do not import or recreate comparator playmat assets.

Concrete M32-05 changes:

- Make the bottom hand strip read like cards in hand at 1280x720.
- Keep the field clean by opening compact piles through an expanded overlay or
  modal instead of printing long card lists inside the field marker.
- Preserve manual simulator freedom and existing command/session paths.

## Acceptance Criteria

For PlayTable work to pass:

- 1280x720 evidence shows the field as the dominant visual object.
- Board zones are recognizable without reading debug text.
- Commands fit in two compact rows and do not dominate the table.
- Hand strip is visible and not clipped.
- Inspect HUD is readable but not the main surface.
- Debug and network payloads are hidden by default.
- Unity compile and EditMode tests pass for C# changes.
- Windows player smoke and visual evidence pass for runtime flow changes.

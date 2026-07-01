# UI Experience Redesign Spec

## Purpose

The current client is technically functional, but the first-run experience feels
like an internal debug tool instead of a Vanguard simulator. This spec resets
the UI direction without copying code, assets, or exact layouts from VangPro,
Vanguard Area, or official games.

Current active scope is Windows-first program completion. Android/mobile,
APK/app packaging, and release-candidate work are deferred until the user
explicitly reopens that track.

## Evidence Reviewed

- Current Windows player screenshot from the user:
  `C:/Users/Phet/AppData/Local/Temp/codex-clipboard-fd5be2b7-9fee-48a9-ab1f-eaa613327146.png`
- VangPro local study screenshots:
  - `work/vangpro_after_load.png`
  - `work/vangpro_deckedit.png`
  - `work/vangpro_load_deck.png`
  - `work/vangpro_deck_type.png`
- Vanguard Area local structure and visual references:
  - `D:/Cardfight!! Area Full Version 4.16/Decks/`
  - `D:/Cardfight!! Area Full Version 4.16/replays/`
  - `D:/Cardfight!! Area Full Version 4.16/Manual/`
  - `D:/Cardfight!! Area Full Version 4.16/Sprite/Field1.png`
- Comparator study 2026-06-27:
  `outputs/comparator_study/COMPARATOR_SYSTEM_UX_STUDY_TH.md`

## UX Problem Statement

The app currently exposes implementation systems before player goals. A player
sees search/loading controls, cache controls, debug-like PlayTable actions, and
internal automation terms before they see a clear flow such as select deck,
edit deck, start solo table, fight CPU, or create room.

This is a structural UX issue, not only a visual polish issue.

## Comparator Patterns To Learn From

Use these as product patterns only.

- VangPro lobby pattern:
  - clear online/server status
  - player identity
  - selected format
  - selected deck readiness
  - large mode entries such as create room, CPU, solo, deck edit
- VangPro deck editor pattern:
  - top nation/clan/filter tabs
  - left selected-card preview and text
  - center card pool grid
  - right deck list
  - always-visible deck counters and legality status
  - load/save/code/import actions in dedicated dialogs
- VangPro custom-card/accessory pattern:
  - custom import is spreadsheet plus images zip plus manifest/hash checks
  - deck type/accessory metadata should cover sleeve, playmat, crest/persona
    shield, and marker-like options outside deck legality validation
  - loading tips, replay, and notes are normal player features, not debug tools
- Vanguard Area manual simulator pattern:
  - table/playmat is the main mental model
  - deck files and replays are first-class user artifacts
  - zones are visible on the field instead of hidden behind command buttons
  - manual freedom is preserved, but the board remains the center of the UI
- CGS / Card Game Simulator custom-pack pattern:
  - `cgs.json` style manifest defines game metadata, card id field, image field,
    primary text field, properties, default action, starting hand, and update URL
  - set/card/deck data are portable and inspectable
  - deck text files are human-readable count-line lists
- Cardfight Connect technical pattern:
  - Unity Netcode/Unity Services is a viable online stack reference
  - do not switch away from Photon without an ADR and a concrete reason
- Dear Days high-level pattern:
  - tutorial/story/CPU clarity remains useful as inspiration
  - local packed content is not a safe source for data extraction

## Current UI Risks

1. The app starts in a Card Browser/Deck Builder hybrid, not a player lobby.
2. Loading states occupy filters and status areas without a clear next action.
3. Card Browser, Deck Builder, Start Game, and Online Room are mixed into one
   screen, so the user cannot tell which mode they are in.
4. PlayTable toolbar exposes internal implementation controls such as trigger
   logs, pending AUTO queues, draft decisions, and apply previews as primary
   player actions.
5. The play table does not yet visually lead with Vanguard zones, so users must
   reason from buttons instead of the board.
6. Dialogs and tool panels are not separated by user task. Deck load/share,
   card filtering, pack validation, trigger automation, and online diagnostics
   need different surfaces.
7. Desktop and Android responsive checks exist, but the information architecture
   still reads as a test harness on both platforms.

## Target Screen Architecture

## Temporary Visual Baseline

Use a VangPro-style UX baseline for the first M19 pass because it already
matches the product direction better than the current debug-oriented UI.

This means:

- lobby-first navigation
- large mode buttons
- visible deck readiness
- format selector near play actions
- deck editor with left preview, center card grid, right deck list
- focused dialogs for load/save/code/type choices
- dark card-game tabletop framing

This does not mean:

- copying VangPro assets, logos, button frames, backgrounds, icons, code, or
  exact layout measurements
- extracting anything from the VangPro app package
- using VangPro naming for internal code unless it is a generic UX term

The implementation should feel familiar to a VangPro user while remaining a
distinct Vanguard Thai Sim UI.

## Temporary Taxonomy Baseline

Product decision 2026-06-27: use Vanguard Area-style clan/nation grouping for
the Deck Builder and Card Browser pass for now. This is a taxonomy/UX baseline
only, and it remains active until a later explicit UI milestone replaces it.

This means:

- card filtering is organized around familiar clan/nation buckets first
- D-series nations and older clan names are both visible where data supports it
- mixed/special categories such as `No Nation`, `Touken Ranbu`, `Bang Dream!`,
  or other collaboration-style groups can appear as their own buckets
- deck builder tabs/filters should prioritize these buckets before advanced
  series/set filtering

This does not mean:

- copying Vanguard Area text files, sprites, playmat art, or updater assets
- replacing the verified KK Card Fight exported data source
- hard-coding taxonomy from file names when the local card database already has
  structured clan/nation/series fields

Implementation rule: derive visible groups from the local runtime card database
first, then add display aliases/order rules in a documented mapping when needed.

Current implementation:

- `VanguardAreaClanTaxonomy` orders classic clans into Vanguard Area-style
  buckets and adds D-era nation filter options.
- `CardBrowserBootstrap` keeps one dropdown surface, but each option now maps
  to either a `Clan` query or a `Nation` query.
- `N/A` stays available, but it no longer dominates the top of the filter list
  only because it has the largest raw card count.
- The implementation does not copy Vanguard Area files or mutate card pack
  data.

### Home Dashboard

The app starts here. Required content:
- Player status (Guest/Profile)
- Selected deck summary, validity status, selected format, and pack loading details.
- Primary navigation actions:
  - 🎴 Card Workshop
  - ⚔️ Battle Center
  - ⚙️ System & Support
- Design rule: this screen presents a clear, uncluttered dashboard answering "what can I do now?".

### Card Workshop (Consolidated Split-Screen)

The unified interface for card database lookup, deck assembly, and custom pack status.
- Left half: Active deck list, triggers (12/16 count), grade distribution, and tools (Save/Load/Import/Export).
- Right half: Searchable, scrollable Card Database grid.
- Allows split-screen drag-and-drop or click to instantly add/remove cards without losing editor state.
- Tab bar at the top to toggle between:
  - `✍️ Deck Builder` (Split-screen active deck editing)
  - `📚 Card Database` (Read-only fullscreen browser)
  - `📦 Custom Packs` (Validation checks for user custom packs)

### Battle Center (Matchmaking & Practice)

The entry point for matches and playback. Toggles between:
- `🤖 Solo vs CPU`: Choose player deck, select bot deck, adjust difficulty, and start. Includes a Quick Deck Switch/Edit overlay.
- `🌐 Online Room`: Photon matchmaking room with **Navigation Lockout**. Players cannot leave the prep screen via standard menu buttons; they must explicitly concessions or click "Leave Room" to cleanly disconnect first.
- `🎬 Match Replay`: Browser to load and watch offline logs on a full-screen board.

### Play Table (Full-Screen Gameplay)

Dedicated gameplay board interface.
- **Full-Screen Transition:** Starting a match unloads standard UI groups and expands the table to borderless fullscreen. Conceding or finishing the match cleanly returns the player to the Battle Center lobby.
- **Manual Drawer Overlay:** Open rules/manual directly over the board (using ESC or top icon). It slides out as an overlay panel without disconnecting from Photon or pausing the match.
- Board/playmat zone-first layout, local hand strip, opponent summary, zone counts (soul zone modeling added).

### System & Support

Handles general configuration and education. Toggles between:
- `🔧 Options`: Player name, UI scale, image cache options.
- `📖 Rules Manual`: Basic Vanguard rules, phases, triggers, combat calculations.

## Visual Direction

- Keep the card art and table state as the primary visual content.
- Use a dark tabletop/cyber tabletop theme only as a supporting frame.
- Avoid one giant toolbar of text buttons.
- Prefer mode tabs, segmented filters, icon+text primary actions, card grids,
  zone labels, and collapsible panels.
- Use the shared icon policy in `docs/UI_ICON_SYSTEM_SPEC.md`. The current
  primary icon subset is Lucide under
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/`.
- Use `docs/UI_GAME_SYMBOLS_SPEC.md` for trigger, marker, zone, and card-type
  symbols. These must be original/semantic symbols, not extracted assets.
- Do not copy VangPro/Vanguard Area frames, backgrounds, logos, or button art.
- Build a distinct "Vanguard Thai Sim" theme with our own shapes and tokens.

## M19 Player Experience Reset

M19 is the next UI milestone after the release artifact smoke work. It is a
frontend/client milestone and must not change RulesCore behavior.

- `M19-01`: UI audit and screen map closeout
  - finalize this spec
  - add player-first navigation map
  - document which debug surfaces move behind Advanced
- `M19-02`: Home/Lobby shell
  - add first screen with mode buttons and selected deck summary
  - route to existing Card Browser/Deck Builder and PlayTable flows
  - keep current smoke bootstrap path working
  - shipped in the first M19 implementation slice:
    `HomeLobbyBootstrap` is now the runtime entry point, Card Browser/Deck
    Builder opens from Home, Solo Play and Online Room reuse the latest saved
    deck when available, trigger symbols are English text badges, and the
    smoke bootstrap still bypasses normal UI startup.
  - post-M19 polish: Home/Lobby header and panel layout groups now avoid forced
    child expansion so buttons, dividers, title text, and the Vanguard Area clan
    baseline keep stable preferred sizes on Windows and Android reference
    profiles.
- `M19-03`: Deck Builder dedicated layout
  - separate deck-building surface from generic card browsing
  - implement left preview, center grid, right deck list/counters
  - keep existing card repository/deck validation unchanged
  - shipped in the second M19 implementation slice:
    Card Browser now has `DeckBuilder` and read-only `Browser` launch modes.
    Home routes `Deck Builder / Cards` to the deck-building mode and `Card
    Browser` to read-only search. The deck-building mode uses left preview,
    center card grid, and right deck list/counters while keeping repository and
    deck validation logic unchanged.
- `M19-04`: Deck load/save/code dialog
  - move deck code/load/delete/copy/apply actions into a focused dialog
  - keep deck import/export tests and deck code round-trip intact
  - shipped in the third M19 implementation slice:
    Deck Builder now exposes a `Deck Tools` dialog for save, load latest, copy
    code, apply pasted code, clear, and delete-current actions. The visible deck
    panel keeps only deck readiness plus play/online launch actions.
- `M19-05`: PlayTable zone-first layout
  - make the board and zones the dominant screen structure
  - move common actions close to zones
  - preserve existing command facade and legal action mask behavior
  - shipped in the fourth M19 implementation slice:
    common turn/phase/move/gift/undo controls now sit inside the zone-first
    table panel instead of the top toolbar. The toolbar is reduced to table
    title/status, close, and remaining debug/online automation controls that
    M19-06 will move behind Advanced.
- `M19-06`: Advanced PlayTable drawer
  - move trigger draft, pending AUTO, manual resolution, online cursor, and
    replay diagnostics out of the primary toolbar
  - no removal of tooling; only hide it behind deliberate access
  - shipped in the fifth M19 implementation slice:
    online PlayTable automation/debug controls now live in an `Advanced` drawer
    hidden by default. The drawer uses CanvasGroup/ignoreLayout so the primary
    UI stays clean while existing test hooks and runtime control references stay
    intact.
- `M19-07`: Loading, empty, and error states
  - replace raw `Loading...`/blank panels with clear player states
  - keep broken-image fallback and pack validation messaging visible
  - shipped in the sixth M19 implementation slice:
    Card Browser filter/status placeholders now use player-facing preparation
    text, card-pack load failure details are formatted through a shared UI state
    formatter, and empty PlayTable zones show `No cards` instead of raw `Empty`.
- `M19-08`: Windows and Android visual smoke
  - screenshot-check Home, Deck Builder, and PlayTable at desktop and Android
    reference sizes
  - shipped in the seventh M19 implementation slice:
    automated M19 visual smoke now checks Windows and Android reference
    viewports plus Home taxonomy, Deck Builder layout, Card Browser read-only
    mode, PlayTable zone-first layout, Advanced drawer, and loading/empty state
    labels. This is an automated gate; manual artifact smoke remains the next
    post-M19 queue item.
  - run Unity compile/EditMode and player smoke
- `M19-09`: User-provided icon override loader
  - implement semantic symbol lookup for trigger, marker, card-type, and zone
    symbols
  - validate the private icon manifest/template without committing proprietary
    assets
  - keep missing private icons as fallback warnings, not UI blockers
  - shipped after Home/Lobby polish:
    `UiGameSymbolRegistry`, `UserIconPackManifest`, and
    `UserIconPackValidator` are implemented, Home trigger badges resolve by
    semantic key, path traversal is rejected, and default English labels remain
    the safe visible fallback.

## Post-M19 Online Room Polish

The first post-M19 online UI slice keeps the existing Photon and reconnect
contracts but reorganizes `MultiplayerLobbyBootstrap` into player-facing
Connection, Room, and Safety/Reveal panels. This is still a trusted-client
friend-room surface, not ranked/security UI. The lobby formatter must not show
deck codes or revealed deck codes.

## Post-M19 Home/Lobby Layout Polish

The Home/Lobby surface keeps the M19 player-first flow, but fixes the first
Android/Windows visual smoke issue where layout groups expanded button and
divider children into large blocks and wrapped the title. The implementation
must keep `VanguardAreaClanTaxonomy` as the card-filter baseline, show compact
Vanguard Area clan wording in the header, and avoid changing card data, deck
validation, RulesCore, or network contracts.

## Acceptance Criteria

- Player starts from Home/Lobby, not raw card search.
- Deck Builder and Card Browser are visually and conceptually separated.
- PlayTable primary screen is zone-first, not toolbar-first.
- Internal debug/network/automation controls are available but not primary.
- Existing GameState, RulesCore, deck validation, card repository, and smoke
  tests remain compatible.
- No copied commercial code, assets, exact UI frames, or proprietary layouts.

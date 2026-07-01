# UI Icon System Spec

## Purpose

This project needs one consistent icon system for the M19 Player Experience
Reset. Icons should make navigation and actions easier to scan without copying
VangPro, Vanguard Area, or official game assets.

## Primary Icon Source

Use Lucide as the default UI icon set.

- Source: https://lucide.dev
- Local source package: `work/icon_sources/lucide-static-1.21.0.tgz`
- Local Unity subset:
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/`
- License file:
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/LICENSE-lucide-static.txt`
- Package metadata:
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/package-lucide-static.json`

Lucide is suitable for:

- lobby mode buttons
- deck builder actions
- play table common actions
- online/replay/settings navigation
- advanced/debug drawer labels

## Secondary Icon Sources

These are allowed only when Lucide cannot express the action clearly.

### Kenney Game Icons

- Source: https://kenney.nl/assets/game-icons
- License: CC0 according to the asset page.
- Best for gamepad/input prompts and friendly game utility icons.
- Do not use Kenney logos.

### Game-icons.net

- Source: https://game-icons.net
- License: CC-BY for most icons according to the FAQ.
- Best for fantasy/card-game-flavored symbols.
- Must add visible credits in an in-app credits/settings screen before shipping.
- Do not use as the default UI set because attribution management is heavier.

## Icon Import Policy

- Do not import full icon packs into Unity.
- Import only icons used by a screen or near-term milestone.
- Keep original license/package metadata next to imported assets.
- Prefer monochrome line icons for UI controls.
- If Unity SVG import becomes unreliable, rasterize the selected SVGs into
  `64x64` and `128x128` PNG sprites as a build/tool step.
- Do not use proprietary icons, copied screenshots, game logos, or assets from
  VangPro, Vanguard Area, Dear Days, or Dear Days 2.

## Selected Lucide Subset

The following SVG files are already copied into
`client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/`.

| Icon | Use |
| --- | --- |
| `home.svg` | Home/Lobby |
| `user.svg` | Player/guest profile |
| `users.svg` | Online players/rooms |
| `wifi.svg` | Connected/online status |
| `server.svg` | Server/transport status |
| `settings.svg` | Settings |
| `search.svg` | Search |
| `filter.svg` | Filter action |
| `sliders-horizontal.svg` | Advanced filters |
| `x.svg` | Close/clear |
| `refresh-cw.svg` | Refresh/reload |
| `chevron-left.svg` | Previous page/back |
| `chevron-right.svg` | Next page/forward |
| `card-sim.svg` | Card browser/card pool |
| `wallet-cards.svg` | Deck summary/deck code |
| `layers.svg` | Packs/layers/zones |
| `library.svg` | Card library |
| `book-open.svg` | Card detail/rules text |
| `book-marked.svg` | Saved decks/rules reference |
| `save.svg` | Save deck |
| `folder-open.svg` | Load/open deck |
| `copy.svg` | Copy deck code |
| `image.svg` | Export deck image |
| `play.svg` | Start solo/manual play |
| `swords.svg` | Fight/battle |
| `bot.svg` | CPU fight |
| `bot-message-square.svg` | Bot debug/trace |
| `list.svg` | Deck list/log list |
| `scroll-text.svg` | Replay/event log |
| `undo-2.svg` | Undo |
| `redo-2.svg` | Redo/replay step forward |
| `shield.svg` | Guard/defense |
| `shield-check.svg` | Valid/verified deck |
| `shield-alert.svg` | Invalid/risk/warning |
| `zap.svg` | Trigger/effect |
| `sparkles.svg` | AUTO/ability |
| `table.svg` | Play table |
| `panel-right-open.svg` | Open side panel/drawer |
| `panel-right-close.svg` | Close side panel/drawer |
| `bug.svg` | Debug tools |
| `terminal.svg` | Advanced technical logs |
| `eye.svg` | Spectator/visible info |
| `eye-off.svg` | Hidden/private info |
| `download.svg` | Import/download |
| `upload.svg` | Export/upload |
| `trash-2.svg` | Delete |
| `plus.svg` | Add card |
| `minus.svg` | Remove card |
| `check.svg` | Confirm/apply |
| `circle-alert.svg` | Error/blocked state |
| `info.svg` | Info/help |

## M19 Usage Map

### M19-02 Home/Lobby

- Home: `home`
- Deck Builder: `wallet-cards`
- Solo Manual Table: `table` or `play`
- CPU Fight: `bot`
- Online Room: `users` plus `wifi`
- Replay: `scroll-text`
- Settings: `settings`

### M19-03 Deck Builder

- Search: `search`
- Filters: `filter`, `sliders-horizontal`
- Card Pool: `card-sim`, `library`
- Deck List: `wallet-cards`, `list`
- Save/Load/Code: `save`, `folder-open`, `copy`
- Export Image: `image`
- Valid/Invalid: `shield-check`, `shield-alert`

### M19-05 PlayTable

- Table: `table`
- Start/phase advance: `play`, `chevron-right`
- Battle: `swords`
- Guard: `shield`
- Trigger/effects: `zap`, `sparkles`
- Undo/redo: `undo-2`, `redo-2`
- Logs/replay: `scroll-text`

### M19-06 Advanced Drawer

- Drawer open/close: `panel-right-open`, `panel-right-close`
- Debug: `bug`
- Technical logs: `terminal`
- Hidden-state view: `eye`, `eye-off`
- Online state: `server`, `wifi`

## Vanguard-Specific Game Symbols

Do not pull trigger or marker icons from VangPro, Vanguard Area, Dear Days,
Dear Days 2, or official websites. Use `docs/UI_GAME_SYMBOLS_SPEC.md` for safe
semantic aliases such as `trigger_critical`, `trigger_draw`,
`marker_quick_shield`, and `marker_persona`.

Trigger symbols should default to English text badges (`CRITICAL`, `DRAW`,
`FRONT`, `HEAL`, `OVER`, `STAND`). Lucide icons are optional supporting
decoration, not required for trigger recognition.

## Private Override Pack

If the user supplies private icons independently, the UI can support them
through `docs/UI_ICON_OVERRIDE_SPEC.md` and
`client/unity/VanguardThaiSim/Assets/UI/Icons/UserProvided/icon-pack-manifest.json`.
Codex must not extract, scrape, or commit proprietary icons.

## Assessed External Repositories

### `dragogodev/cgs` Cardfight Vanguard

Repository path assessed:

```text
https://github.com/dragogodev/cgs/tree/master/Cardfight%20Vanguard
```

Result:

- Not an icon source.
- No repository-level license file was found during review.
- The Cardfight Vanguard folder contains simulator config/data files such as
  `cgs.json`, `AllSets.json`, `sets/`, and a scraper script.
- `cgs.json` points to external official/Wikia image URLs and declares
  `copyright: Bushiroad`.
- Do not import assets from this repo into the project icon set.
- It may be used only as a reference for how another simulator structures game
  metadata, not as a licensed source of icons or official UI art.

## Acceptance Criteria

- Every icon used by M19 has a documented source and local license.
- Icons are referenced by semantic names in UI code, not by copied visual names
  from comparator apps.
- The player-facing UI uses icons to reduce toolbar text noise.
- Debug icons stay inside the Advanced drawer, not on the primary toolbar.

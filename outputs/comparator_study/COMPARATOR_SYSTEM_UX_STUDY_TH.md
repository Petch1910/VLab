# Comparator System / UX Study

วันที่ศึกษา: 2026-06-27

## ขอบเขต

เอกสารนี้สรุปการศึกษาแหล่งอ้างอิงที่ผู้ใช้ระบุ เพื่อรีดเอาแนวคิดด้านระบบ,
ข้อมูล, UX/UI, custom pack, manual simulator, online room และ workflow มาใช้กับ
Vanguard Thai Sim โดยไม่คัดลอก code, asset, icon, playmat, binary content หรือ
protected game data เข้ามาในโปรเจกต์

แหล่งที่ตรวจ:

- Existing study: `outputs/vangpro_study/VANGPRO_STUDY_TH.md`
- Public repo: `https://github.com/dragogodev/cgs`
- Public launch URL: `https://cgs.gg/main?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdragogodev%2Fcgs%2Fmaster%2FCardfight%2520Vanguard%2Fcgs.json`
- Local Vanguard Area: `D:/Cardfight!! Area Full Version 4.16`
- Local Dear Days: `C:/Program Files (x86)/Steam/steamapps/common/VGDD`
- Local Card Game Simulator: `C:/Program Files (x86)/Steam/steamapps/common/Card Game Simulator`
- Local Cardfight Connect: `C:/Program Files (x86)/Cardfight Connect`

## Safety Boundary

ใช้ได้:

- file inventory
- JSON/text/config/manifest ที่อ่านตรงๆ ได้
- metadata, file count, dimensions, hash, package headers
- UX/system pattern ระดับ high-level

ห้ามใช้:

- คัดลอก proprietary icons, card art, playmat, sprites, UI frames
- decompile/patch/run commercial executables เพื่อเอา logic
- แตก content pack ใหญ่ของ Dear Days
- นำ card text จาก comparator มาแทน source หลักของเรา
- import repo `dragogodev/cgs` เป็นฐานหลักโดยไม่ตรวจ license/permission

## Executive Findings

1. **VangPro ให้ reference ที่ดีที่สุดสำหรับ player-facing product flow**
   VangPro มี lobby, deck readiness, format selection, deck editor, deck tools,
   deck code, custom card import, accessories, tutorial/loading tips และ
   replay/notes ที่ตรงกับเป้าหมายของโปรแกรมเรามากที่สุด. ใช้เป็น UX/product
   pattern ได้ แต่ห้ามคัดลอก asset/code/exact layout.

2. **Vanguard Area ให้ reference ที่ดีที่สุดสำหรับ manual simulator**
   โครงสร้างผู้ใช้ชัดมาก: `Decks/`, `replays/`, `Manual/`, `Text/`, `Sprite/`,
   `Setting.ini`. สิ่งที่ควรนำมาใช้คือ mental model ของ player artifacts,
   board-first play table, deck profile, replay, settings และ clan/nation text
   grouping.

3. **CGS ให้ reference ที่ดีที่สุดสำหรับ custom pack schema**
   ทั้ง public repo และ local Card Game Simulator ใช้แนว `cgs.json` เป็น
   game manifest, `AllSets.json`, per-set card JSON, deck text files, และ
   generic card properties. เหมาะกับ M15 custom packs ของเรา.

4. **Cardfight Connect ให้ network stack clue**
   build ใช้ Unity Netcode, Unity Services Multiplayer/Auth/QoS/Lobbies,
   websocket-sharp, NativeGallery, Newtonsoft.Json. ใช้เป็น reference ด้าน
   stack ได้ แต่ไม่พบ local card database/config ที่อ่านตรงๆ ได้.

5. **Dear Days local folder ไม่เหมาะกับการแกะข้อมูล**
   content อยู่ใน `data.lpkg` และ split pack ขนาดใหญ่ `data.lpkg.0` ถึง
   `data.lpkg.18`. header แสดง `LCD1` และข้อมูล compressed/packed. ใช้ได้แค่
   แนวคิด high-level จากการเล่น/สังเกตภายนอกหรือข้อมูล public เท่านั้น.

6. **งานของเราควรโฟกัสตัวโปรแกรมก่อน packaging**
   comparator ทั้งหมดชี้ว่าจุดสำคัญคือ playable flow: Home -> Deck Builder ->
   Play Table -> Replay/Room. การ build app เป็นแค่ smoke test ไม่ใช่เป้าหมาย
   หลักตอนนี้.

## VangPro Findings

Source: `outputs/vangpro_study/VANGPRO_STUDY_TH.md`

Observed technical hints from the previous VangPro study:

- Package: `com.DOT.VangPro`
- Version observed: `13.8.8`
- Engine: Unity `2022.3.62f2`, IL2CPP, arm64-v8a
- Main activity: Unity player activity
- Backend hints from strings: Firebase Auth, Firebase Realtime Database
- Multiplayer hints from strings: Photon/PUN
- Asset system hint: Unity Addressables
- Remote card image path hint: Cloudflare R2 `card-images/v1`
- Custom card template supports EN/JA/KO/TH/ZH

Screenshots already captured in the previous study:

- `work/vangpro_screen2.png` login/main
- `work/vangpro_after_load.png` lobby after guest login
- `work/vangpro_deckedit.png` deck editor
- `work/vangpro_load_deck.png` load deck/share code dialog
- `work/vangpro_deck_type.png` deck accessory/type dialog

### VangPro Product Patterns

Use these as product/UX patterns only.

Lobby:

- server/version status
- connected status
- guest/user identity
- online count
- room count
- selected format such as `D STANDARD`
- selected deck/readiness status
- clear entry points for create room, CPU, solo, deck edit, replay, notes

Deck editor:

- landscape-first layout
- top nation/clan tabs
- selected-card preview/detail on the left
- card pool grid in the center
- deck list on the right
- visible `Ride` / `Main` counters
- rule badge such as fighter rules
- dedicated actions for capture deck, reset deck, save deck, load deck, deck type

Deck sharing:

- saved-deck selector
- load/delete
- copy code
- apply code
- mismatch handling should warn on missing cards or data-version mismatch

Custom card import:

- spreadsheet-like data file (`.csv` / `.xlsx`)
- images zip
- useful columns:
  - `CustomId`
  - `Name`
  - `Nation`
  - `Clan`
  - `Tribe`
  - `Grade`
  - `Power`
  - `Shield`
  - `Rare`
  - `Trigger`
  - `OrderType`
  - `Tags`
  - `OrderEvents`
  - `Effect`
  - `ImageFile`
  - `Notes`
- manifest with custom id, definition hash, image hash, data version, language
- multiplayer should compare custom pack/hash before play

Effect handling:

- effects can start as display text/manual handling
- full automatic execution should be built gradually through supported
  templates and pending-resolution queues
- bot should only use actions supported by the core

Accessories/deck type:

- playmat
- sleeve
- crest
- persona shield
- token/marker-like metadata
- these should live in deck metadata, not deck legality validation

Loading/tutorial/replay:

- loading screens can teach ride deck, persona ride, trigger timing, and guard
  basics
- replay and notes should be first-class lobby actions
- replay/action logs can later feed bot debugging and combo discovery

### VangPro Impact On Our Backlog

Recommended work from VangPro study:

- Home/Lobby should show deck readiness, format, online status, and room count
  before debug systems.
- Deck Builder should keep the VangPro-like 3-column task flow already started:
  preview, grid, deck list.
- Deck Tools should stay a focused dialog for save/load/code/apply/delete.
- Add a real Settings/Deck Type screen:
  sleeve, playmat, markers/crest/persona shield, default deck, player name.
- Add custom card import UX for `.csv/.xlsx + images.zip`, with manifest/hash
  verification.
- Add loading tips and in-app Manual.
- Do not attempt arbitrary auto-effect execution before structured templates
  and rule fixtures are stable.

Do not copy:

- VangPro art, icons, frames, button graphics, logos, package content, or code.
- Exact UI layout measurements.
- Backend endpoints or private asset URLs as runtime dependencies.

## Vanguard Area Findings

Path: `D:/Cardfight!! Area Full Version 4.16`

Top-level structure:

- `CardSprite/`
- `CardSpriteMini2/`
- `Decks/`
- `Manual/`
- `replays/`
- `Sprite/`
- `Text/`
- `Utils/`
- `Setting.ini`
- `Autoupdater.exe`
- `Vanguard.exe`

Safe inventory:

- `Text/` has 54 text files.
- `Text/` has 16,474 `CardStat` entries, 16,474 `CardName` entries, and
  16,474 `CardText` entries.
- `CardSprite/`: 16,473 jpg files, about 1,220.47 MB.
- `CardSpriteMini2/`: 16,474 jpg files, about 153.45 MB.
- `Sprite/Field1.png`: 607 x 719.
- `Sprite/FaceDownCard.jpg`: 300 x 428.
- `Manual/index.html` title: `Cardfight Area | Help`.
- Manual headings found: Basics, Playing Field, Deck Editor, Server.
- `Text/Version.ini`: text/client/sleeve version counters.
- `Setting.ini` stores default deck, server IP, nickname, sleeve, image update
  mode, resolution, and deck editor image size.

Observed data model:

- Card data is script/text oriented, not relational DB.
- Text files are grouped by clan/nation/title-like buckets.
- Each card text block is keyed by an internal numeric `CardStat`.
- Separate files such as `UnitPower.txt` store numeric stat arrays.
- Deck profiles use `.prfl` binary/fixed-width-like data; treat as format
  reference only unless a later explicit importer milestone defines safe
  parsing.

Product lessons:

- Preserve first-class `Decks` and `replays` concepts.
- Keep board/playmat as the main mental model.
- Settings should expose practical player preferences:
  default deck, sleeve/back, resolution/window mode, image cache/update mode,
  deck editor image size.
- Clan/nation grouping should stay Vanguard Area-style for now.
- Add an in-app Manual/How-To surface. It should cover Basics, Playing Field,
  Deck Builder, Online Room, Replay, and Custom Pack.

Do not copy:

- `Sprite/Field1.png`
- `BackgroundMenu.png`
- `FaceDownCard.jpg`
- card sprites
- `Text/*.txt` data as source data
- updater data or sleeve lists

## CGS Public Repo Findings

Repo: `https://github.com/dragogodev/cgs`

GitHub API summary:

- Repo: `dragogodev/cgs`
- Description: `Card Game Simulator data`
- Default branch: `master`
- License metadata: `null`

Important implication:

- Treat the repo as public reference/schema only.
- Do not import full card data into our production pack without license review.

Cardfight Vanguard `cgs.json` shape:

- `name`: Cardfight Vanguard
- `allSetsUrl`: raw GitHub `AllSets.json`
- `autoUpdateUrl`: raw GitHub `cgs.json`
- `cardIdIdentifier`: `number`
- `cardPrimaryProperty`: `effect`
- `cardImageProperty`: `image_url`
- `cardImageFileTypeIdentifier`: `image_file_type`
- `gameDefaultCardAction`: `tap`
- `gameStartHandCount`: 5
- `deckSharePreference`: `individual`
- `copyright`: Bushiroad
- `rulesUrl`: official Vanguard how-to page
- card properties: type, clan, grade, power, shield, effect, url

`AllSets.json`:

- 240 sets observed.
- Each set has `code`, `name`, and `cardsUrl`.
- Recent sampled sets include DZ-era products such as Festival Booster 2025 and
  DZ-BT09.

Per-set card JSON sampled fields:

- `name`
- `number`
- `url`
- `image_url`
- `image_file_type`
- `effect`
- `productName`
- `type`
- `clan`
- `grade`
- `power`
- `shield`

Product lessons:

- M15 custom packs should support a CGS-like manifest:
  `pack_id`, `display_name`, `all_sets_url`, `auto_update_url`,
  `card_id_field`, `image_field`, `primary_text_field`, `card_properties`,
  `default_card_action`, `starting_hand`, and `deck_share_preference`.
- Pack validation UI should show schema version, set count, card count,
  missing images, unsupported fields, and source/copyright note.
- Card Browser can expose dynamic custom fields from a pack, not only hard-coded
  Vanguard fields.
- Deck import/export can support a human-readable count-line format.

## Local Card Game Simulator Findings

Path: `C:/Program Files (x86)/Steam/steamapps/common/Card Game Simulator`

Relevant readable structure:

- Unity player build.
- `StandaloneWindows64_Data/StreamingAssets/`
- bundled games under folders like:
  - `Dominoes@www.cardgamesimulator.com`
  - `Mahjong@www.cardgamesimulator.com`
  - `Standard Playing Cards@www.cardgamesimulator.com`
- each bundled game has `cgs.json`, `AllCards.json`, `AllSets.json`, `decks/`,
  and set folders/files.

Bundled game manifest patterns:

- Dominoes: properties `type, suit, rank`, start hand 7, default action rotate.
- Mahjong: properties `type, suit, rank`, start hand 13.
- Standard Playing Cards: properties `rank, suit, color`, start hand 2.

Deck text example pattern:

- header line identifies game/deck.
- each following line uses `count card name`.

Product lessons:

- Custom pack folders should be portable and inspectable.
- Keep pack data under one folder with manifest + cards + sets + decks.
- Provide a "Load From URL" path later, but keep local/offline import first.
- Support generic card games as a future platform direction only after Vanguard
  core is stable.

## Cardfight Connect Findings

Path: `C:/Program Files (x86)/Cardfight Connect`

Readable structure:

- Unity player build.
- `Cardfight Connect_Data/StreamingAssets/` only exposes
  `UnityServicesProjectConfiguration.json`.
- `ScriptingAssemblies.json` and `Managed/` reveal package-level stack.

Observed stack clues:

- `Assembly-CSharp.dll`
- `Unity.Netcode.Runtime.dll`
- `Unity.Networking.Transport.dll`
- `Unity.Services.Multiplayer.dll`
- `Unity.Services.Authentication.dll`
- `Unity.Services.Authentication.PlayerAccounts.dll`
- `Unity.Services.QoS.dll`
- `Unity.Services.Wire.Internal.dll`
- `unity-websocket-sharp.dll`
- `websocket-sharp.dll`
- `NativeGallery.Runtime.dll`
- `Newtonsoft.Json.dll`
- `Unity.InputSystem`
- `Unity.TextMeshPro`
- URP / 2D renderer modules

Product lessons:

- Unity Netcode + UGS is a real viable path for this genre, but switching now
  would be a structural change and needs ADR.
- Our current Photon trusted-client room can remain. The key is to improve
  gameplay sync, reconnect, public event masking, and UX before changing
  network stack.
- Native gallery/image import hints are relevant for future custom sleeves,
  playmats, or user-provided icon packs.
- No local readable card database was found; do not infer rule implementation
  from package list alone.

## Dear Days Local Findings

Path: `C:/Program Files (x86)/Steam/steamapps/common/VGDD`

Readable safe metadata:

- `VG2.exe`: 10,096,128 bytes.
- SHA256 observed:
  `05ACE07319F7113E8698995977A9F6FC2E7E5057ED0094441F1CD60391DA6272`
- `app/data.lpkg`: 371,622 bytes.
- `app/data.lpkg` header starts with `LCD1`.
- split content files `data.lpkg.0` through `data.lpkg.18`.
- most split files are around 1 GiB each; final observed split is about
  283 MB.

Product lessons:

- Use Dear Days only as UX/game-mode inspiration unless the user provides
  explicit screenshots or public documentation.
- Good concepts to keep from earlier high-level study:
  tutorial/story onboarding, CPU difficulty, deck recipe flow, trigger/skill
  resolution clarity, replay/result presentation.

Do not do:

- do not unpack `data.lpkg.*`
- do not run/debug/attach to `VG2.exe`
- do not copy card art/UI/audio/effects

## Recommended Design Decisions For Our Project

### 1. Custom Pack V3 Should Borrow CGS Shape

Add a future spec for `custom-pack-v3`:

- manifest:
  - `schema`
  - `pack_id`
  - `display_name`
  - `source_kind`
  - `all_sets_url`
  - `auto_update_url`
  - `card_id_field`
  - `image_field`
  - `primary_text_field`
  - `card_properties`
  - `default_card_action`
  - `starting_hand`
  - `deck_share_preference`
  - `copyright_note`
- folders:
  - `cards/`
  - `sets/`
  - `images/`
  - `decks/`
  - `abilities/`

This should not replace `vanguard_th`; it should be an adapter/import path.

### 2. Deck Import/Export Should Add Human-Readable Mode

Keep current compact deck code, but add a plain text option:

```text
### Vanguard Thai Sim Deck: Deck Name
4 CARD-ID-001
3 CARD-ID-002
1 RIDE:CARD-ID-003
```

Benefits:

- easy for users to inspect
- easy for AI/dev tools to debug
- similar UX benefit to CGS decks without copying their exact implementation

### 3. Play Table Should Continue Zone-First Work

From Vanguard Area:

- deck/drop/damage/soul/bind/order/replay should be visible artifacts.
- event log and replay should feel like player tools, not developer output.
- manual freedom matters; automation must not block a player from manually
  correcting state.

Recommended next UX work:

- bigger board area
- clearer zone labels and pile counts
- hand strip with selected-card preview
- Advanced drawer remains hidden for automation/debug
- in-app manual button from Home and PlayTable

### 4. Settings Should Become A Real Screen

Use Vanguard Area setting categories as a practical baseline:

- default deck
- nickname/player profile
- sleeve/card back
- image cache/update mode
- resolution/window scale
- deck editor image size
- online transport mode/status

### 5. Online Direction Should Stay Photon For Now

Cardfight Connect suggests Unity Services Multiplayer is viable, but changing
transport now would distract from unfinished gameplay.

Keep:

- Photon trusted-client casual rooms
- explicit unsafe/ranked warning
- public event masking
- reconnect cursor/batch replay

Defer:

- UGS/Unity Netcode migration
- custom authoritative server
- ranked/security model

### 6. Data Source Direction

Current source of truth remains:

- KK Card Fight Thai export JSON/images
- runtime SQLite/JSON pack generated by our tooling

Comparator data is reference only.

## Immediate Backlog Impact

Recommended next slices:

1. `M20-01 Comparator Findings Spec Update`
   - link this study into AI/context docs
   - make safety boundary explicit

2. `M20-02 PlayTable Player UX Pass`
   - larger board, zone counts, hand strip, selected-card preview
   - no new app packaging

3. `M20-03 Settings Screen`
   - default deck, player name, sleeve/text badge mode, image cache mode

4. `M20-04 In-App Manual`
   - Basics, Playing Field, Deck Builder, Online Room, Replay, Custom Packs

5. `M15-next Custom Pack CGS Adapter Spec`
   - design adapter/importer for CGS-like manifest without importing full
     unlicensed data by default

6. `M13-next Online Room Gameplay Usability`
   - keep Photon, improve player-facing sync/reconnect/status

## Final Rule

Use comparator products to learn structure and workflows. Do not make Vanguard
Thai Sim depend on copied assets, copied code, or protected extracted content.

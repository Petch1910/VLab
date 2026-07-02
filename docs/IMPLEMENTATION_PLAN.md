# Implementation Plan

## Required Reading

1. `AGENTS.md`
2. `docs/AI_QUICK_START.md`
3. Spec ของระบบที่กำลังแตะ
4. `docs/CORE_DEVELOPMENT_GUARDRAILS.md` เมื่อแตะ core/bot/simulation
5. `docs/VANGUARD_CORE_RULE_ARCHITECTURE_REFERENCE.md` เมื่อแตะ rules/formats/timing

เมื่อจบงานต้องอัปเดตสถานะในไฟล์นี้ หรือเขียน handoff ด้วย
`docs/AI_TASK_HANDOFF_TEMPLATE.md`

## Current Direction

```text
Windows-first program completion
-> finish Home -> Deck Builder -> PlayTable -> Replay / Online Room on Windows
-> defer Android, mobile QA, APK, app packaging, release-candidate packaging,
   and public distribution until the user explicitly re-enables that track
-> M32 Vanguard Area-style PlayTable reset is paused by user instruction
-> active pivot: rule-corpus-driven deck legality and combo compatibility
-> active data source: data/packs/vanguard_th/cards.sqlite
-> active product scope: TD01-TD06, BT01-BT09, EB01-EB05 grouped by clan
-> latest PlayTable target closed: M32-04 playmat slot layout / visual evidence closeout
-> latest planning target closed: M34-03 deck-feasible archetype priority v2 (Phase A1)
-> latest target closed: M35-A2 first target slice selection + format policy +
   taxonomy gap report
-> selected first slice: Classic Core / โนว่า เกรปเปอร์
-> latest target closed: M35-A3 minimal deck legality fixtures for selected slice
-> latest target closed: M35-A4 first-slice feasibility report refresh
-> latest target closed: M35-B1 semantic vocabulary for selected slice
-> latest target closed: M35-B2 offline semantic extractor for selected slice
-> latest target closed: M35-B3 requirement/provider model for selected slice
-> latest target closed: M35-B4 manual review queue for unknown/low-confidence cards
-> latest target closed: M35-C1 pair compatibility graph for selected slice
-> latest target closed: M35-C2 resource conflict detector for selected slice
-> latest target closed: M35-C3 timing compatibility detector for selected slice
-> latest target closed: M35-C4 zone/target compatibility detector for selected slice
-> latest target closed: M35-C5 selected-slice compatibility output
-> latest target closed: M35-D1 candidate package selection
-> latest target closed: M35-D2 deck skeleton ratio planner
-> latest target closed: M35-D3 combo line explainer
-> latest target closed: M35-D4 reviewed playbook seed export
-> latest target closed: M35-E1 second slice selection
-> latest target closed: M35-E2 second-slice fixture/format readiness check
-> latest target closed: M35-E3 generalize semantic/compatibility tools
-> latest target closed: M35-E4 bot integration gate
-> latest target closed: M35-closeout Hybrid Vertical-Slice closeout /
   next queue selection
-> latest target closed: M36-01 first-slice review packet
-> latest target closed: M36-02 deck recipe draft model
-> latest target closed: M36-03 deck recipe validator
-> latest target closed: M36-04 combo-line to recipe consistency check
-> latest target closed: M36-05 second-slice readiness comparison
-> latest target closed: M36-closeout deck recipe validation closeout
-> latest target closed: M37-01 accepted seed slot-gap completion candidates
-> latest target closed: M37-02 trigger package repair proposal
-> latest target closed: M37-03 rejected-line support-gap triage
-> latest target closed: M37-04 manual semantic mapping candidates
-> latest target closed: M37-05 revised recipe validation rerun
-> latest target closed: M37-closeout first runtime-ready recipe decision
-> latest target closed: M38-01 accepted seed human review packet
-> latest target closed: M38-02 grade profile repair candidates
-> latest target closed: M52-06 fifth-slice blocker repair candidates
-> latest target closed: M52-closeout fifth-slice runtime readiness decision
-> latest target closed: M53-01 fifth-slice human repair review packet
-> latest target closed: M53-02 fifth-slice human-selected recipe artifact
-> latest target closed: M53-03 fifth-slice human-accepted repair artifact
-> latest target closed: M53-04 fifth-slice repaired recipe validation rerun
-> latest target closed: M53-05 fifth-slice runtime fixture promotion gate
-> latest target closed: M53-closeout fifth-slice fixture closeout
-> latest target closed: M54-01 fifth fixture schema validator
-> latest target closed: M55-04 sixth-slice recipe pipeline entry gate
-> latest target closed: M56-01 sixth-slice fixture scaffold
-> latest target closed: M56-02 sixth-slice review packet
-> latest target closed: M56-03 sixth-slice recipe draft model
-> latest target closed: M56-04 sixth-slice recipe validator
-> latest target closed: M56-05 sixth-slice combo-to-recipe consistency
-> latest target closed: M56-06 sixth-slice blocker repair candidates
-> latest target closed: M56-closeout sixth-slice runtime readiness decision
-> latest target closed: M57-01 sixth-slice human repair review packet
-> current next target: M60-closeout seventh-slice runtime readiness decision
   real artifact after M60-01 through M60-06 real outputs; M58-01 through M60-06 scaffolds are ready
```

## Completed Phases (M0-M19)

All historical milestones and detailed task records are archived in
`docs/IMPLEMENTATION_PLAN_ARCHIVE.md`. Summary:

| Phase | Description | Status |
|-------|-------------|--------|
| M0 | Bootstrap repo, Unity project, packages | Done |
| M1 | Card data pipeline, SQLite, verification | Done |
| M2 | Card browser, search, filters, detail | Done |
| M3 | Deck model, validator, builder UI, save/load/code | Done |
| M4 | Manual PlayTable, actions, replay | Done |
| M5 | Bot baseline, profiles, legal action generator | Done |
| M5.5 | RulesCore facade, seeded RNG, snapshot, AbilityCore | Done |
| M6 | Image cache, responsive UI, mobile readiness | Done |
| M7 | Custom pack schema, importer | Done |
| M8 | Multiplayer: Photon transport, lobby, room, reconnect, deck privacy, public events | Done |
| M9 | Trigger probability, board evaluator, battle search, guard estimator, playbook, combo discovery | Done |
| M10 | Ability/trigger automation foundation (112 sub-tasks) | Done |
| M11 | RulesCore completion: timing, facade coverage, replay, snapshot, hidden-state, resource ledger, RuleSet | Done |
| M12 | Structured card ability data: schema, validator, registry, templates, fixture DSL | Done |
| M13 | Owner-private rooms, command envelopes, public events, reconnect, lifecycle, spectator, tournament audit | Done |
| M14 | Heuristic bot v2, guard bot, trigger-risk, battle search v2, simulation, playbook, combo, ISMCTS gate, advanced search | Done |
| M15 | Custom pack v2, ability data, pack validation, pack manager, format profiles, Standard/V/Premium flags, sandbox | Done |
| M16 | UI polish: PlayTable panels, Deck Builder filters, Card Browser search, broken image, Android QA, smoke flow | Done |
| M17 | Headless CLI, batch runner, dataset export, observation/action/reward API, profiler, packed-state gate, worker | Done |
| M18 | CI: Python tests, data validation, Unity compile/EditMode, regression suites, Windows/Android build, release checklist | Done |
| M19 | Player experience reset: Home/Lobby, Deck Builder, PlayTable, Advanced drawer, states, smoke, icon override | Done |

## Current Verification Baseline

```powershell
python tools\verification\verify_vanguard_th_pack.py
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack_v2
python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite
python tools\data\import_custom_pack.py data\templates\custom_pack_v2 --output-dir work\custom_pack_v2_import --overwrite
python -m unittest discover -s tests -p "test_*.py"
```

Latest M51 Python/data verification: M51-01, M51-02, M51-03, and M51-04
generators passed; targeted tests passed `7/7`, `8/8`, `8/8`, and `9/9`;
full Python unittest discovery passed `902/902`. M51-01 selected
`โกลด์ พาลาดิน` / `link_joker_legion_mate` as the fifth offline slice. M51-02
found source_card_count `106`, trigger capacity `36`, non-trigger capacity
`388`, trigger gaps `[]`, fixture expectations met `true`, and semantic probe
ready `true`. M51-03 found semantic cards `106`, manual-review cards `4`, pair
graph edges `3075`, candidate edges `142`, all stage readiness passed `true`,
and saved-deck / UI / bot promotion disabled. M51-04 opened M52 offline recipe
pipeline with blockers `0`, fixture scaffold required `true`, runtime/UI/
saved-deck/bot/GameState mutation disabled, and next target `M52-01`.
M52-01 created the fifth-slice fixture scaffold with source-backed cards `106`,
trigger capacity `36`, non-trigger capacity `388`, candidate edges `142`,
scaffold_ready `true`, blockers `0`, runtime/UI/saved-deck/bot/GameState
mutation disabled, targeted tests `9/9`, full Python unittest discovery
`911/911`, and next target `M52-02`.
M52-02 exported the fifth-slice review packet with fixture scaffold items `1`,
manual-review cards `4`, candidate edges `142`, total review items `147`,
ready_for_m52_03 `true`, runtime/UI/saved-deck/bot/GameState mutation disabled,
targeted tests `8/8`, full Python unittest discovery `919/919`, and next
target `M52-03`.
M52-03 generated `25` fifth-slice advisory recipe drafts, all `25`
quantity-complete, trigger/missing skipped edges `0`, manual-overlap recipes
`0`, ready_for_m52_04 `true`, runtime/UI/saved-deck/bot/GameState mutation
disabled, targeted tests `8/8`, full Python unittest discovery `927/927`, and
next target `M52-04`.
M52-04 validated `25` fifth-slice advisory recipe drafts; runtime-ready recipes
`0`, validator-passed pending human selection `25`, invalid drafts `0`,
missing/copy/slot/trigger/manual-overlap blockers `0`, grade-profile review
recipes `25`, ready_for_m52_05 `true`, runtime/UI/saved-deck/bot/GameState
mutation disabled, targeted tests `7/7`, full Python unittest discovery
`934/934`, and next target `M52-05`.
M52-05 checked `25` fifth-slice recipe drafts, pair cards present `25`,
missing pair-card checks `0`, promotion allowed `0`, status
`consistent_pending_human_selection=25`, ready_for_m52_06 `true`, runtime/UI/
saved-deck/bot/GameState mutation disabled, targeted tests `6/6`, full Python
unittest discovery `940/940`, and next target `M52-06`.
M52-06 generated `25` fifth-slice blocker repair candidate previews, complete
grade-profile candidates `25`, human selection required `25`, unexpected
structural blocker recipes `0`, ready_for_m52_closeout `true`, runtime/UI/
saved-deck/bot/GameState mutation disabled, targeted tests `7/7`, full Python
unittest discovery `947/947`, and next target `M52-closeout`.
M52-closeout completed the fifth-slice runtime-readiness decision with
m52_complete `true`, runtime-ready recipe available `false`, human selection
review allowed `true`, next queue `M53`, runtime/UI/saved-deck/bot/GameState
mutation disabled, targeted tests `9/9`, full Python unittest discovery
`956/956`, and next target `M53-01`.
M53-01 exported `25` fifth-slice human repair review items, complete
grade-profile candidates `25`, human selection required `25`, unexpected
structural blockers `0`, ready_for_m53_02 `true`, runtime/UI/saved-deck/bot/
GameState mutation disabled, targeted tests `9/9`, full Python unittest
discovery `965/965`, and next target `M53-02`.
M53-02 recorded user-selected review item
`m53_01_m52_recipe_001_repair_review` for `m52_recipe_001`, selected grade
package `m52_recipe_001_grade_profile_pkg_001`, ready_for_m53_03 `true`,
runtime promotion disabled.
M53-03 recorded explicit acceptance for `m52_recipe_001`, applied the repair
preview in memory, produced a repaired main deck count `50`, repair application
issues `0`, ready_for_m53_04 `true`, and still did not declare the recipe valid
or promote runtime.
M53-04 reran repaired validation/consistency in memory; validation status
`validator_passed`, consistency status `consistent_validator_passed`,
runtime-ready recipes `1`, promotion-allowed checks `1`, blockers `0`, and
ready_for_m53_05 `true`.
M53-05 passed the runtime fixture gate with promotion_allowed `true`, passed
checks `5`, failed checks `0`, and created only the offline runtime/test
fixture `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`.
M53-closeout completed the fifth-slice fixture gate with m53_complete `true`,
fifth runtime fixture available `true`, next queue `M54`, saved-deck/UI/bot/
GameState mutation disabled, targeted M53 tests `35/35`, full Python unittest
discovery `1000/1000`, and next target `M54-01`.
M54-01 validated the Gold Paladin fifth fixture independently from the M53
generator; schema valid `true`, blockers `0`, main deck `50`, unique cards
`16`, trigger profile `Critical=4/Draw=4/Heal=4/Stand=4`, grade profile
`G0=17/G1=14/G2=11/G3=8`, ready_for_m54_02 `true`, targeted tests `8/8`,
full Python unittest discovery `1008/1008`, and next target `M54-02`.
M54-02 exported the Gold Paladin fifth fixture as review-only count-line deck
text; export_ready `true`, blockers `0`, main deck `50`, exported card lines
`16`, deck text
`outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt`,
deck text SHA-256
`95737f94c4505c0a2529f91459d9ee6677bb887bd746c8046fbae374285777fa`,
targeted tests `7/7`, full Python unittest discovery `1015/1015`, and next
target `M54-03`.
M54-03 loaded the Gold Paladin fifth fixture through deck-code headless smoke;
offline load ready `true`, Unity headless accepted `true`, deck source
`deck_code`, actions/events `4/4`, blockers `0`, targeted tests `9/9`, full
Python unittest discovery `1024/1024`, and next target `M54-04`.
M54-04 reviewed all five fixture evidence bundles; five fixtures passed, failed
fixtures `0`, candidate count `5`, sixth-slice offline pipeline allowed `true`,
targeted tests `8/8`, full Python unittest discovery `1032/1032`, and next
target `M55-01`.
M55-01 selected the sixth offline slice target: `ชาโดว์ พาลาดิน` / `g_next_z`,
rank `4`, candidate count `5`, ready_for_m55_02 `true`, targeted tests `7/7`,
full Python unittest discovery `1039/1039`, and next target `M55-02`.
M55-02 verified the sixth slice fixture/format readiness: source cards `77`,
grade profile `19/20/16/11/11`, trigger profile
`Critical=4/Draw=4/Heal=2/Stand=2`, semantic probe ready `true`, targeted tests
`8/8`, full Python unittest discovery `1047/1047`, and next target `M55-03`.
M55-03 completed the sixth-slice semantic/compatibility probe: semantic cards
`77`, manual-review cards `11`, pair graph edges `2069`, candidate edges `70`,
all stage readiness `true`, targeted tests `8/8`, full Python unittest
discovery `1055/1055`, and next target `M55-04`.
M55-04 opened the sixth-slice offline recipe pipeline: offline recipe pipeline
allowed `true`, blockers `0`, ready_for_m56 `true`, targeted tests `9/9`, full
Python unittest discovery `1064/1064`, and next target `M56-01`.
M56-01 built the sixth-slice fixture scaffold: scaffold_ready `true`,
blockers `0`, source cards `77`, Grade 4 cards advisory only until G Zone
support, targeted tests `9/9`, full Python unittest discovery `1073/1073`,
and next target `M56-02`.
M56-02 exported the sixth-slice review packet: review items `82`, manual-review
cards `11`, candidate edges `70`, ready_for_m56_03 `true`, targeted tests
`8/8`, full Python unittest discovery `1081/1081`, and next target `M56-03`.
M56-03 built advisory recipe drafts: recipe drafts `12`, quantity-complete
recipes `12`, skipped trigger/Grade 4/missing edges `58`, manual-overlap
recipes `12`, targeted tests `9/9`, full Python unittest discovery
`1090/1090`, and next target `M56-04`.
M56-04 validated the sixth-slice recipe drafts: validated drafts `12`,
runtime-ready recipes `0`, manual-review blocked recipes `12`, missing/copy/
slot/trigger/Grade 4 blockers `0`, targeted tests `7/7`, full Python unittest
discovery `1097/1097`, and next target `M56-05`.
M56-05 checked combo-to-recipe consistency: consistency checks `12`, pair cards
present `12`, missing pair-card checks `0`, promotion allowed `0`, recipe
manual dependencies `12`, G Zone deferred `12`, targeted tests `6/6`, full
Python unittest discovery `1103/1103`, and next target `M56-06`.
M56-06 built blocker repair candidates: repair items `12`, manual repair
complete `12`, grade repair complete `12`, G Zone deferred `12`, ready for
human repair review `12`, targeted tests `8/8`, full Python unittest discovery
`1111/1111`, and next target `M56-closeout`.
M56-closeout closed the sixth-slice readiness decision: M56 complete `true`,
runtime-ready recipe available `false`, next queue `M57`, manual review
blocked `12`, G Zone deferred `12`, targeted tests `9/9`, full Python unittest
discovery `1120/1120`, and next target `M57-01`.
M57-01 exported the sixth-slice human repair review packet: review items `12`,
complete manual repairs `12`, complete grade repairs `12`, G Zone deferred
items `12`, targeted tests `10/10`, full Python unittest discovery
`1130/1130`, and next target `M57-02`.
Latest
Unity/Windows verification is M32-04: Unity EditMode
1179/1179, Windows build `errors=0`
`warnings=0`, Windows player smoke `blockers=[]`, and visual evidence passed.
For Unity/C# changes, also run Unity batchmode compile and EditMode tests.

Previous M21-05a6 result: Unity EditMode 955/955, Windows player smoke
`blockers=[]`, battle flow status.

## Active Guardrails

- **Windows-only**: M20-M27 must not run Android, APK, LDPlayer, mobile QA, app
  packaging, release-candidate, or public distribution unless user reopens.
- **No comparator copying**: use VangPro/Vanguard Area/CGS/Dear Days as UX
  references only. Do not copy assets, code, icons, playmats, or pack data.
- **No GameState mutation from UI**: UI reads display state and sends commands
  through RulesCore facade.
- **Hidden state safety**: opponent, spectator, replay, and bot views must never
  see private data.
- **Taxonomy**: Card Browser/Deck Builder use `VanguardAreaClanTaxonomy` until
  explicitly replaced.
- **Icons**: use Lucide subset + `UiGameSymbolRegistry`. Private icons via
  `UI_ICON_OVERRIDE_SPEC.md` with fallback.
- **Transport**: keep Photon trusted-client rooms. No transport switch without
  ADR.

---

## M20: Windows Product Reset — Done

- Updated roadmap to Windows-first program completion.
- Removed Android/app/release from active queue.
- Added Windows playable-loop checklist and Windows-only verification profile.
- Closeout: `docs/M20_WINDOWS_PRODUCT_RESET_CLOSEOUT.md`.

## M21: PlayTable Windows UX Pass — Done

Spec: `docs/PLAYTABLE_WINDOWS_UX_V2_SPEC.md`

- `M21-01`: PlayTable v2 spec. **Done.**
- `M21-02`: Board/table area prominence. **Done.**
  Closeout: `docs/history/M21_02_PLAYTABLE_BOARD_FIRST_CLOSEOUT.md`.
- `M21-03`: Zone status panel (deck, hand, drop, damage, bind, order, ride deck,
  trigger zone counts). **Done.**
  Closeout: `docs/history/M21_03_PLAYTABLE_ZONE_STATUS_CLOSEOUT.md`.
- `M21-04`: Hand strip and selected-card preview. **Done.**
  - Hand as horizontal strip near player side, cards selectable.
  - Selected-card preview: card id, name, zone, type/grade/power/shield,
    card text (Thai skill text), and action hint.
  Closeout: `docs/history/M21_04_HAND_STRIP_SELECTED_PREVIEW_CLOSEOUT.md`.
- `M21-04b`: Soul zone status/resource ledger wiring. **Done.**
  - `GameZone.Soul` and `PlayerGameState.soul` already exist; do not add them
    again.
  - Zone status panel shows real `Soul`, `G Zone`, and `Guardian` counts.
  - PlayTable resource row includes visible Soul panel.
  - `ResourceLedgerState.FromGameState` derives available Soul from
    `PlayerGameState.soul`.
  - Closeout: `docs/history/M21_04B_SOUL_STATUS_LEDGER_CLOSEOUT.md`.
- `M21-04d`: Event-sourced ride/Soul resource command path. **Done.**
  - Add replay-safe command/event coverage for ride-to-soul.
  - Add live SoulBlast/SoulCharge execution only through covered command/event
    paths.
  - Preserve no-mutation rejects, replay determinism, and hidden-state masking.
  - Closeout: `docs/history/M21_04D_RIDE_SOUL_RESOURCE_COMMAND_CLOSEOUT.md`.
- `M21-04c`: Card image/thumbnail on board circles. **Done.**
  - Show minimal card thumbnail or name on vanguard/rear-guard circles.
  - Use existing `CardImageCache` for lazy load.
  - Closeout: `docs/history/M21_04C_BOARD_THUMBNAIL_CLOSEOUT.md`.
- `M21-05`: Player-facing common actions. **Done.**
  - `M21-05a`: Common action availability and phase buttons. **Done.**
    Closeout: `docs/history/M21_05A_ACTION_AVAILABILITY_CLOSEOUT.md`.
  - `M21-05a2`: Player-facing Check/Guard surface. **Done.**
    Closeout: `docs/history/M21_05A2_CHECK_GUARD_SURFACE_CLOSEOUT.md`.
  - `M21-05a3`: Drive/Damage trigger-check source split. **Done.**
    Closeout: `docs/history/M21_05A3_TRIGGER_SOURCE_SPLIT_CLOSEOUT.md`.
  - `M21-05a4`: Attack selected Vanguard/rear-guard to opponent Vanguard
    shortcut. **Done.**
    Closeout: `docs/history/M21_05A4_ATTACK_VANGUARD_SURFACE_CLOSEOUT.md`.
  - `M21-05a5`: Opponent target selection and `Atk Target` action. **Done.**
    Closeout: `docs/history/M21_05A5_ATTACK_TARGET_SELECTION_CLOSEOUT.md`.
  - `M21-05a6`: Battle Flow status guidance panel. **Done.**
    Closeout: `docs/history/M21_05A6_BATTLE_FLOW_STATUS_CLOSEOUT.md`.
  - `M21-05a7`: Manual Note local PlayTable surface. **Done.**
    Closeout: `docs/history/M21_05A7_MANUAL_NOTE_SURFACE_CLOSEOUT.md`.
  - `M21-05b1`: Setup readiness guard before PlayTable entry. **Done.**
    Closeout: `docs/history/M21_05B1_SETUP_READINESS_GUARD_CLOSEOUT.md`.
  - `M21-05b2`: First Vanguard setup from Ride Deck. **Done.**
    Closeout: `docs/history/M21_05B2_FIRST_VANGUARD_SETUP_CLOSEOUT.md`.
  - `M21-05b3`: Selected hand-card mulligan surface. **Done.**
    Closeout: `docs/history/M21_05B3_MULLIGAN_SELECTED_CLOSEOUT.md`.
  - `M21-05b4`: Stand/Ride phase legal action completion. **Done.**
    Closeout: `docs/history/M21_05B4_PHASE_ACTIONS_CLOSEOUT.md`.
  - `M21-05b5`: Setup status / finish guidance. **Done.**
    Closeout: `docs/history/M21_05B5_SETUP_STATUS_GUIDANCE_CLOSEOUT.md`.
  - Draw, Ride, Call, Move, Drive Check, Damage Check, Guard/Manual Note.
  - **Attack declaration flow**: select attacker → select target → opponent guard
    step → drive check → battle resolution → close step.
  - **Phase enforcement**: wire PhaseTimingMatrix (M11-02) into PlayTable so
    actions are only available in their legal phase.
  - **End Phase / Next Phase**: guided phase progression buttons.
- `M21-05b`: Game setup and deck-to-PlayTable flow. **Done.**
  - Setup readiness guard is complete; Home Solo Play and Deck Builder Start
    Game now require `DeckValidator.IsPlayable` before creating a PlayTable
    game state.
  - First Vanguard setup is complete; Mulligan phase can place a selected
    ride-deck card onto Vanguard through RulesCore.
  - Selected-card mulligan is complete; Mulligan phase can return one selected
    hand card and redraw through RulesCore.
  - Stand/Ride phase legal actions are complete; PlayTable phase buttons now
    execute through legal actions instead of default button interactivity.
  - Setup status guidance is complete; PlayTable tells the player when to pick
    first Vanguard, mulligan selected cards, or press Stand to begin.
  - Select deck from saved decks before entering PlayTable.
  - Guided setup: choose first vanguard → place face-down → shuffle → draw 5 →
    mulligan → determine first player → stand up vanguard.
  - Support starting Solo Play from Home with a selected deck.
- `M21-06`: Hide debug/automation/network payload in Advanced only. **Done.**
  Closeout: `docs/history/M21_06_ADVANCED_DEBUG_SURFACE_CLOSEOUT.md`.
- `M21-07`: Event/replay log player-readable. **Done.**
  Closeout: `docs/history/M21_07_PLAYER_READABLE_EVENT_LOG_CLOSEOUT.md`.
  - Prefer `P1 drew 1 card.` over raw reducer trace.
  - Hide event protocol ids, private instance ids, queue ids.
- `M21-08`: Tests: formatter/layout helpers, no direct `GameState` mutation,
  Windows player smoke. **Done.**
  Closeout: `docs/history/M21_08_TESTS_WINDOWS_SMOKE_CLOSEOUT.md`.
- `M21-09`: Closeout: manual table on Windows is easier to understand. **Done.**
  Closeout: `docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md`.

## M22: Windows Settings / Deck Type / Accessories - Done

- `M22-01`: `PlayerSettings`: player name, default deck, preferred format, UI
  scale, image/cache mode. **Done.**
  Closeout: `docs/history/M22_01_PLAYER_SETTINGS_CLOSEOUT.md`.
- `M22-02`: `DeckAppearanceMetadata`: sleeve/card back, playmat key,
  crest/persona shield/marker options. **Done.**
  Closeout: `docs/history/M22_02_DECK_APPEARANCE_METADATA_CLOSEOUT.md`.
- `M22-03`: Settings screen from Home. **Done.**
  Closeout: `docs/history/M22_03_HOME_SETTINGS_SCREEN_CLOSEOUT.md`.
- `M22-04`: Deck Type / Accessories dialog in Deck Builder. **Done.**
  Closeout: `docs/history/M22_04_DECK_TYPE_ACCESSORIES_DIALOG_CLOSEOUT.md`.
- `M22-05`: Keep cosmetic metadata separate from deck legality validation.
  **Done.**
  Closeout: `docs/history/M22_05_COSMETIC_LEGALITY_SEPARATION_CLOSEOUT.md`.
- `M22-06`: User-provided asset slot through manifest/hash/fallback only.
  **Done.**
  Closeout: `docs/history/M22_06_USER_DECK_ASSET_SLOT_CLOSEOUT.md`.
- `M22-07`: Tests: JSON round-trip, fallback, deck validation unchanged.
  **Done.**
  Closeout: `docs/history/M22_07_SETTINGS_ACCESSORIES_TEST_ROLLUP_CLOSEOUT.md`.
- `M22-08`: Closeout. **Done.**
  Closeout: `docs/history/M22_WINDOWS_SETTINGS_ACCESSORIES_CLOSEOUT.md`.

## M23: In-App Manual / Tutorial

- `M23-01`: Manual content spec. **Done.**
  Closeout: `docs/history/M23_01_MANUAL_CONTENT_SPEC_CLOSEOUT.md`.
  - **App Guide**: Playing Field, Deck Builder, PlayTable, Online Room, Replay,
    Custom Packs.
  - **Vanguard Rules Basics**: phase flow, turn structure, trigger types, combat
    math, zone purposes, format differences (Standard/V-Premium/Premium).
    **New — gap fix.**
- `M23-02`: Manual screen from Home and PlayTable. **Done.**
  Closeout: `docs/history/M23_02_MANUAL_SCREEN_CLOSEOUT.md`.
- `M23-03`: Loading tips for data reload, card images, deck load. **Done.**
  Closeout: `docs/history/M23_03_LOADING_TIPS_CLOSEOUT.md`.
- `M23-04`: Write original content only. **Done.**
  Closeout: `docs/history/M23_04_ORIGINAL_CONTENT_GATE_CLOSEOUT.md`.
- `M23-05`: Simple search/category filter in Manual. **Done.**
  Closeout: `docs/history/M23_05_MANUAL_FILTER_CLOSEOUT.md`.
- `M23-06`: Tests: content load, missing content fallback, navigation. **Done.**
  Closeout: `docs/history/M23_06_MANUAL_TESTS_CLOSEOUT.md`.
- `M23-07`: Closeout: new player can understand both the app and the game. **Done.**
  Closeout: `docs/history/M23_MANUAL_TUTORIAL_CLOSEOUT.md`.

## M24: Deck Builder / Import / Custom Pack UX

- `M24-01`: Windows landscape Deck Builder: preview, grid, deck list, counters,
  rule badge. **Done.**
  Closeout: `docs/history/M24_01_DECK_BUILDER_WINDOWS_LANDSCAPE_CLOSEOUT.md`.
- `M24-02`: Human-readable count-line deck export/import. **Done.**
  Closeout: `docs/history/M24_02_COUNT_LINE_DECK_TEXT_CLOSEOUT.md`.
- `M24-03`: Deck-code mismatch UI for missing card, pack version, hash mismatch. **Done.**
  Closeout: `docs/history/M24_03_DECK_IMPORT_MISMATCH_UI_CLOSEOUT.md`.
- `M24-04`: CGS-like custom pack adapter spec (reference only). **Done.**
  Closeout: `docs/history/M24_04_CGS_LIKE_CUSTOM_PACK_ADAPTER_SPEC_CLOSEOUT.md`.
- `M24-05`: VangPro-like custom import spec for `.csv/.xlsx + images.zip +
  manifest/hash`. **Done.**
  Closeout: `docs/history/M24_05_VANGPRO_LIKE_CUSTOM_IMPORT_SPEC_CLOSEOUT.md`.
- `M24-06`: Local custom import validator first; no auto-download. **Done.**
  Closeout: `docs/history/M24_06_LOCAL_CUSTOM_IMPORT_VALIDATOR_CLOSEOUT.md`.
- `M24-07`: Pack validation UI: schema, set count, card count, missing images,
  unsupported fields, copyright note. **Done.**
  Closeout: `docs/history/M24_07_PACK_VALIDATION_UI_CLOSEOUT.md`.
- `M24-08`: Deck image export. **Done.**
  Closeout: `docs/history/M24_08_DECK_IMAGE_EXPORT_CLOSEOUT.md`.
- `M24-09`: Tests: parser, validator, failed import no mutation, isolated pack.
  **Done.**
  Closeout: `docs/history/M24_09_CUSTOM_IMPORT_WORKFLOW_TEST_ROLLUP_CLOSEOUT.md`.
- `M24-10`: Closeout. **Done.**
  Closeout: `docs/history/M24_DECK_BUILDER_IMPORT_CUSTOM_PACK_UX_CLOSEOUT.md`.

## M25: Windows Online Room Usability

- `M25-01`: Keep Photon trusted-client room. **Done.**
  Closeout: `docs/history/M25_01_PHOTON_TRUSTED_CLIENT_ROOM_POLICY_CLOSEOUT.md`.
- `M25-02`: Lobby flow: create, join, ready, start, rematch, back home.
  **Done.**
  Closeout: `docs/history/M25_02_LOBBY_FLOW_CLOSEOUT.md`.
- `M25-03`: Room status: connection, player count, deck hash, pack hash, public
  cursor. **Done.**
  Closeout: `docs/history/M25_03_ROOM_STATUS_CLOSEOUT.md`.
- `M25-04`: Reconnect UX with clear failure reasons.
  **Done.**
  Closeout: `docs/history/M25_04_RECONNECT_UX_CLOSEOUT.md`.
- `M25-05`: Hide payload/debug in default online PlayTable UI.
  **Done.**
  Closeout: `docs/history/M25_05_ONLINE_PLAYTABLE_DEFAULT_UI_CLOSEOUT.md`.
- `M25-06`: Player-facing replay sync/status.
  **Done.**
  Closeout: `docs/history/M25_06_REPLAY_SYNC_STATUS_CLOSEOUT.md`.
- `M25-07`: Tests: no deck-code leak, stale cursor reject, reconnect display,
  masked event delivery. **Done.**
  Closeout: `docs/history/M25_07_ONLINE_ROOM_TEST_ROLLUP_CLOSEOUT.md`.
- `M25-08`: Closeout: friend-room on Windows is easier to use. **Done.**
  Closeout: `docs/history/M25_WINDOWS_ONLINE_ROOM_USABILITY_CLOSEOUT.md`.

## M26: Bot / Automation Return Gate

- `M26-01`: Audit M21-M25 before returning to bot/automation. **Done.**
  Closeout: `docs/history/M26_01_BOT_AUTOMATION_RETURN_AUDIT_CLOSEOUT.md`.
- `M26-02`: Bot uses legal action mask and masked state only. **Done.**
  Closeout: `docs/history/M26_02_BOT_LEGAL_ACTION_MASKED_STATE_GATE_CLOSEOUT.md`.
- `M26-03`: Player-readable bot explanation panel. **Done.**
  Closeout: `docs/history/M26_03_BOT_EXPLANATION_PANEL_CLOSEOUT.md`.
- `M26-04`: Structured ability only for templates with tests. **Done.**
  Closeout: `docs/history/M26_04_STRUCTURED_ABILITY_TEMPLATE_GATE_CLOSEOUT.md`.
- `M26-05`: No LLM/runtime text parsing for live effect resolution. **Done.**
  Closeout: `docs/history/M26_05_LIVE_EFFECT_NO_TEXT_PARSING_GATE_CLOSEOUT.md`.
- `M26-06`: Solo Play entry flow from Home. **Done.**
  Closeout: `docs/history/M26_06_SOLO_PLAY_HOME_ENTRY_FLOW_CLOSEOUT.md`.
  - "เริ่มเกม Solo" from Home → select deck → select bot difficulty
    (Easy/Normal/Hard) → select bot deck or random → enter PlayTable.
- `M26-07`: Tests: no hidden leak, simulation no live mutation, replay
  deterministic. **Done.**
  Closeout: `docs/history/M26_07_BOT_AUTOMATION_SAFETY_REGRESSION_CLOSEOUT.md`.
- `M26-08`: Closeout: CPU/bot can continue without bloating core.
  **Done.**
  Closeout: `docs/history/M26_BOT_AUTOMATION_RETURN_GATE_CLOSEOUT.md`.

## M27: Windows Stability Pass

- `M27-01`: Windows player smoke covers Home, Deck Builder, PlayTable, Manual,
  Settings, Online Room. **Done.**
  Closeout: `docs/history/M27_01_WINDOWS_STABILITY_SMOKE_CLOSEOUT.md`.
- `M27-02`: Fix crash/blockers found by Windows smoke. **Done.**
  Closeout: `docs/history/M27_02_WINDOWS_SMOKE_BLOCKER_REVIEW_CLOSEOUT.md`.
- `M27-03`: Baseline performance of Card Browser and Deck Builder.
  **Done.**
  Closeout: `docs/history/M27_03_WINDOWS_PERFORMANCE_BASELINE_CLOSEOUT.md`.
- `M27-04`: Memory / performance gate. **Expanded — gap fix.**
  **Done.**
  Closeout: `docs/history/M27_04_WINDOWS_PERFORMANCE_GATE_CLOSEOUT.md`.
  - Image memory: verify `CardImageCache` unloads unused textures.
  - Card Browser scroll: 10K+ cards without frame drop.
  - PlayTable frame rate target: 30+ fps.
  - Profile headless CLI for memory ceiling.
- `M27-05`: Graceful error handling. **New — gap fix.**
  **Done.**
  Closeout: `docs/history/M27_05_WINDOWS_GRACEFUL_ERROR_HANDLING_CLOSEOUT.md`.
  - Card database load failure → error dialog with retry.
  - Missing image → fallback (already exists, verify coverage).
  - Unhandled exception → log + user message instead of silent crash.
- `M27-06`: Integration / PlayMode test. **New — gap fix.**
  **Done.**
  Closeout: `docs/history/M27_06_WINDOWS_PLAYMODE_INTEGRATION_CLOSEOUT.md`.
  - At least one PlayMode test: Home → select deck → enter PlayTable → draw →
    ride → end turn.
  - Verify no runtime exceptions in the automated flow.
- `M27-07`: Known limitations list for local Windows build.
  **Done.**
  Closeout: `docs/history/M27_07_WINDOWS_KNOWN_LIMITATIONS_CLOSEOUT.md`.
- `M27-08`: Do not make a public release until the user asks.
  **Done.**
  Closeout: `docs/history/M27_08_NO_PUBLIC_RELEASE_GATE_CLOSEOUT.md`.

---

## M28: Windows Gameplay Completion Pass

Spec: `docs/specs/ci_and_qa/WINDOWS_GAMEPLAY_COMPLETION_PASS_SPEC.md`

- `M28-01`: Windows gameplay completion gate. **Done.**
  - Replace the old short PlayTable smoke with a deterministic manual route:
    setup both first Vanguards, keep mulligan, Stand & Draw, Draw, Ride,
    Main, call rear-guard, Battle, attack, guard, Drive check, Damage check,
    End phase.
  - Verify committed event count, key zone counts, final phase, and replay
    determinism.
  - Keep scope Windows-only and manual-simulator focused.
  Closeout: `docs/history/M28_01_WINDOWS_GAMEPLAY_COMPLETION_GATE_CLOSEOUT.md`.
- `M28-02`: Local PlayTable seat toggle. **Done.**
  - Local manual PlayTable must switch between P1/P2 without recreating state.
  - Online PlayTable must stay locked to the session local player.
  - Switching seats must clear selected card/target UI state and avoid direct
    `GameState` mutation.
  Closeout: `docs/history/M28_02_LOCAL_PLAYTABLE_SEAT_TOGGLE_CLOSEOUT.md`.
- `M28-03`: UI-level two-seat match smoke. **Done.**
  - Use PlayMode runtime UI to set both first Vanguards, switch seats, play P1
    through turn flow, guard/check as P2, return to P1, and reach End phase.
  - This must use UI controls and display-state assertions, not direct
    `GameState` mutation.
  Closeout: `docs/history/M28_03_UI_TWO_SEAT_MATCH_SMOKE_CLOSEOUT.md`.
- `M28-04`: Windows manual match gap audit. **Done.**
  - Review the M28-01 to M28-03 gates and identify remaining player-facing
    gaps before adding more systems.
  - Keep the audit Windows-only and focused on manual match completion:
    setup, phase navigation, battle/guard/check controls, selected-card
    feedback, event readability, and table navigation.
  - Do not start Android/mobile/release work and do not copy comparator
    assets/code/data.
  Closeout: `docs/history/M28_04_WINDOWS_MANUAL_MATCH_GAP_AUDIT.md`.
- `M28-05`: PlayTable guided next-action panel. **Done.**
  - Add a pure formatter that tells the current local seat what to do next.
  - Surface the hint in the PlayTable side panel without changing core rules.
  - Cover setup, mulligan/stand, ride, main/call, battle/attack,
    guard/check, drive check, and end hints.
  - Add EditMode tests and run Unity compile/EditMode/client smoke.
  Closeout: `docs/history/M28_05_PLAYTABLE_GUIDED_NEXT_ACTION_CLOSEOUT.md`.
- `M28-06`: Windows built-player smoke. **Done.**
  - Rebuild the Windows executable after M28 UI changes.
  - Run `VanguardThaiSim.exe -vanguardPlayerSmoke`.
  - Verify player smoke JSON reports `blockers=[]`.
  - Keep this as local Windows verification only; no release packaging.
  Closeout: `docs/history/M28_06_WINDOWS_BUILT_PLAYER_SMOKE_CLOSEOUT.md`.
- `M28-07`: PlayTable action grouping polish. **Done.**
  - Improve action-row scanability without changing RulesCore.
  - Keep commands and legal-action gating intact.
  - Add tests for any formatter/layout helper change.
  - Verify with compile/EditMode/PlayMode/client smoke and Windows player smoke
    if runtime UI changes are made.
  Closeout: `docs/history/M28_07_PLAYTABLE_ACTION_GROUPING_POLISH_CLOSEOUT.md`.
- `M28-08`: PlayTable side-panel density audit. **Done.**
  - Audit whether added Next Action and Action Groups text makes side panel too
    dense.
  - Decide whether to shorten, collapse, or move any panels into Advanced.
  - Keep this as Windows UI audit/spec before more code.
  Closeout: `docs/history/M28_08_PLAYTABLE_SIDE_PANEL_DENSITY_AUDIT.md`.
- `M28-09`: Move Bot Plan out of primary manual panel. **Done.**
  - Preserve bot explanation functionality.
  - Move Bot Plan into Advanced drawer or hide it when no bot trace is active.
  - Keep manual table primary panel focused on player flow.
  - Verify runtime UI because this changes the PlayTable surface.
  Closeout: `docs/history/M28_09_BOT_PLAN_ADVANCED_DRAWER_CLOSEOUT.md`.
- `M28-10`: Match Log / Preview density review. **Done.**
  - Review whether Match Log and Selected Card Preview still crowd the side
    panel after M28-09.
  - Keep Selected Card Preview primary unless a visual audit proves otherwise.
  - Consider compact Match Log summary before any larger PlayTable rewrite.
  Closeout: `docs/history/M28_10_MATCH_LOG_PREVIEW_DENSITY_CLOSEOUT.md`.

## M29: Photon Lobby / Room UI Reopen Pass

Goal: follow the active online-room objective without changing transport,
payloads, deck privacy policy, reconnect protocol, or `GameState`.

- `M29-01`: Photon lobby navigation lockout. **Done.**
  - Disable normal `Back Home` while a Photon room is active.
  - Require explicit `Leave Room` before returning Home.
  - Add formatter/test coverage with no deck-code leak.
  Closeout: `docs/history/M29_01_PHOTON_LOBBY_NAVIGATION_LOCKOUT_CLOSEOUT.md`.
- `M29-02`: Photon lobby reconnect flow polish review. **Done.**
  - Verify reconnect request, batch, cursor, room mismatch, and Start Table
    handoff messaging in the current runtime lobby.
  - If a verified player-facing gap exists, fix the smallest UI/controller
    surface needed and rerun compile, EditMode, client smoke, and Windows
    player smoke when runtime UI changes.
  - Do not change Photon event codes or replay semantics without a separate
    spec update.
  Closeout: `docs/history/M29_02_PHOTON_LOBBY_RECONNECT_FLOW_POLISH_CLOSEOUT.md`.
- `M29-03`: Photon lobby Quick Deck Selector / Quick Edit. **Done.**
  - Let the player inspect/change the active lobby deck before hosting,
    joining, or reconnecting.
  - Do not mutate room state after a room is active; require leaving the room
    before changing the active online deck.
  - Keep deck codes hidden in status text.
  - Start with a spec and the smallest local UI slice that can be verified.
  Closeout: `docs/history/M29_03_PHOTON_LOBBY_QUICK_DECK_SELECTOR_CLOSEOUT.md`.
- `M29-04`: Photon lobby Quick Edit modal / return-flow. **Done.**
  - Decide whether the safest next slice is an embedded lobby edit modal or a
    Deck Builder return handoff.
  - Preserve room lockout: no online deck edits while `CurrentRoom` is active.
  - Keep this as a player-facing prep workflow; do not change Photon payloads
    or deck privacy rules.
  Closeout: `docs/history/M29_04_PHOTON_LOBBY_QUICK_EDIT_MODAL_CLOSEOUT.md`.
- `M29-05`: Online Room usability closeout audit. **Done.**
  - Check the Online Room requirements in `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`
    after M29-01 through M29-04.
  - Identify any remaining player-facing Photon lobby/room blockers.
  - Decide whether to continue Photon UI work or return to deferred `M28-10`.
  Closeout: `docs/history/M29_05_ONLINE_ROOM_USABILITY_CLOSEOUT_AUDIT.md`.
- `M29-06`: Online deck readiness guard. **Done.**
  - Block Host, Join, and Reconnect before room state is created when the
    active lobby deck is missing or count-unready.
  - Keep this as a lightweight local count guard; do not add repository-backed
    card-id validation in this slice.
  Closeout: `docs/history/M29_06_ONLINE_DECK_READINESS_GUARD_CLOSEOUT.md`.

## M30: Windows Playable Loop Final Audit

Spec: `docs/WINDOWS_PLAYABLE_LOOP_FINAL_AUDIT_SPEC.md`

- `M30-01`: Windows playable loop final audit. **Done.**
  - Audit Home, Deck Builder, PlayTable, Online Room, Manual, Settings, custom
    pack UX, and known limitations after M28/M29.
  - Classify findings as blocker, polish, or deferred.
  - Decide the next implementation slice from evidence instead of opening a new
    system by default.
  Closeout: `docs/history/M30_01_WINDOWS_PLAYABLE_LOOP_FINAL_AUDIT.md`.
- `M30-02`: Windows Replay entry/browser. **Done.**
  - Unlock the Home Replay route into a player-facing Replay screen.
  - Show clear empty-state guidance if no local replay file is selected.
  - Keep replay event semantics and network replay protocols unchanged.
  Closeout: `docs/history/M30_02_WINDOWS_REPLAY_ENTRY_BROWSER_CLOSEOUT.md`.
- `M30-03`: Windows Replay local file import. **Done.**
  - Add a path input and load action for local replay JSON.
  - Validate with `GameReplay.FromJson`.
  - Failed import must not mutate live `GameState`.
  Closeout: `docs/history/M30_03_WINDOWS_REPLAY_LOCAL_FILE_IMPORT_CLOSEOUT.md`.
- `M30-04`: Windows Replay viewer launch. **Done.**
  - Launch or preview a loaded replay through `GameReplayPlayer`.
  - Keep replay event semantics and network protocols unchanged.
  - Failed launch must not mutate live `GameState`.
  Closeout: `docs/history/M30_04_WINDOWS_REPLAY_VIEWER_LAUNCH_CLOSEOUT.md`.
- `M30-05`: Windows PlayTable replay export. **Done.**
  - Export a local PlayTable replay JSON from initial state plus event log.
  - Use a deterministic local `work/` path first; no native save dialog.
  - Export must not mutate live state and must stay local-only.
  Closeout: `docs/history/M30_05_WINDOWS_PLAYTABLE_REPLAY_EXPORT_CLOSEOUT.md`.
- `M30-06`: Windows playable loop M30 closeout audit. **Done.**
  - Audit the complete Windows playable loop now that Replay export/import/view
    exists.
  - Classify remaining gaps as blocker, polish, deferred, or next-feature.
  - Set the next target from evidence, without opening Android/mobile/release
    work.
  Closeout: `docs/history/M30_06_WINDOWS_PLAYABLE_LOOP_CLOSEOUT_AUDIT.md`.

## M31: Windows UI Evidence / Polish Pass

Spec: `docs/WINDOWS_UI_EVIDENCE_AUDIT_SPEC.md`

Goal: improve player-facing Windows UX from evidence without opening Android,
release, or unrelated automation tracks.

- `M31-01`: Windows UI evidence capture and blocker/polish audit. **Done.**
  - Review current Windows player screens against
    `docs/UI_EXPERIENCE_REDESIGN_SPEC.md`.
  - Prioritize concrete blocker/polish fixes by screen: Home, Card Browser,
    Deck Builder, PlayTable, Replay, Settings, Online Room.
  - Do not copy comparator assets/code/data.
  - Do not expand bot/automation unless the audit explicitly closes UX
    readiness first.
  Closeout: `docs/history/M31_01_WINDOWS_UI_EVIDENCE_AUDIT.md`.
- `M31-02`: Card Workshop first-screen clarity pass. **Done.**
  - Clarify Card Browser/Deck Builder first-screen status and next actions.
  - Keep card queries, deck validation, runtime pack data, and RulesCore
    unchanged.
  - Add formatter/UI tests and Windows-only verification.
  Closeout: `docs/history/M31_02_CARD_WORKSHOP_FIRST_SCREEN_CLARITY_CLOSEOUT.md`.
- `M31-03`: Card Workshop toolbar density pass. **Done.**
  - Reduce top-toolbar density while preserving search/filter/page behavior.
  - Demote secondary controls such as cache.
  - Add tests and Windows-only verification.
  Closeout: `docs/history/M31_03_CARD_WORKSHOP_TOOLBAR_DENSITY_CLOSEOUT.md`.
- `M31-04`: Windows UI visual evidence pass. **Done.**
  - Capture or review current Windows visual evidence after M31-02/M31-03.
  - Decide the next UI slice from screenshots/player-visible evidence.
  - Keep this Windows-only and avoid gameplay/rules/network scope.
  Closeout: `docs/history/M31_04_WINDOWS_UI_VISUAL_EVIDENCE_PASS_CLOSEOUT.md`.
- `M31-05`: Card detail preview aspect-ratio fix. **Next.**
  - Fix selected-card detail preview stretching in Card Browser and Deck
    Builder.
  - Preserve card data, image files, search/query behavior, and deck
    validation.
  - Verify with tests and refreshed Windows visual evidence.

## M32: Vanguard Area-Style PlayTable Reset

Specs:

- `docs/VANGUARD_AREA_STYLE_PLAYTABLE_RESET_SPEC.md`
- `docs/VANGUARD_DIGITAL_CLIENT_UX_BLUEPRINT.md`

Goal: respond to user/team feedback that the current PlayTable design is hard
to use. Stop polishing the dashboard-style direction and reset the PlayTable
toward a Vanguard Area-style manual table mental model without copying
proprietary assets, code, or data.

- `M32-01`: PlayTable field-mat shell and compact command dock. **Done; needs M32-02 visual polish.**
  - Replace the row-heavy table body with a field-mat layout where zones are
    the first visible structure.
  - Move primary commands into a compact two-row dock that cannot push the
    side panel off screen.
  - Keep existing RulesCore/session command paths unchanged.
- `M32-01b`: Digital-client UX blueprint from public/reference study. **Done.**
  - Record observed UX patterns from Vanguard Area, VangPro, Cardfight
    Connect, Vanguard EX, Dear Days, and Dear Days 2.
  - Added public-reference coverage for Ride to Victory, Lock on Victory,
    Stride to Victory, Cardfight Online, Vanguard EX, Dear Days, and Dear Days
    2 using Fandom, YouTube, Google Images, official pages, and public article
    screenshots.
  - Stored capture manifest at
    `outputs/vanguard_video_game_ux_research/source_manifest.json`.
  - Link captured reference frames and current PlayTable visual evidence.
  - Keep the no-copy boundary explicit.
- `M32-02`: Zone placement polish and board-card sizing. **Done.**
  - Make the PlayTable read as a card table at first glance.
  - Add missing trigger/order zone surfaces.
  - Reduce command/HUD dominance.
  - Keep the hand strip visible and unclipped.
  Closeout: `docs/history/M32_02_ZONE_PLACEMENT_POLISH_CLOSEOUT.md`.
- `M32-02b`: Letsplay layout research alignment. **Done.**
  - Installed and verified `yt-dlp` for metadata indexing.
  - Indexed the supplied public letsplay playlists/videos.
  - Recorded layout-only findings for field, zones, hand, phase rail, action
    prompts, inspect/log surfaces, and online table HUD.
  - Explicitly excluded story, character, dialogue, campaign, copied assets,
    official icons, playmats, code, and extracted game data from the current
    PlayTable reset.
  Research summary:
  `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/LETSPLAY_LAYOUT_RESEARCH_SUMMARY.md`.
  Closeout: `docs/history/M32_02B_LETSPLAY_LAYOUT_RESEARCH_CLOSEOUT.md`.
- `M32-03`: PlayTable de-dashboard field/HUD pass. **Done.**
  - Remove or collapse the permanent right Inspect column.
  - Promote field zones, bottom hand strip, phase rail, and contextual action
    bar.
  - Move logs/setup/zone status/bot/online/trigger/ability diagnostics into
    Advanced or collapsible surfaces.
  - Reserve a right field gutter so the compact Inspect HUD does not cover
    Trigger, Drop, Bind, or Gift Marker zones.
  Closeout: `docs/history/M32_03_PLAYTABLE_DE_DASHBOARD_FIELD_HUD_CLOSEOUT.md`.
- `M32-04`: Playmat slot layout / Windows visual evidence closeout. **Done.**
  - Replaced long rear-guard bars with a playmat-style slot skeleton:
    opponent/local front and back RG slots plus VG center slots.
  - Moved local pile markers toward a playmat mental model: damage/order left,
    deck/drop/bind/ride/trigger/gift right, and Soul off the VG centerline.
  - Kept comparator playmat assets out of the project; `Field1.png` was used
    only as a layout reference.
  - Changed compact pile zones to marker/count surfaces instead of overflowing
    card lists.
  - Remaining UX gap: hand strip and pile expansion/selection need a dedicated
    M32-05 pass.
  Closeout: `docs/history/M32_04_PLAYMAT_SLOT_LAYOUT_CLOSEOUT.md`.
- `M32-05`: Hand strip and compact pile interaction polish. **Paused by user.**
  - Make the hand strip fully visible at 1280x720.
  - Add an expanded/overlay interaction path for compact pile zones such as
    Ride Deck so the field can stay clean while manual selection remains
    usable.
  - Keep the no-copy asset boundary and existing RulesCore/session command
    paths.

## M33: Offline Clan Combo Pairing Logic

Goal: pause UI work and build a first offline algorithm that pairs advisory
combo candidates inside each clan for the early Vanguard card pool.

Spec: `docs/specs/bot_and_headless/CLAN_COMBO_PAIRING_OFFLINE_SPEC.md`.

- `M33-01`: TD01-TD06 / BT01-BT09 / EB01-EB05 clan combo pairing report. **Done.**
  - Load cards from `data/packs/vanguard_th/cards.sqlite`.
  - Filter `series_code` to TD01-TD06, BT01-BT09, and EB01-EB05.
  - Group by `clan`.
  - Extract offline heuristic features from card metadata/text.
  - Score explainable pair synergies inside each clan.
  - Export deterministic JSON to
    `outputs/combo_discovery/td01_td06_bt01_bt09_eb01_eb05_clan_combos.json`.
  - Add Python unit tests.
  - Do not mutate `GameState`, consume RNG, parse live match text, or claim full
    effect legality.
  Closeout: `docs/history/M33_01_CLAN_COMBO_PAIRING_OFFLINE_CLOSEOUT.md`.
- `M33-02`: Era preset expansion for Link Joker/Legion, G, V, D, and DZ ranges. **Done.**
  - Added set-range parsing for prefixed codes such as `G-TD01-G-TD09`,
    `V-EB10-V-EB15`, `D-LBT03-D-LBT04`, and `DZ-SD01-DZ-SD06`.
  - Added era presets for `link_joker_legion_mate`, `g_series_first`,
    `g_next_z`, `v_reboot`, `v_shinemon_if`, `d_overdress`, `d_willdress`,
    and `dz_divinez`.
  - Added auto grouping that uses clan first and falls back to nation for
    D/DZ-style `clan=N/A` cards.
  - Generated one JSON report per preset plus
    `outputs/combo_discovery/era_combo_report_summary.json`.
  - Missing runtime pack set codes are recorded in each report.
  Closeout: `docs/history/M33_02_ERA_COMBO_PRESET_EXPANSION_CLOSEOUT.md`.
- `M33-03`: Matrix artifacts for era/group/synergy review. **Done.**
  - Added `tools/combo/build_combo_matrices.py`.
  - Generated `combo_matrix_era_summary.csv`,
    `combo_matrix_group_candidates.csv`, `combo_matrix_group_cards.csv`,
    `combo_matrix_top_pair_scores.csv`, `combo_matrix_synergy_tags.csv`, and
    `combo_matrix_summary.json`.
  - Matrix scope is summary-level, not full card x card adjacency.
  Closeout: `docs/history/M33_03_COMBO_MATRIX_ARTIFACTS_CLOSEOUT.md`.
- `M33-04`: Review high-confidence clan/nation combo pairs for playbook seed data. **Deferred until Phase D.**
  - Manually inspect top pairs per clan/nation from the M33-01 and M33-02 JSON reports.
  - Mark selected pairs as playbook seed candidates only after review.
  - Do not wire heuristic pairs directly into runtime bot choices.
  - Do not promote heuristic pairs until the selected slice passes Phase A
    legality, Phase B semantic, and Phase C compatibility/conflict gates.
  - Prefer structured ability fixture candidates when a pair depends on exact
    timing, cost, target, or board position.

## M34: Offline Deck Construction Possibility

Goal: calculate whether each clan/nation has enough local card data to build
theoretical decks before spending time on combo/playbook review.

Spec: `docs/specs/cards_and_decks/DECK_POSSIBILITY_ANALYSIS_SPEC.md`.

- `M34-01`: Deck possibility analysis by clan/nation group. **Done.**
  - Added `tools/deck/analyze_deck_possibilities.py`.
  - Generated per-preset JSON reports under `outputs/deck_possibility/`.
  - Generated `deck_possibility_summary.csv` and
    `deck_possibility_summary.json`.
  - Calculates 50-card capacity, 16-trigger/34-non-trigger capacity, bounded
    count-distribution possibilities, ride grade choice availability, and
    G-zone capacity.
  - This is theoretical math only, not complete official deck legality.
  Closeout: `docs/history/M34_01_DECK_POSSIBILITY_ANALYSIS_CLOSEOUT.md`.
- `M34-02`: Rule corpus intake and plan refresh. **Done.**
  - Copied the 2026-06-29 research corpus into
    `outputs/research_2026_06_29_new_chat/`.
  - Reviewed rule taxonomy, rule-engine spec, implementation checklist,
    field/zone blueprints, official source map, mechanic presence matrix, and
    effect-condition notes.
  - Added `docs/specs/cards_and_decks/RULE_CORPUS_DECK_COMBO_PLAN_SPEC.md`.
  - Closeout: `docs/history/M34_02_RULE_CORPUS_PLAN_REFRESH_CLOSEOUT.md`.
- `M34-03`: Deck-feasible archetype priority v2 / Phase A1. **Done.**
  - Use `deck_possibility_summary.csv` to choose clans/nations that pass
    50-card and 16-trigger feasibility.
  - Combine M33 combo matrix outputs with rule-complexity metadata from the
    new corpus.
  - Prioritize feasible/high-data groups with lower-risk mechanics first.
  - Defer groups with missing set data, insufficient trigger/non-trigger
    capacity, or unsupported mechanic modules until data/rules are filled.
  - Generated
    `outputs/archetype_priority/archetype_priority_ranking.csv`.
  - Generated
    `outputs/archetype_priority/archetype_priority_ranking.json`.
  - Ranked `45` groups; `37` groups are deck-feasible by current offline
    capacity checks.
  - Closeout: `docs/history/M34_03_ARCHETYPE_PRIORITY_CLOSEOUT.md`.

## Hybrid Vertical-Slice Strategy: Phase A-E

This plan keeps deck/combo work vertical. The project should not build a full
taxonomy layer, full legality layer, full semantic layer, and full
compatibility layer before producing a useful deck/combo artifact.

Instead, each phase must push one selected target slice closer to an explainable
deck skeleton:

```text
select slice -> source-backed format policy -> semantic tags ->
compatibility checks -> reviewed deck skeleton -> safe playbook seed
```

Allowed target slices:

- `Classic Core`: early clan-era deck/combo work such as
  `TD01-TD06 / BT01-BT09 / EB01-EB05`.
- `Standard D/DZ`: current nation-era deck/combo work when the selected group
  depends on ride deck, orders, over triggers, Energy, or Divine Skill.
- Later slices for `V Premium`, `G/Premium`, or title/collab mechanics only
  after the first slice proves the process.

Do not assume Standard by default. Choose the first slice from M34-03 evidence
and user/team priority.

### Phase A: Foundation Slice

Goal: pick one target slice and prove the minimum source-backed deck legality
needed before semantic/combo work.

- `M35-A1`: Archetype priority ranking. **Done as M34-03.**
- `M35-A2`: First target slice selection + format policy + taxonomy gap report. **Done.**
  - Choose one target group/era from M34-03 ranking and current user priority.
  - Record whether the slice is `Classic Core`, `Standard D/DZ`, `V`, `G`, or
    `Premium`.
  - Pull only the needed rules/taxonomy rows from
    `outputs/research_2026_06_29_new_chat/`.
  - Output a small gap report: required zones, phases, actions, trigger policy,
    deck limits, and unsupported modules.
  - Selected `Classic Core / โนว่า เกรปเปอร์`.
  - Outputs:
    `outputs/target_slice/m35_a2_first_target_slice_report.json` and
    `outputs/target_slice/m35_a2_first_target_slice_report.md`.
  - Closeout: `docs/history/M35_A2_FIRST_TARGET_SLICE_CLOSEOUT.md`.
- `M35-A3`: Minimal deck legality fixtures for selected slice. **Done.**
  - Add small pass/fail fixture decklists for the selected slice.
  - Cover trigger count, copy limits, identity rule, and any selected-format
    special limits needed by the first slice.
  - Outputs:
    `outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.json` and
    `outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.md`.
  - Closeout:
    `docs/history/M35_A3_FIRST_SLICE_DECK_LEGALITY_FIXTURES_CLOSEOUT.md`.
- `M35-A4`: First-slice feasibility report refresh. **Done.**
  - Re-run or extend offline reports so the selected slice shows
    `capacity + legality-readiness + missing-rule-gate`.
  - Outputs:
    `outputs/target_slice/m35_a4_first_slice_feasibility_refresh.json` and
    `outputs/target_slice/m35_a4_first_slice_feasibility_refresh.md`.
  - Closeout:
    `docs/history/M35_A4_FIRST_SLICE_FEASIBILITY_REFRESH_CLOSEOUT.md`.

### Phase B: Semantic Slice

Goal: tag only the selected slice enough to reason about combo lines.

- `M35-B1`: Semantic vocabulary for selected slice. **Done.**
  - Start from ability type, zone, timing, condition, cost, effect, duration,
    and mechanic group.
  - Use rule taxonomy as allowed vocabulary, not as a full runtime import.
  - Outputs:
    `outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.json` and
    `outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.md`.
  - Closeout:
    `docs/history/M35_B1_FIRST_SLICE_SEMANTIC_VOCABULARY_CLOSEOUT.md`.
- `M35-B2`: Offline semantic extractor for selected slice. **Done.**
  - Parse local KK/runtime card text into advisory tags only.
  - No live match parsing and no runtime effect execution.
  - Outputs:
    `outputs/target_slice/m35_b2_first_slice_semantic_tags.json` and
    `outputs/target_slice/m35_b2_first_slice_semantic_tags.md`.
  - Closeout:
    `docs/history/M35_B2_FIRST_SLICE_SEMANTIC_EXTRACTOR_CLOSEOUT.md`.
- `M35-B3`: Requirement/provider model for selected slice. **Done.**
  - Examples: requires `CB1`, provides `CounterCharge`, requires `soul >= 3`,
    provides `SoulCharge`, requires `on_attack`, provides `multi_attack`.
  - Outputs:
    `outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json`
    and
    `outputs/target_slice/m35_b3_first_slice_requirement_provider_model.md`.
  - Closeout:
    `docs/history/M35_B3_FIRST_SLICE_REQUIREMENT_PROVIDER_MODEL_CLOSEOUT.md`.
- `M35-B4`: Manual review queue for unknown/low-confidence cards. **Done.**
  - Export unknown timing/cost/target cards before they can become playbook
    inputs.
  - Outputs:
    `outputs/target_slice/m35_b4_first_slice_manual_review_queue.json`,
    `outputs/target_slice/m35_b4_first_slice_manual_review_queue.csv`, and
    `outputs/target_slice/m35_b4_first_slice_manual_review_queue.md`.
  - Closeout:
    `docs/history/M35_B4_FIRST_SLICE_MANUAL_REVIEW_QUEUE_CLOSEOUT.md`.

### Phase C: Compatibility Slice

Goal: prove whether candidate pairs/packages are actually compatible enough to
be deck-building candidates.

- `M35-C1`: Pair compatibility graph for selected slice. **Done.**
  - Connect provider cards to consumer cards using M35-B3 requirements/providers.
  - Carry M35-B4 manual-review gating forward.
  - Outputs:
    `outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json`
    and
    `outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.md`.
  - Closeout:
    `docs/history/M35_C1_FIRST_SLICE_PAIR_COMPATIBILITY_GRAPH_CLOSEOUT.md`.
- `M35-C2`: Resource conflict detector. **Done.**
  - Detect CB/SB/EB/discard/retire pressure and missing recovery.
  - Outputs:
    `outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json`
    and
    `outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.md`.
  - Closeout:
    `docs/history/M35_C2_FIRST_SLICE_RESOURCE_CONFLICT_DETECTOR_CLOSEOUT.md`.
- `M35-C3`: Timing compatibility detector. **Done.**
  - Check provider timing can occur before consumer timing.
  - Outputs:
    `outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.json`
    and
    `outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.md`.
  - Closeout:
    `docs/history/M35_C3_FIRST_SLICE_TIMING_COMPATIBILITY_DETECTOR_CLOSEOUT.md`.
- `M35-C4`: Zone/target compatibility detector. **Done.**
  - Check VC/RC/GC/hand/drop/soul/bind/deck/order requirements and board slot
    conflicts.
  - Outputs:
    `outputs/target_slice/m35_c4_first_slice_zone_target_detector.json`
    and
    `outputs/target_slice/m35_c4_first_slice_zone_target_detector.md`.
  - Closeout:
    `docs/history/M35_C4_FIRST_SLICE_ZONE_TARGET_DETECTOR_CLOSEOUT.md`.
- `M35-C5`: Selected-slice compatibility output. **Done.**
  - Export `synergy`, `conflict`, `missing_data`, and
    `manual_review_required` reasons.
  - Outputs:
    `outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json`
    and
    `outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.md`.
  - Closeout:
    `docs/history/M35_C5_FIRST_SLICE_SELECTED_COMPATIBILITY_OUTPUT_CLOSEOUT.md`.

### Phase D: Deck Skeleton + Safe Playbook Seed

Goal: produce a human-reviewable deck skeleton and only then create safe
playbook seed data.

- `M35-D1`: Candidate package selection from compatibility output. **Done.**
  - Outputs:
    `outputs/target_slice/m35_d1_first_slice_candidate_packages.json`
    and
    `outputs/target_slice/m35_d1_first_slice_candidate_packages.md`.
  - Closeout:
    `docs/history/M35_D1_FIRST_SLICE_CANDIDATE_PACKAGES_CLOSEOUT.md`.
- `M35-D2`: Deck skeleton ratio planner. **Done.**
  - Include grade curve, trigger package, key cards, support cards, resource
    recovery, guard/shield profile, and known missing data.
  - Outputs:
    `outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json`
    and
    `outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.md`.
  - Closeout:
    `docs/history/M35_D2_FIRST_SLICE_DECK_SKELETON_RATIO_PLANNER_CLOSEOUT.md`.
- `M35-D3`: Combo line explainer. **Done.**
  - Explain why the package is included and what it needs to work.
  - Outputs:
    `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
    and
    `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.md`.
  - Closeout:
    `docs/history/M35_D3_FIRST_SLICE_COMBO_LINE_EXPLAINER_CLOSEOUT.md`.
- `M35-D4`: Reviewed playbook seed export. **Done.**
  - Export advisory playbook seed only after human/AI review.
  - Do not auto-inject into player decks or live bot runtime.
  - Outputs:
    `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json`
    and
    `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.md`.
  - Closeout:
    `docs/history/M35_D4_FIRST_SLICE_REVIEWED_PLAYBOOK_SEED_CLOSEOUT.md`.

### Phase E: Scale Out

Goal: expand only after one vertical slice proves the process.

- `M35-E1`: Second slice selection. **Done.**
  - Choose next slice by M34-03 priority and user/team need.
  - Selected `Classic Core / Oracle Think Tank` after excluding the closed
    `Classic Core / Nova Grappler` first slice.
  - Outputs:
    `outputs/target_slice/m35_e1_second_target_slice_report.json`
    and
    `outputs/target_slice/m35_e1_second_target_slice_report.md`.
  - Closeout:
    `docs/history/M35_E1_SECOND_TARGET_SLICE_CLOSEOUT.md`.
- `M35-E2`: Second-slice fixture/format readiness check. **Done.**
  - Create or refresh source-backed fixtures for `Classic Core / Oracle Think
    Tank`.
  - Decide whether Classic Core policy can be reused as-is or whether new
    format/mechanic fixtures are required before semantic scale-out.
  - Outputs:
    `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
    and
    `outputs/target_slice/m35_e2_second_slice_fixture_readiness.md`.
  - Closeout:
    `docs/history/M35_E2_SECOND_SLICE_FIXTURE_READINESS_CLOSEOUT.md`.
- `M35-E3`: Generalize semantic/compatibility tools. **Done.**
  - Generalize only after two slices expose repeated patterns.
  - Ran the B1-B4 semantic pipeline and C1-C5 compatibility pipeline for
    `Classic Core / Oracle Think Tank` through injected selected-report data.
  - Outputs:
    `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
    and
    `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.md`.
  - Closeout:
    `docs/history/M35_E3_SELECTED_SLICE_SEMANTIC_COMPATIBILITY_PROBE_CLOSEOUT.md`.
- `M35-E4`: Bot integration gate. **Done.**
  - Bot may use reviewed playbook hints only through legal actions, masked
    state, and tested structured ability templates.
  - Runtime bot integration remains disabled.
  - Outputs: `outputs/target_slice/m35_e4_bot_integration_gate.json` and
    `outputs/target_slice/m35_e4_bot_integration_gate.md`.
  - Closeout:
    `docs/history/M35_E4_BOT_INTEGRATION_GATE_CLOSEOUT.md`.
- `M35-closeout`: Hybrid Vertical-Slice closeout / next queue selection.
  **Done.**
  - Outputs: `outputs/target_slice/m35_closeout_hybrid_vertical_slice.json`
    and `outputs/target_slice/m35_closeout_hybrid_vertical_slice.md`.
  - Closeout: `docs/history/M35_HYBRID_VERTICAL_SLICE_CLOSEOUT.md`.

## M36: Human-review-assisted Deck Recipe Validation

Goal: turn the M35 advisory deck/combo outputs into reviewable deck recipe
drafts before any runtime or bot work resumes.

Hard gates:

- No runtime bot wiring.
- No live card text parsing.
- No direct `GameState` mutation.
- No automatic deck injection.
- Human review is required before playbook/runtime promotion.

- `M36-01`: First-slice review packet. **Done.**
  - Create a human-review packet for rejected M35-D4 lines, M35-B4 manual
    review cards, and the one accepted seed.
  - Outputs: `outputs/target_slice/m36_01_first_slice_review_packet.json`,
    `outputs/target_slice/m36_01_first_slice_review_packet.md`, and
    `outputs/target_slice/m36_01_first_slice_review_packet.csv`.
  - Closeout:
    `docs/history/M36_01_FIRST_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M36-02`: Deck recipe draft model. **Done.**
  - Convert advisory skeletons into explicit draft recipes with card
    quantities and validation metadata.
  - Outputs: `outputs/target_slice/m36_02_deck_recipe_draft_model.json` and
    `outputs/target_slice/m36_02_deck_recipe_draft_model.md`.
  - Closeout:
    `docs/history/M36_02_DECK_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M36-03`: Deck recipe validator. **Done.**
  - Validate clan/format, main/trigger/grade counts, ride deck constraints
    where applicable, and missing-card paths.
  - Outputs:
    `outputs/target_slice/m36_03_deck_recipe_validation_report.json` and
    `outputs/target_slice/m36_03_deck_recipe_validation_report.md`.
  - Closeout:
    `docs/history/M36_03_DECK_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M36-04`: Combo-line to recipe consistency check. **Done.**
  - Confirm selected combo lines are present in the draft recipe and do not
    rely on blocked/manual-review cards.
  - Outputs:
    `outputs/target_slice/m36_04_combo_recipe_consistency_report.json` and
    `outputs/target_slice/m36_04_combo_recipe_consistency_report.md`.
  - Closeout:
    `docs/history/M36_04_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M36-05`: Second-slice readiness comparison. **Done.**
  - Compare Oracle Think Tank probe outputs against the first-slice pipeline
    before broader scale-out.
  - Outputs:
    `outputs/target_slice/m36_05_second_slice_readiness_comparison.json` and
    `outputs/target_slice/m36_05_second_slice_readiness_comparison.md`.
  - Closeout:
    `docs/history/M36_05_SECOND_SLICE_READINESS_COMPARISON_CLOSEOUT.md`.
- `M36-closeout`: Deck recipe validation closeout. **Done.**
  - Close M36 and decide the next queue from validation evidence.
  - Results: `0` runtime-ready recipes, `0` promotable combo lines, `1`
    invalid draft, `24` blocked-by-review recipes, `16` slot-gap recipes, and
    `12` trigger-count mismatch recipes.
  - Outputs:
    `outputs/target_slice/m36_closeout_deck_recipe_validation.json` and
    `outputs/target_slice/m36_closeout_deck_recipe_validation.md`.
  - Closeout: `docs/history/M36_DECK_RECIPE_VALIDATION_CLOSEOUT.md`.

## M37: First-slice Blocker Resolution and Recipe Repair

Goal: repair the first selected slice recipe blockers before any runtime deck,
bot playbook, or broader slice scale-out work resumes.

Hard gates:

- No runtime deck promotion until validator pass and human acceptance are both
  true.
- No bot/playbook promotion until combo consistency reports
  `promotion_allowed=true`.
- No automatic fill from raw card text without reviewable source evidence.
- No direct `GameState` mutation.
- No hidden-state or private deck leakage.

- `M37-01`: Accepted seed slot-gap completion candidates. **Done.**
  - Suggest source-backed candidates for missing accepted-seed recipe slots
    without auto-promoting them.
  - Results: accepted seed `recipe_003` has `12` trigger slots unfilled,
    `18` source-backed trigger candidate cards, and `5` advisory completion
    packages. Runtime promotion remains disabled.
  - Outputs:
    `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.json` and
    `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.md`.
  - Closeout:
    `docs/history/M37_01_ACCEPTED_SEED_SLOT_GAP_CANDIDATES_CLOSEOUT.md`.
- `M37-02`: Trigger package repair proposal. **Done.**
  - Repair trigger-count mismatch candidates for the accepted seed recipe.
  - Results: recommended advisory package `m37_01_pkg_001` /
    `balanced_classic`, resolving `main_deck_size_mismatch`,
    `trigger_count_mismatch`, and `unfilled_slots` while leaving grade profile
    review and human acceptance pending. Runtime promotion remains disabled.
  - Outputs:
    `outputs/target_slice/m37_02_trigger_package_repair_proposal.json` and
    `outputs/target_slice/m37_02_trigger_package_repair_proposal.md`.
  - Closeout:
    `docs/history/M37_02_TRIGGER_PACKAGE_REPAIR_PROPOSAL_CLOSEOUT.md`.
- `M37-03`: Rejected-line support-gap triage. **Done.**
  - Group blocked combo lines by unsupported semantic or review reason.
  - Results: `24` rejected lines classified into `5` support-gap groups:
    resource pressure (`9`), zone access (`15`), broad timing review (`10`),
    detector-gap manual review (`10`), and no-resource-dependency review (`5`).
  - Outputs:
    `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json` and
    `outputs/target_slice/m37_03_rejected_line_support_gap_triage.md`.
  - Closeout:
    `docs/history/M37_03_REJECTED_LINE_SUPPORT_GAP_TRIAGE_CLOSEOUT.md`.
- `M37-04`: Manual semantic mapping candidates. **Done.**
  - Create reviewable mappings for unsupported effects such as bounce-to-hand
    style interactions.
  - Results: `5` non-executable mapping candidates generated from `49`
    triage line links. No ability schema or recipe draft was modified.
  - Outputs:
    `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json` and
    `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.md`.
  - Closeout:
    `docs/history/M37_04_MANUAL_SEMANTIC_MAPPING_CANDIDATES_CLOSEOUT.md`.
- `M37-05`: Revised recipe validation rerun. **Done.**
  - Re-run recipe validator and consistency checks after accepted repairs are
    documented.
  - Results: in-memory trigger repair clears accepted seed blockers and changes
    recipe `recipe_003` to `validator_passed_pending_human_acceptance` /
    `consistent_pending_human_acceptance`. Runtime promotion remains disabled.
  - Outputs:
    `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json` and
    `outputs/target_slice/m37_05_revised_recipe_validation_rerun.md`.
  - Closeout:
    `docs/history/M37_05_REVISED_RECIPE_VALIDATION_RERUN_CLOSEOUT.md`.
- `M37-closeout`: First runtime-ready recipe decision. **Done.**
  - Decide whether a recipe can become a runtime/test fixture or remains
    advisory only.
  - Decision: `recipe_003` remains advisory; no runtime-ready recipe is
    available because `human_acceptance_pending`, `grade_profile_review`, and
    `promotion_not_allowed` remain.
  - Outputs:
    `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.json`
    and
    `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.md`.
  - Closeout:
    `docs/history/M37_FIRST_RUNTIME_READY_RECIPE_DECISION_CLOSEOUT.md`.

## M38: Human Acceptance and Grade-profile Repair Gate

Goal: turn the repaired accepted seed recipe into an explicitly reviewed
artifact before any runtime fixture promotion.

Hard gates:

- No runtime deck promotion without explicit human acceptance.
- No runtime deck promotion while `grade_profile_review` remains open.
- No bot/playbook promotion until runtime fixture gate passes.
- No automatic mutation of M36 recipe draft artifacts.
- No live card text parsing.

- `M38-01`: Accepted seed human review packet. **Done.**
  - Export a concise review packet for `recipe_003` and the recommended trigger
    repair.
  - Results: review packet exported with `1` item, `3` quantity delta cards,
    `2` unresolved review codes, and `3` decision options. The packet does not
    record acceptance.
  - Outputs:
    `outputs/target_slice/m38_01_accepted_seed_human_review_packet.json`,
    `outputs/target_slice/m38_01_accepted_seed_human_review_packet.md`, and
    `outputs/target_slice/m38_01_accepted_seed_human_review_packet.csv`.
  - Closeout:
    `docs/history/M38_01_ACCEPTED_SEED_HUMAN_REVIEW_PACKET_CLOSEOUT.md`.
- `M38-02`: Grade profile repair candidates. **Done.**
  - Propose reviewable grade-profile adjustments without mutating runtime
    decks.
  - Results: `2` complete substitution-preview candidates can reach
    `G0=17/G1=14/G2=11/G3=8` by adding `20` cards and removing `20` G3 cards.
    Runtime promotion remains disabled.
  - Outputs:
    `outputs/target_slice/m38_02_grade_profile_repair_candidates.json` and
    `outputs/target_slice/m38_02_grade_profile_repair_candidates.md`.
  - Closeout:
    `docs/history/M38_02_GRADE_PROFILE_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M38-03`: Human-accepted recipe artifact. **Done.**
  - Record explicit acceptance or rejection of the repaired recipe.
  - Results: accepted `m38_02_grade_pkg_001` plus trigger repair
    `m37_01_pkg_001`; `recipe_003` now has `50` cards, `16` triggers,
    grade profile `G0=17/G1=14/G2=11/G3=8`, `0` blockers, and remains out of
    runtime until M38-04.
  - Outputs:
    `outputs/target_slice/m38_03_human_accepted_recipe_artifact.json` and
    `outputs/target_slice/m38_03_human_accepted_recipe_artifact.md`.
  - Closeout:
    `docs/history/M38_03_HUMAN_ACCEPTED_RECIPE_ARTIFACT_CLOSEOUT.md`.
- `M38-04`: Runtime fixture promotion gate. **Done.**
  - Promote only if validation, consistency, grade review, and human acceptance
    all pass.
  - Results: all `5` gate checks passed, promotion is allowed for an offline
    runtime/test fixture artifact, and no saved deck/UI/bot/GameState mutation
    occurred.
  - Outputs:
    `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.json`,
    `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.md`, and
    `outputs/target_slice/runtime_fixtures/recipe_003_classic_core_nova_grappler_m38_04.json`.
  - Closeout:
    `docs/history/M38_04_RUNTIME_FIXTURE_PROMOTION_GATE_CLOSEOUT.md`.
- `M38-closeout`: First runtime fixture closeout. **Done.**
  - Decide whether the first recipe enters runtime/test-fixture scope or
    remains advisory.
  - Results: `recipe_003` enters offline runtime/test fixture scope only;
    live runtime deck UI and bot playbooks remain disabled. Next queue is
    `M39`.
  - Outputs:
    `outputs/target_slice/m38_closeout_first_runtime_fixture.json` and
    `outputs/target_slice/m38_closeout_first_runtime_fixture.md`.
  - Closeout:
    `docs/history/M38_FIRST_RUNTIME_FIXTURE_CLOSEOUT.md`.

## M39: Fixture Consumption and Second-Slice Scale Gate

Goal: validate how the first fixture can be consumed safely, then decide
whether to scale recipe work to the second slice.

Hard gates:

- Do not inject fixtures into saved player decks.
- Do not expose fixtures in live UI deck lists without a user/team review gate.
- Do not enable bot playbooks from fixture data.
- Do not mutate `GameState`.
- Keep M39 outputs offline and reviewable unless a later gate says otherwise.

- `M39-01`: Offline fixture schema validator. **Done.**
  - Validate runtime fixture artifacts independently from the M38 generator.
  - Results: fixture schema is valid with `0` blockers, `50` cards, `17`
    unique cards, trigger profile `Critical=4/Draw=4/Heal=4/Stand=4`, and
    grade profile `G0=17/G1=14/G2=11/G3=8`.
  - Outputs:
    `outputs/target_slice/m39_01_offline_fixture_schema_validation.json` and
    `outputs/target_slice/m39_01_offline_fixture_schema_validation.md`.
  - Closeout:
    `docs/history/M39_01_OFFLINE_FIXTURE_SCHEMA_VALIDATOR_CLOSEOUT.md`.
- `M39-02`: Fixture-to-deck text exporter. **Done.**
  - Export the fixture as reviewable count-line deck text without adding it to
    saved decks.
  - Results: reviewable count-line deck text generated with `17` importable
    card lines, `50` total cards, pack version `251`, and current pack
    definition hash. No saved deck/UI/bot/GameState mutation occurred.
  - Outputs:
    `outputs/target_slice/m39_02_fixture_deck_text_export.txt`,
    `outputs/target_slice/m39_02_fixture_deck_text_export.json`, and
    `outputs/target_slice/m39_02_fixture_deck_text_export.md`.
  - Closeout:
    `docs/history/M39_02_FIXTURE_DECK_TEXT_EXPORT_CLOSEOUT.md`.
- `M39-03`: Headless fixture load smoke. **Done.**
  - Load the fixture through offline tooling/headless paths without UI or bot
    mutation.
  - Results: count-line deck text parsed back into a deterministic `VGTH1.`
    deck code artifact, Unity headless accepted it with `deck_source=deck_code`,
    `actions_executed=4`, and `event_count=4`. No saved deck/UI/bot/GameState
    promotion occurred.
  - Outputs:
    `outputs/target_slice/m39_03_headless_fixture_deck_code.txt`,
    `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`,
    `outputs/target_slice/m39_03_headless_fixture_load_smoke.md`,
    `outputs/target_slice/m39_03_headless_fixture_unity_result.json`, and
    `outputs/target_slice/m39_03_headless_fixture_unity_replay.json`.
  - Closeout:
    `docs/history/M39_03_HEADLESS_FIXTURE_LOAD_SMOKE_CLOSEOUT.md`.
- `M39-04`: Second-slice recipe scale decision. **Done.**
  - Decide whether Oracle Think Tank moves into the same recipe repair
    pipeline.
  - Results: offline recipe pipeline is allowed for Classic Core / Oracle
    Think Tank. Runtime deck promotion, saved deck/UI publication, and
    bot/playbook promotion remain blocked.
  - Outputs:
    `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.json` and
    `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.md`.
  - Closeout:
    `docs/history/M39_04_SECOND_SLICE_RECIPE_SCALE_DECISION_CLOSEOUT.md`.

## M40: Second-slice Offline Recipe Pipeline

Goal: run the same review-first recipe pipeline for `Classic Core / Oracle
Think Tank` without promoting anything to saved decks, UI deck lists, runtime
fixtures, or bot playbooks.

Hard gates:

- Offline artifacts only until a later human-acceptance/runtime gate.
- No saved deck injection.
- No UI deck-list publication.
- No runtime deck promotion.
- No bot/playbook promotion.
- No live card text parsing.

- `M40-01`: Second-slice review packet. **Done.**
  - Export Oracle Think Tank candidate edges, manual-review cards, and fixture
    notes for review.
  - Results: `6` fixture notes, `7` manual-review cards, `259` candidate
    edges, `272` total review items, and `ready_for_m40_02=True`.
  - Outputs:
    `outputs/target_slice/m40_01_second_slice_review_packet.json`,
    `outputs/target_slice/m40_01_second_slice_review_packet.md`, and
    `outputs/target_slice/m40_01_second_slice_review_packet.csv`.
  - Closeout:
    `docs/history/M40_01_SECOND_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M40-02`: Second-slice recipe draft model. **Done.**
  - Create advisory recipe drafts only; no saved deck injection.
  - Results: `25` pair-anchored, fixture-scaffolded advisory recipe drafts,
    all quantity-complete at `50` cards with `16` triggers.
  - Outputs:
    `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json` and
    `outputs/target_slice/m40_02_second_slice_recipe_draft_model.md`.
  - Closeout:
    `docs/history/M40_02_SECOND_SLICE_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M40-03`: Second-slice recipe validator. **Done.**
  - Validate count, trigger, grade, clan identity, copy limits, and missing
    cards.
  - Results: `25` drafts validated, `0` missing-card recipes, `0` copy-limit
    violations, `0` slot-gap recipes, `0` trigger-count mismatch recipes,
    `25` manual-review overlap blockers, and `0` runtime-ready recipes.
  - Outputs:
    `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`
    and
    `outputs/target_slice/m40_03_second_slice_recipe_validation_report.md`.
  - Closeout:
    `docs/history/M40_03_SECOND_SLICE_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M40-04`: Second-slice combo-to-recipe consistency. **Done.**
  - Check selected combo lines are present and not blocked by manual review.
  - Results: `25` consistency checks, `25` drafts with candidate pair cards
    present, `0` missing pair-card checks, `25` recipe-level manual-review
    dependencies, and `0` promotion-allowed checks.
  - Outputs:
    `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`
    and
    `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.md`.
  - Closeout:
    `docs/history/M40_04_SECOND_SLICE_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M40-05`: Second-slice blocker repair candidates. **Done.**
  - Generate source-backed repair candidates for blocked recipes.
  - Results: `25` repair items, `25` grade-profile repair candidates, `25`
    grade packages that clear manual overlap, and `25` items ready for human
    repair review. Runtime promotion remains blocked.
  - Outputs:
    `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.json`
    and
    `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.md`.
  - Closeout:
    `docs/history/M40_05_SECOND_SLICE_BLOCKER_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M40-closeout`: Second-slice runtime readiness decision. **Done.**
  - Decide whether any recipe can later enter a human-acceptance/runtime
    fixture gate.
  - Results: `m40_complete=True`, `0` runtime-ready recipes, `0`
    promotion-allowed checks, `25` manual-review overlap recipes, and `25`
    repair candidates ready for human review.
  - Decision: second slice remains advisory; saved deck/UI/runtime/bot
    promotion stays disabled; next queue is `M41`.
  - Outputs:
    `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.json`
    and
    `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.md`.
  - Closeout:
    `docs/history/M40_SECOND_SLICE_RUNTIME_READINESS_CLOSEOUT.md`.

## M41: Second-slice Human Repair Review Gate

Goal: review the M40-05 repair candidates, record explicit human acceptance or
rejection, rerun validation, and only then decide whether an Oracle Think Tank
recipe may enter offline fixture scope.

Hard gates:

- No runtime fixture until human acceptance and validation both pass.
- No saved deck injection.
- No UI deck-list publication.
- No bot/playbook promotion.
- No mutation of M40-02 draft artifacts.

- `M41-01`: Second-slice human repair review packet. **Done.**
  - Export a concise packet for human/team review of M40-05 repair packages.
  - Results: `25` review items, `25` ready-for-review repair candidates, `25`
    complete grade-profile candidates, and `ready_for_m41_02=True`.
  - Outputs:
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.json`,
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.md`,
    and
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.csv`.
  - Closeout:
    `docs/history/M41_01_SECOND_SLICE_HUMAN_REPAIR_REVIEW_PACKET_CLOSEOUT.md`.
- `M41-02`: Second-slice human-accepted repair artifact. **Done.**
  - Record explicit acceptance or rejection of one repaired Oracle Think Tank
    recipe.
  - Results: accepted review item `m41_01_m40_recipe_001_repair_review`,
    recipe `m40_recipe_001`, repair package
    `m40_recipe_001_grade_profile_pkg_001`, `50` cards after repair, and `0`
    repair-application issues.
  - Boundary: does not declare the recipe valid and does not promote runtime.
  - Outputs:
    `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
    and
    `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.md`.
  - Closeout:
    `docs/history/M41_02_SECOND_SLICE_HUMAN_ACCEPTED_REPAIR_ARTIFACT_CLOSEOUT.md`.
- `M41-03`: Second-slice repaired recipe validation rerun. **Done.**
  - Apply accepted repair in memory and rerun count, trigger, grade,
    copy-limit, clan, and manual-overlap validation.
  - Results: `invalid_repaired_recipe`, main deck count `50`, trigger count
    `2/16`, grade counts `G0=17/G1=14/G2=11/G3=8`, manual-review overlap
    cleared, `ready_for_m41_04=False`.
  - Decision: do not enter M41-04; route to `M41-repair`.
  - Outputs:
    `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.json`
    and
    `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.md`.
  - Closeout:
    `docs/history/M41_03_SECOND_SLICE_REPAIRED_RECIPE_VALIDATION_RERUN_CLOSEOUT.md`.
- `M41-repair`: Second-slice trigger/profile repair loop. **Done.**
  - Repair the accepted recipe's trigger profile before any promotion gate.
  - Results: `3` complete repair candidates; balanced package
    `m41_repair_pkg_001` restores trigger count to `16/16` while preserving
    `G0=17/G1=14/G2=11/G3=8`.
  - Outputs:
    `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.json`
    and
    `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.md`.
  - Closeout:
    `docs/history/M41_REPAIR_SECOND_SLICE_TRIGGER_PROFILE_CANDIDATES_CLOSEOUT.md`.
- `M41-repair-accept`: Second-slice trigger repair acceptance artifact. **Done.**
  - Record acceptance of one trigger/profile repair package before rerunning
    validation.
  - Results: accepted `m41_repair_pkg_001` /
    `balanced_classic_trigger_restore`; this does not declare the recipe valid
    or enable runtime promotion.
  - Outputs:
    `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.json`
    and
    `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.md`.
  - Closeout:
    `docs/history/M41_REPAIR_ACCEPT_SECOND_SLICE_TRIGGER_REPAIR_ARTIFACT_CLOSEOUT.md`.
- `M41-repair-validate`: Second-slice repaired recipe validation after trigger
  repair. **Done.**
  - Rerun validation after applying the accepted trigger repair.
  - Results: `validator_passed`, blockers `0`, main deck `50`, trigger count
    `16`, grade profile `G0=17/G1=14/G2=11/G3=8`, and
    `ready_for_m41_04=True`.
  - Outputs:
    `outputs/target_slice/m41_repair_validate_second_slice_repaired_recipe.json`
    and
    `outputs/target_slice/m41_repair_validate_second_slice_repaired_recipe.md`.
  - Closeout:
    `docs/history/M41_REPAIR_VALIDATE_SECOND_SLICE_REPAIRED_RECIPE_CLOSEOUT.md`.
- `M41-04`: Second-slice runtime fixture promotion gate. **Done.**
  - Promote only if repaired validation, consistency, and human acceptance all
    pass.
  - Results: `promotion_allowed=True`, passed checks `6`, failed checks `0`,
    and fixture created at
    `outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json`.
  - Closeout:
    `docs/history/M41_04_SECOND_SLICE_RUNTIME_FIXTURE_PROMOTION_GATE_CLOSEOUT.md`.
- `M41-closeout`: Second-slice fixture closeout. **Done.**
  - Decide whether Oracle Think Tank enters offline fixture scope or remains
    advisory.
  - Results: M41 complete, second runtime fixture available, live runtime deck,
    saved deck, UI deck list, and bot playbook remain disabled.
  - Outputs:
    `outputs/target_slice/m41_closeout_second_slice_fixture.json`
    and
    `outputs/target_slice/m41_closeout_second_slice_fixture.md`.
  - Closeout:
    `docs/history/M41_SECOND_SLICE_FIXTURE_CLOSEOUT.md`.

## M42: Second Fixture Consumption and Third-Slice Scale Gate — Active

- `M42-01`: Second fixture schema validator. **Done.**
  - Validate the Oracle Think Tank runtime fixture independently from the M41
    generator.
  - Results: schema valid, blockers `0`, main deck `50`, unique cards `15`,
    trigger profile `Critical=4/Draw=4/Heal=4/Stand=4`, and ready for `M42-02`.
  - Closeout:
    `docs/history/M42_01_SECOND_FIXTURE_SCHEMA_VALIDATOR_CLOSEOUT.md`.
- `M42-02`: Second fixture deck text exporter. **Done.**
  - Export the Oracle Think Tank fixture as reviewable count-line deck text
    without adding it to saved decks.
  - Results: export ready, blockers `0`, card lines `15`, and ready for
    `M42-03`.
  - Closeout:
    `docs/history/M42_02_SECOND_FIXTURE_DECK_TEXT_EXPORT_CLOSEOUT.md`.
- `M42-03`: Second fixture headless load smoke. **Done.**
  - Load the Oracle Think Tank fixture through offline/headless paths without UI
    or bot mutation.
  - Results: offline ready, Unity headless smoke passed, deck_source=`deck_code`,
    actions `4`, events `4`, and ready for `M42-04`.
  - Closeout:
    `docs/history/M42_03_SECOND_HEADLESS_FIXTURE_LOAD_SMOKE_CLOSEOUT.md`.
- `M42-04`: Multi-fixture scale decision. **Done.**
  - Review Nova Grappler and Oracle Think Tank fixture evidence before selecting
    any third slice.
  - Results: fixture evidence `2`, passed fixtures `2`, candidate queue `5`,
    third-slice offline pipeline allowed, no live/UI/saved-deck/bot promotion.
  - Closeout:
    `docs/history/M42_04_MULTI_FIXTURE_SCALE_DECISION_CLOSEOUT.md`.

## M43: Third-Slice Offline Pipeline Entry — Active

- `M43-01`: Third target slice selection. **Done.**
  - Select one third slice for offline analysis from the M42-04 candidate queue.
  - Results: selected `เบอร์มิวด้า ไทรแองเกิล` / `link_joker_legion_mate` for
    offline analysis only.
  - Closeout:
    `docs/history/M43_01_THIRD_TARGET_SLICE_SELECTION_CLOSEOUT.md`.
- `M43-02`: Third-slice fixture/format readiness. **Done.**
  - Confirm source-backed fixtures and format policy before semantic work.
  - Results: `127` source-backed cards, grade 0-3 coverage, trigger capacity
    `84`, EB06/EB10 source coverage, and ready for `M43-03`.
  - Closeout:
    `docs/history/M43_02_THIRD_SLICE_FIXTURE_READINESS_CLOSEOUT.md`.
- `M43-03`: Third-slice semantic/compatibility probe. **Done.**
  - Run semantic and compatibility tooling only after readiness passes.
  - Results: `127` semantic cards, `61` manual-review cards, `4835` pair graph
    edges, `109` candidate edges, and ready for `M43-04`.
  - Closeout:
    `docs/history/M43_03_THIRD_SLICE_SEMANTIC_COMPATIBILITY_PROBE_CLOSEOUT.md`.
- `M43-04`: Third-slice recipe pipeline entry gate. **Done.**
  - Decide whether the selected third slice can enter recipe draft/repair work.
  - Results: blockers `0`, offline recipe pipeline allowed, fixture scaffold
    required before recipe validation, and ready for `M44-01`.
  - Closeout:
    `docs/history/M43_04_THIRD_SLICE_RECIPE_PIPELINE_ENTRY_GATE_CLOSEOUT.md`.

## M44: Third-Slice Offline Recipe Pipeline — Active

- `M44-01`: Third-slice fixture scaffold. **Done.**
  - Define source-backed fixture policy for the Link Joker/Legion Mate slice
    before validator work.
  - Results: source-backed scaffold ready, blockers `0`, and ready for
    `M44-02`.
  - Closeout:
    `docs/history/M44_01_THIRD_SLICE_FIXTURE_SCAFFOLD_CLOSEOUT.md`.
- `M44-02`: Third-slice review packet. **Done.**
  - Results: `1` fixture scaffold item, `61` manual-review cards, `109`
    candidate edges, `171` total review items, and ready for `M44-03`.
  - Closeout:
    `docs/history/M44_02_THIRD_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M44-03`: Third-slice recipe draft model. **Done.**
  - Results: `25` quantity-complete advisory drafts, `50` cards per draft,
    `16` triggers per draft, `25` manual-review overlap blockers, and ready
    for `M44-04`.
  - Closeout:
    `docs/history/M44_03_THIRD_SLICE_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M44-04`: Third-slice recipe validator. **Done.**
  - Results: `25` drafts validated, `0` missing-card recipes, `0` copy-limit
    violations, `0` slot-gap recipes, `0` trigger-count mismatch recipes,
    `25` manual-review overlap blockers, and `0` runtime-ready recipes.
  - Closeout:
    `docs/history/M44_04_THIRD_SLICE_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M44-05`: Third-slice combo-to-recipe consistency. **Done.**
  - Results: `25` consistency checks, `25` drafts with candidate pair cards
    present, `0` missing pair-card checks, `25` recipe-level manual-review
    dependencies, and `0` promotion-allowed checks.
  - Closeout:
    `docs/history/M44_05_THIRD_SLICE_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M44-06`: Third-slice blocker repair candidates. **Done.**
  - Results: `25` repair items, `25` complete manual repair packages, `25`
    complete grade-profile repair packages, `0` grade packages that clear
    manual overlap, and `25` items ready for human repair review.
  - Closeout:
    `docs/history/M44_06_THIRD_SLICE_BLOCKER_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M44-closeout`: Third-slice runtime readiness decision. **Done.**
  - Results: `m44_complete=True`, `0` runtime-ready recipes, `0`
    promotion-allowed checks, `25` repair candidates ready for human review,
    and next queue `M45`.
  - Closeout:
    `docs/history/M44_THIRD_SLICE_RUNTIME_READINESS_CLOSEOUT.md`.

## M45: Third-slice Human Repair Review Gate - Active

Goal: review the M44-06 repair candidates, record explicit human acceptance or
rejection, rerun validation, and only then decide whether a third-slice recipe
may enter offline fixture scope.

Hard gates:

- No runtime fixture until human acceptance and validation both pass.
- No saved deck injection.
- No UI deck-list publication.
- No bot/playbook promotion.
- No mutation of M44-03 draft artifacts.

- `M45-01`: Third-slice human repair review packet. **Done.**
  - Results: `25` review items, `25` ready for human repair review, `25`
    complete manual repair packages, `25` complete grade-profile candidates,
    `3` decision options per item, and `ready_for_m45_02=True`.
  - Closeout:
    `docs/history/M45_01_THIRD_SLICE_HUMAN_REPAIR_REVIEW_PACKET_CLOSEOUT.md`.
- `M45-02`: Third-slice human-accepted repair artifact. **Done.**
  - Results: accepted `m45_01_m44_recipe_001_repair_review`, detected `2`
    source grade-package conflicts after manual substitution, recomputed the
    combined grade repair, produced a `50`-card preview with grade counts
    `0:17 / 1:14 / 2:11 / 3:8`, and `ready_for_m45_03=True`.
  - Closeout:
    `docs/history/M45_02_THIRD_SLICE_HUMAN_ACCEPTED_REPAIR_ARTIFACT_CLOSEOUT.md`.
- `M45-03`: Third-slice repaired recipe validation rerun. **Done.**
  - Results: validated accepted recipe `m44_recipe_001`, `validator_passed=1`,
    `runtime_ready=1`, no missing-card/copy-limit/slot-gap/trigger-count/
    manual-overlap/grade-profile issues, and `ready_for_m45_04=True`.
  - Closeout:
    `docs/history/M45_03_THIRD_SLICE_REPAIRED_RECIPE_VALIDATION_RERUN_CLOSEOUT.md`.
- `M45-04`: Third-slice runtime fixture promotion gate. **Done.**
  - Results: `promotion_allowed=True`, `7/7` gate checks passed, offline
    runtime/test fixture artifact created, and no saved deck/UI/bot/GameState
    mutation.
  - Fixture:
    `outputs/target_slice/runtime_fixtures/m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json`.
  - Closeout:
    `docs/history/M45_04_THIRD_SLICE_RUNTIME_FIXTURE_PROMOTION_GATE_CLOSEOUT.md`.
- `M45-closeout`: Third-slice fixture closeout. **Done.**
  - Results: `m45_complete=True`, third runtime fixture available, next queue
    `M46`, and live runtime deck UI / saved deck / UI deck list / bot playbook
    remain disabled.
  - Closeout:
    `docs/history/M45_THIRD_SLICE_FIXTURE_CLOSEOUT.md`.

## M46: Third Fixture Consumption and Multi-Fixture Scale Gate - Active

Goal: validate and consume the third offline runtime/test fixture safely before
any further scale decision.

Hard gates:

- No saved deck injection.
- No UI deck-list publication.
- No bot/playbook promotion.
- No mutation of fixture artifacts by validation/export/smoke tools.
- No direct `GameState` mutation.

- `M46-01`: Third fixture schema validator. **Done.**
  - Results: `schema_valid=True`, blockers `0`, main deck count `50`, trigger
    profile `Critical=4 / Draw=4 / Heal=4 / Stand=4`, grade profile
    `G0=17 / G1=14 / G2=11 / G3=8`, and no saved-deck/UI/bot/GameState
    mutation.
  - Closeout:
    `docs/history/M46_01_THIRD_FIXTURE_SCHEMA_VALIDATOR_CLOSEOUT.md`.
- `M46-02`: Third fixture deck text exporter. **Done.**
  - Results: `export_ready=True`, blockers `0`, `15` exported card lines,
    review-only count-line deck text generated, and no saved-deck/UI/bot/
    GameState mutation.
  - Closeout:
    `docs/history/M46_02_THIRD_FIXTURE_DECK_TEXT_EXPORT_CLOSEOUT.md`.
- `M46-03`: Third fixture headless load smoke. **Done.**
  - Results: `offline_load_ready=True`, `deck_code_created=True`,
    `unity_headless_smoke_passed=True`, `deck_source=deck_code`,
    `actions_executed=4`, `event_count=4`, blockers `0`, and no saved-deck/UI/
    bot/GameState mutation.
  - Closeout:
    `docs/history/M46_03_THIRD_HEADLESS_FIXTURE_LOAD_SMOKE_CLOSEOUT.md`.
- `M46-04`: Multi-fixture scale decision. **Done.**
  - Results: `ready_for_m47=True`, `passed_fixture_count=3`,
    `failed_fixture_count=0`, `candidate_count=5`, fourth-slice offline
    pipeline allowed, and no saved-deck/UI/bot/GameState mutation.
  - Closeout:
    `docs/history/M46_04_THREE_FIXTURE_SCALE_DECISION_CLOSEOUT.md`.

## M47: Fourth Offline Slice Selection - Active

Goal: select a fourth offline-only analysis slice after three fixtures have
passed schema, deck-text, and headless load smoke gates.

Hard gates:

- No saved deck injection.
- No UI deck-list publication.
- No bot/playbook promotion.
- No live runtime deck selection.
- No direct `GameState` mutation.

- `M47-01`: Fourth target slice selection. **Done.**
  - Results: selected `รอยัล พาลาดิน`, era preset `g_series_first`,
    offline analysis only, and no recipe/runtime/UI/saved-deck/bot/GameState
    mutation.
  - Closeout:
    `docs/history/M47_01_FOURTH_TARGET_SLICE_SELECTION_CLOSEOUT.md`.
- `M47-02`: Fourth-slice fixture/format readiness. **Done.**
  - Results: source-backed card count `71`, trigger gap `Heal`,
    `all_fixture_expectations_met=False`, `repair_required=True`, and no
    recipe/runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M47_02_FOURTH_SLICE_FIXTURE_READINESS_CLOSEOUT.md`.
- `M47-repair`: Fourth-slice readiness blocker repair. **Done.**
  - Results: same-group Heal triggers exist outside selected scope, recommended
    action `review_same_group_source_expansion`, no card data mutation, no
    recipe/runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M47_REPAIR_FOURTH_SLICE_READINESS_BLOCKERS_CLOSEOUT.md`.
- `M47-repair-expand-scope`: Same-group source expansion review. **Done.**
  - Results: recommended `g_era_heal_expansion`, source card count `190`,
    added series count `7`, trigger gaps cleared, no card data mutation, no
    recipe/runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M47_REPAIR_EXPAND_SCOPE_REVIEW_CLOSEOUT.md`.
- `M47-repair-apply-scope`: Apply reviewed source scope expansion. **Done.**
  - Results: applied `g_era_heal_expansion` to the offline fixture pipeline
    scope artifact, source card count `190`, blockers `0`, no card data
    mutation, no recipe/runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M47_REPAIR_APPLY_SCOPE_CLOSEOUT.md`.
- `M47-03`: Fourth-slice semantic/compatibility probe. **Done.**
  - Results: semantic cards `190`, manual-review cards `15`, pair graph edges
    `14150`, candidate edges `785`, all stage readiness flags passed, no card
    data mutation, no recipe/runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M47_03_FOURTH_SLICE_SEMANTIC_COMPATIBILITY_PROBE_CLOSEOUT.md`.
- `M47-04`: Fourth-slice recipe pipeline entry gate. **Done.**
  - Results: offline M48 recipe pipeline allowed, fixture scaffold required,
    blockers `0`, no card data mutation, no recipe/runtime/UI/saved-deck/bot/
    GameState mutation.
  - Closeout:
    `docs/history/M47_04_FOURTH_SLICE_RECIPE_PIPELINE_ENTRY_GATE_CLOSEOUT.md`.
- `M48-01`: Fourth-slice fixture scaffold. **Done.**
  - Results: scaffold ready, blockers `0`, Grade 4/G Zone support deferred as
    advisory/manual-review only, no card data mutation, no recipe/runtime/UI/
    saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_01_FOURTH_SLICE_FIXTURE_SCAFFOLD_CLOSEOUT.md`.
- `M48-02`: Fourth-slice review packet. **Done.**
  - Results: fixture scaffold items `1`, manual-review cards `15`, candidate
    edges `785`, total review items `801`, no card data mutation, no recipe/
    runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_02_FOURTH_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M48-03`: Fourth-slice recipe draft model. **Done.**
  - Results: recipe drafts `25`, quantity-complete drafts `25`, manual-overlap
    drafts `25`, skipped trigger/G4/missing candidate edges `35`, no card data
    mutation, no runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_03_FOURTH_SLICE_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M48-04`: Fourth-slice recipe validator. **Done.**
  - Results: validated drafts `25`, runtime-ready recipes `0`, manual-review
    blocked recipes `25`, Grade 4 main-deck violations `0`, no card data
    mutation, no runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_04_FOURTH_SLICE_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M48-05`: Fourth-slice combo-to-recipe consistency. **Done.**
  - Results: consistency checks `25`, pair cards present `25`, missing pair
    cards `0`, promotion allowed `0`, G Zone deferred checks `25`, no card data
    mutation, no runtime/UI/saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_05_FOURTH_SLICE_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M48-06`: Fourth-slice blocker repair candidates. **Done.**
  - Results: recipes reviewed `25`, complete manual repair candidates `25`,
    complete grade-profile candidates `24`, G Zone deferred recipes `25`,
    unexpected structural blockers `0`, no card data mutation, no runtime/UI/
    saved-deck/bot/GameState mutation.
  - Closeout:
    `docs/history/M48_06_FOURTH_SLICE_BLOCKER_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M48-closeout`: Fourth-slice runtime readiness decision. **Done.**
  - Results: M48 complete `true`, runtime-ready recipe available `false`,
    human/G-Zone review allowed `true`, G Zone deferred recipes `25`, next queue
    `M49`, no card data mutation, no runtime/UI/saved-deck/bot/GameState
    mutation.
  - Closeout:
    `docs/history/M48_CLOSEOUT_FOURTH_SLICE_RUNTIME_READINESS_CLOSEOUT.md`.
- `M49-01`: Fourth-slice human repair review packet. **Done.**
  - Results: review items `25`, complete manual repair packages `25`,
    complete grade-profile candidates `24`, G Zone decision items `25`,
    ready_for_m49_02 `true`, no card data mutation, no runtime/UI/saved-deck/
    bot/GameState mutation.
  - Closeout:
    `docs/history/M49_01_FOURTH_SLICE_HUMAN_REPAIR_REVIEW_PACKET_CLOSEOUT.md`.
- `M49-02`: Fourth-slice G Zone support decision. **Done.**
  - Results: selected `main_deck_only_for_current_windows_fixture`, decision
    items `25`, main-deck-only validation allowed `true`, G Zone runtime
    `false`, Stride runtime `false`, runtime promotion `false`,
    ready_for_m49_03 `true`, no card data/runtime/UI/saved-deck/bot/
    GameState mutation.
  - Closeout:
    `docs/history/M49_02_FOURTH_SLICE_G_ZONE_SUPPORT_DECISION_CLOSEOUT.md`.
- `M49-03`: Fourth-slice human-accepted repair artifact. **Done.**
  - Results: accepted `m48_recipe_001`, selected G Zone option
    `main_deck_only_for_current_windows_fixture`, main-deck-only boundary
    applied `true`, G Zone runtime `false`, Stride runtime `false`, repaired
    main deck `50`, grade profile `17/14/11/8`, repair issues `0`, declares
    recipe valid `false`, runtime promotion `false`, ready_for_m49_04 `true`.
  - Closeout:
    `docs/history/M49_03_FOURTH_SLICE_HUMAN_ACCEPTED_REPAIR_ARTIFACT_CLOSEOUT.md`.
- `M49-04`: Fourth-slice repaired recipe validation rerun. **Done.**
  - Results: accepted `m48_recipe_001`, validator passed `1`, runtime-ready
    recipes `1`, issue_counts `{}`, G Zone runtime `false`, Stride runtime
    `false`, runtime fixture created `false`, runtime promotion `false`,
    ready_for_m49_05 `true`.
  - Closeout:
    `docs/history/M49_04_FOURTH_SLICE_REPAIRED_RECIPE_VALIDATION_RERUN_CLOSEOUT.md`.
- `M49-05`: Fourth-slice runtime fixture gate. **Done.**
  - Results: promotion allowed `true`, passed checks `8`, failed checks `0`,
    fixture created `true`, G Zone runtime `false`, Stride runtime `false`,
    saved-deck/UI/bot/GameState mutation disabled,
    ready_for_m49_closeout `true`.
  - Closeout:
    `docs/history/M49_05_FOURTH_SLICE_RUNTIME_FIXTURE_GATE_CLOSEOUT.md`.
- `M49-closeout`: Fourth-slice fixture closeout. **Done.**
  - Results: M49 complete `true`, fourth runtime fixture available `true`,
    next queue `M50`, G Zone runtime `false`, Stride runtime `false`,
    saved-deck/UI/bot/GameState mutation disabled.
  - Closeout:
    `docs/history/M49_FOURTH_SLICE_FIXTURE_CLOSEOUT.md`.

## M50: Fourth Fixture Consumption and Four-Fixture Scale Gate - Done

- `M50-01`: Fourth fixture schema validator. **Done.**
  - Results: schema valid `true`, blockers `0`, main deck `50`, unique cards
    `14`, trigger profile `4/4/4/4`, grade profile `17/14/11/8`, G Zone
    runtime `false`, Stride runtime `false`, ready_for_m50_02 `true`.
  - Closeout:
    `docs/history/M50_01_FOURTH_FIXTURE_SCHEMA_VALIDATOR_CLOSEOUT.md`.
- `M50-02`: Fourth fixture deck text exporter. **Done.**
  - Results: export ready `true`, blockers `0`, main deck `50`, exported card
    lines `14`, G section comment-only, ready_for_m50_03 `true`.
  - Closeout:
    `docs/history/M50_02_FOURTH_FIXTURE_DECK_TEXT_EXPORT_CLOSEOUT.md`.
- `M50-03`: Fourth fixture headless load smoke. **Done.**
  - Results: offline load ready `true`, deck code created `true`, Unity
    headless accepted `deck_source=deck_code`, actions/events `4/4`, G Zone
    count `0`, ready_for_m50_04 `true`.
  - Closeout:
    `docs/history/M50_03_FOURTH_HEADLESS_FIXTURE_LOAD_SMOKE_CLOSEOUT.md`.
- `M50-04`: Four-fixture scale decision. **Done.**
  - Results: fixture evidence `4`, passed fixtures `4`, failed fixtures `0`,
    candidates `5`, fifth-slice offline pipeline allowed `true`, G Zone runtime
    `false`, Stride runtime `false`, ready_for_m51 `true`.
  - Closeout:
    `docs/history/M50_04_FOUR_FIXTURE_SCALE_DECISION_CLOSEOUT.md`.

## M51: Fifth Fixture Pipeline - Active

- `M51-01`: Fifth target slice selection. **Done.**
  - Results: selected `โกลด์ พาลาดิน`, era preset
    `link_joker_legion_mate`, candidate count `5`, ready_for_m51_02 `true`,
    G Zone runtime `false`, Stride runtime `false`, runtime/UI/saved-deck/bot
    promotion disabled.
  - Closeout:
    `docs/history/M51_01_FIFTH_TARGET_SLICE_SELECTION_CLOSEOUT.md`.
- `M51-02`: Fifth-slice fixture/format readiness. **Done.**
  - Results: source_card_count `106`, trigger capacity `36`,
    non-trigger capacity `388`, trigger gaps `[]`, all fixture expectations
    met `true`, semantic probe ready `true`, ready_for_m51_03 `true`,
    G Zone runtime `false`, Stride runtime `false`, runtime/UI/saved-deck/bot
    promotion disabled.
  - Closeout:
    `docs/history/M51_02_FIFTH_SLICE_FIXTURE_READINESS_CLOSEOUT.md`.
- `M51-03`: Fifth-slice semantic/compatibility probe. **Done.**
  - Results: semantic cards `106`, manual-review cards `4`, pair graph edges
    `3075`, candidate edges `142`, all stage readiness passed `true`,
    ready_for_m51_04 `true`, G Zone runtime `false`, Stride runtime `false`,
    runtime/UI/saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M51_03_FIFTH_SLICE_SEMANTIC_COMPATIBILITY_PROBE_CLOSEOUT.md`.
- `M51-04`: Fifth-slice recipe pipeline entry gate. **Done.**
  - Results: offline recipe pipeline allowed `true`, blockers `0`,
    fixture scaffold required `true`, ready_for_m52 `true`, G Zone runtime
    `false`, Stride runtime `false`, runtime/UI/saved-deck/bot promotion
    disabled.
  - Closeout:
    `docs/history/M51_04_FIFTH_SLICE_RECIPE_PIPELINE_ENTRY_GATE_CLOSEOUT.md`.

## M52: Fifth-Slice Offline Recipe Pipeline - Active

- `M52-01`: Fifth-slice fixture scaffold. **Done.**
  - Results: source_card_count `106`, trigger capacity `36`,
    non-trigger capacity `388`, candidate edges `142`, scaffold_ready `true`,
    blockers `0`, ready_for_m52_02 `true`, G Zone runtime `false`, Stride
    runtime `false`, runtime/UI/saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_01_FIFTH_SLICE_FIXTURE_SCAFFOLD_CLOSEOUT.md`.
- `M52-02`: Fifth-slice review packet. **Done.**
  - Results: fixture scaffold items `1`, manual-review cards `4`, candidate
    edges `142`, total review items `147`, ready_for_m52_03 `true`, runtime/
    UI/saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_02_FIFTH_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M52-03`: Fifth-slice recipe draft model. **Done.**
  - Results: recipe drafts `25`, quantity-complete recipes `25`,
    trigger/missing skipped edges `0`, manual-overlap recipes `0`,
    ready_for_m52_04 `true`, runtime/UI/saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_03_FIFTH_SLICE_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M52-04`: Fifth-slice recipe validator. **Done.**
  - Results: recipes validated `25`, runtime-ready recipes `0`,
    validator-passed pending human selection `25`, invalid drafts `0`,
    missing/copy/slot/trigger/manual-overlap blockers `0`,
    grade-profile review recipes `25`, ready_for_m52_05 `true`, runtime/UI/
    saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_04_FIFTH_SLICE_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M52-05`: Fifth-slice combo-to-recipe consistency. **Done.**
  - Results: consistency checks `25`, pair cards present `25`,
    missing pair-card checks `0`, promotion allowed `0`, status
    `consistent_pending_human_selection=25`, ready_for_m52_06 `true`, runtime/
    UI/saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_05_FIFTH_SLICE_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M52-06`: Fifth-slice blocker repair candidates. **Done.**
  - Results: repair candidates `25`, complete grade-profile candidates `25`,
    human selection required `25`, unexpected structural blockers `0`,
    ready_for_m52_closeout `true`, runtime/UI/saved-deck/bot promotion
    disabled.
  - Closeout:
    `docs/history/M52_06_FIFTH_SLICE_BLOCKER_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M52-closeout`: Fifth-slice runtime readiness decision. **Done.**
  - Results: M52 complete `true`, runtime-ready recipe available `false`,
    human selection review allowed `true`, next queue `M53`, runtime/UI/
    saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M52_CLOSEOUT_FIFTH_SLICE_RUNTIME_READINESS_CLOSEOUT.md`.

### M53: Fifth-slice Human Selection and Repair Gate

- `M53-01`: Fifth-slice human repair review packet. **Done.**
  - Results: review items `25`, ready for human repair review `25`,
    complete grade-profile candidates `25`, human selection required `25`,
    unexpected structural blockers `0`, ready_for_m53_02 `true`, runtime/UI/
    saved-deck/bot promotion disabled.
  - Closeout:
    `docs/history/M53_01_FIFTH_SLICE_HUMAN_REPAIR_REVIEW_PACKET_CLOSEOUT.md`.
- `M53-02`: Fifth-slice human-selected recipe artifact. **Done.**
  - Results: selected review item `m53_01_m52_recipe_001_repair_review`,
    selected recipe `m52_recipe_001`, selected grade package
    `m52_recipe_001_grade_profile_pkg_001`, ready_for_m53_03 `true`, runtime
    promotion disabled.
- `M53-03`: Fifth-slice human-accepted repair artifact. **Done.**
  - Results: human selection and acceptance recorded, repaired main deck count
    `50`, repair application issues `0`, ready_for_m53_04 `true`, recipe
    validity and runtime promotion still not declared.
- `M53-04`: Fifth-slice repaired recipe validation rerun. **Done.**
  - Results: validation status `validator_passed`, consistency status
    `consistent_validator_passed`, runtime-ready recipes `1`,
    promotion-allowed checks `1`, blockers `0`, ready_for_m53_05 `true`,
    runtime fixture promotion still disabled in the rerun artifact.
- `M53-05`: Fifth-slice runtime fixture promotion gate. **Done.**
  - Results: promotion_allowed `true`, passed checks `5`, failed checks `0`,
    fixture created `true`, fixture path
    `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`,
    saved-deck/UI/bot/GameState mutation disabled.
- `M53-closeout`: Fifth-slice fixture closeout. **Done.**
  - Results: m53_complete `true`, fifth runtime fixture available `true`,
    next queue `M54`, saved-deck/UI/bot/GameState mutation disabled.
  - Verification: targeted M53 tests passed `35/35`; full Python unittest
    discovery passed `1000/1000`.

## M54: Fifth Fixture Consumption and Next-Slice Scale Gate - Active

Goal: validate how the Gold Paladin fixture can be consumed safely, export
reviewable deck text, then decide whether the fixture set is ready for another
scale step.

- `M54-01`: Fifth fixture schema validator. **Done.**
  - Results: schema valid `true`, blockers `0`, main deck `50`, unique cards
    `16`, trigger profile `4/4/4/4`, grade profile `17/14/11/8`,
    ready_for_m54_02 `true`, saved-deck/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1008/1008`.
  - Closeout:
    `docs/history/M54_01_FIFTH_FIXTURE_SCHEMA_VALIDATOR_CLOSEOUT.md`.
- `M54-02`: Fifth fixture deck text exporter. **Done.**
  - Results: export_ready `true`, blockers `0`, main deck `50`, exported card
    lines `16`, review-only count-line deck text generated, ready_for_m54_03
    `true`, saved-deck/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `7/7`; full Python unittest discovery
    `1015/1015`.
  - Closeout:
    `docs/history/M54_02_FIFTH_FIXTURE_DECK_TEXT_EXPORT_CLOSEOUT.md`.
- `M54-03`: Fifth fixture headless load smoke. **Done.**
  - Results: offline load ready `true`, deck code created `true`, Unity
    headless accepted `true`, deck source `deck_code`, actions/events `4/4`,
    blockers `0`, main deck `50`, unique cards `16`, G Zone `0`,
    ready_for_m54_04 `true`, saved-deck/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `9/9`; full Python unittest discovery
    `1024/1024`.
  - Closeout:
    `docs/history/M54_03_FIFTH_HEADLESS_FIXTURE_LOAD_SMOKE_CLOSEOUT.md`.
- `M54-04`: Multi-fixture scale decision. **Done.**
  - Results: five fixture evidence records reviewed, passed fixtures `5`,
    failed fixtures `0`, candidate count `5`, sixth-slice offline pipeline
    allowed `true`, ready_for_m55 `true`, runtime/UI/bot/GameState mutation
    disabled.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1032/1032`.
  - Closeout:
    `docs/history/M54_04_FIVE_FIXTURE_SCALE_DECISION_CLOSEOUT.md`.

## M55: Sixth Offline Slice Selection

Goal: select the next offline-only candidate from the five-fixture scale queue.
No runtime fixture, saved deck, UI deck list, bot playbook, G Zone runtime,
Stride runtime, or `GameState` mutation is allowed in `M55-01`.

- `M55-01`: Sixth target slice selection. **Done.**
  - Consume `M54-04` candidate queue and select one target for offline analysis.
  - Results: selected group `ชาโดว์ พาลาดิน`, era `g_next_z`, rank `4`,
    candidate count `5`, ready_for_m55_02 `true`, runtime/UI/bot/GameState
    mutation disabled.
  - Verification: targeted tests `7/7`; full Python unittest discovery
    `1039/1039`.
  - Closeout:
    `docs/history/M55_01_SIXTH_TARGET_SLICE_SELECTION_CLOSEOUT.md`.
- `M55-02`: Sixth-slice fixture/format readiness. **Done.**
  - Verify the selected Shadow Paladin / `g_next_z` target has enough card
    pool, format policy, and taxonomy readiness for offline recipe work.
  - Results: source cards `77`, grade profile `19/20/16/11/11`, trigger
    profile `Critical=4/Draw=4/Heal=2/Stand=2`, trigger capacity `48`,
    non-trigger capacity `260`, semantic probe ready `true`,
    ready_for_m55_03 `true`, runtime/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1047/1047`.
  - Closeout:
    `docs/history/M55_02_SIXTH_SLICE_FIXTURE_READINESS_CLOSEOUT.md`.
- `M55-03`: Sixth-slice semantic/compatibility probe. **Done.**
  - Build an advisory semantic/compatibility probe for Shadow Paladin /
    `g_next_z` without recipe, fixture, UI, bot, or GameState mutation.
  - Results: semantic cards `77`, manual-review cards `11`, pair graph edges
    `2069`, candidate edges `70`, all stage readiness `true`,
    ready_for_m55_04 `true`, runtime/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1055/1055`.
  - Closeout:
    `docs/history/M55_03_SIXTH_SLICE_SEMANTIC_COMPATIBILITY_PROBE_CLOSEOUT.md`.
- `M55-04`: Sixth-slice recipe pipeline entry gate. **Done.**
  - Decide whether the sixth slice may enter recipe draft modeling. This must
    not create recipes, fixtures, UI entries, bot playbooks, or mutate
    `GameState`.
  - Results: offline recipe pipeline allowed `true`, blockers `0`,
    ready_for_m56 `true`, source cards `77`, semantic cards `77`, candidate
    edges `70`, runtime/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `9/9`; full Python unittest discovery
    `1064/1064`.
  - Closeout:
    `docs/history/M55_04_SIXTH_SLICE_RECIPE_PIPELINE_ENTRY_GATE_CLOSEOUT.md`.

## M56: Sixth-Slice Offline Recipe Pipeline

Goal: prepare advisory source-backed recipe drafts for the selected Shadow
Paladin / `g_next_z` slice. Runtime fixture promotion, saved deck injection,
UI deck list publication, bot playbook promotion, G Zone runtime, Stride
runtime, and `GameState` mutation remain blocked until later explicit gates.

- `M56-01`: Sixth-slice fixture scaffold. **Done.**
  - Define the fixture policy scaffold required before recipe validation.
  - Results: scaffold_ready `true`, blockers `0`, source cards `77`, source
    series present `5`, trigger profile `Critical=4/Draw=4/Heal=2/Stand=2`,
    candidate edges `70`, manual-review cards `11`, Grade 4 cards advisory
    only until G Zone support, ready_for_m56_02 `true`, runtime/UI/bot/
    GameState mutation disabled.
  - Verification: targeted tests `9/9`; full Python unittest discovery
    `1073/1073`.
  - Closeout:
    `docs/history/M56_01_SIXTH_SLICE_FIXTURE_SCAFFOLD_CLOSEOUT.md`.
- `M56-02`: Sixth-slice review packet. **Done.**
  - Export fixture scaffold notes, manual-review cards, candidate edges, and
    format/mechanic caveats for human review.
  - Results: fixture scaffold items `1`, manual-review card items `11`,
    candidate edge items `70`, total review items `82`, ready_for_m56_03
    `true`, runtime/UI/bot/GameState mutation disabled.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1081/1081`.
  - Closeout:
    `docs/history/M56_02_SIXTH_SLICE_REVIEW_PACKET_CLOSEOUT.md`.
- `M56-03`: Sixth-slice recipe draft model. **Done.**
  - Create advisory recipe drafts only; no saved deck, UI injection, runtime
    fixture, bot playbook, or `GameState` mutation.
  - Results: candidate edge input count `70`, skipped trigger/Grade 4/missing
    edges `58`, recipe drafts `12`, quantity-complete recipes `12`,
    manual-overlap recipes `12`, fixture scaffold cards `14`, fixture scaffold
    total cards `50`, ready_for_m56_04 `true`.
  - Verification: targeted tests `9/9`; full Python unittest discovery
    `1090/1090`.
  - Closeout:
    `docs/history/M56_03_SIXTH_SLICE_RECIPE_DRAFT_MODEL_CLOSEOUT.md`.
- `M56-04`: Sixth-slice recipe validator. **Done.**
  - Validate count, trigger, grade, identity, copy limits, missing cards,
    manual-review blockers, and fixture scaffold constraints.
  - Results: validated drafts `12`, runtime-ready recipes `0`,
    manual-review blocked recipes `12`, grade-profile review recipes `12`,
    G Zone deferred recipes `12`, missing/copy/slot/trigger/Grade 4 blockers
    `0`, ready_for_m56_05 `true`.
  - Verification: targeted tests `7/7`; full Python unittest discovery
    `1097/1097`.
  - Closeout:
    `docs/history/M56_04_SIXTH_SLICE_RECIPE_VALIDATOR_CLOSEOUT.md`.
- `M56-05`: Sixth-slice combo-to-recipe consistency. **Done.**
  - Compare M56-04 validated recipe drafts back against M56-02 candidate
    edges and M56-03 pair anchors.
  - Confirm every advisory recipe still preserves the intended combo source/
    target cards, review blockers, and no-runtime-promotion boundaries.
  - Results: consistency checks `12`, pair cards present `12`, missing
    pair-card checks `0`, recipe manual dependency checks `12`, G Zone
    deferred checks `12`, promotion allowed `0`, ready_for_m56_06 `true`.
  - Verification: targeted tests `6/6`; full Python unittest discovery
    `1103/1103`.
  - Closeout:
    `docs/history/M56_05_SIXTH_SLICE_COMBO_RECIPE_CONSISTENCY_CLOSEOUT.md`.
- `M56-06`: Sixth-slice blocker repair candidates. **Done.**
  - Convert M56-04/M56-05 blockers into human-review repair options.
  - Keep repair candidates advisory only; no saved-deck injection, UI
    publication, runtime fixture promotion, bot playbook promotion, or
    `GameState` mutation.
  - Results: repair items `12`, manual repair complete `12`, grade repair
    complete `12`, G Zone deferred `12`, ready for human repair review `12`,
    runtime promotion allowed `false`, ready_for_m56_closeout `true`.
  - Verification: targeted tests `8/8`; full Python unittest discovery
    `1111/1111`.
  - Closeout:
    `docs/history/M56_06_SIXTH_SLICE_BLOCKER_REPAIR_CANDIDATES_CLOSEOUT.md`.
- `M56-closeout`: Sixth-slice runtime readiness decision. **Done.**
  - Summarize M55/M56 evidence and decide whether the sixth slice may become a
    runtime fixture or must remain blocked.
  - Expected result: runtime promotion remains blocked until human review and
    G Zone/Stride support decisions are complete.
  - Results: M56 complete `true`, runtime-ready recipe available `false`,
    human selection review allowed `true`, next queue `M57`, ready_for_next_queue
    `true`.
  - Verification: targeted tests `9/9`; full Python unittest discovery
    `1120/1120`.
  - Closeout:
    `docs/history/M56_CLOSEOUT_SIXTH_SLICE_RUNTIME_READINESS_CLOSEOUT.md`.
- `M57-01`: Sixth-slice human repair review packet. **Done.**
  - Export a concise review packet from M56-06 repair packages and M56-closeout
    decision blockers for team/human selection.
  - Results: review items `12`, complete manual repairs `12`, complete grade
    repairs `12`, G Zone deferred items `12`, ready_for_m57_02 `true`.
  - Verification: targeted tests `10/10`; full Python unittest discovery
    `1130/1130`.
  - Closeout:
    `docs/history/M57_01_SIXTH_SLICE_HUMAN_REPAIR_REVIEW_PACKET_CLOSEOUT.md`.
- `M57-02`: Sixth-slice human-selected recipe artifact. **Pending.**
  - Record exactly one selected sixth-slice recipe id from the M57-01 review
    packet without mutating M56 drafts.
  - Scaffold status: spec/tool/tests are present and reject non-M57 review ids.
    Targeted tests pass `9/9`; full Python unittest discovery passes
    `1139/1139`. The real `outputs/target_slice/m57_02_*` artifact is not
    generated until the user selects a valid `m57_01_m56_recipe_*_repair_review`
    item.
- `M57-03`: Sixth-slice human-accepted repair artifact. **Blocked by M57-02 output; scaffold ready.**
  - Record explicit acceptance after M57-02 exists, then build an in-memory
    repaired preview.
  - Apply manual substitutions first, detect source grade-package conflicts,
    then recompute grade-profile repair from post-manual quantities.
  - Scaffold status: spec/tool/tests are present. Targeted tests pass `7/7`;
    full Python unittest discovery passes `1146/1146`. The real
    `outputs/target_slice/m57_03_*` artifact is not generated until M57-02
    exists and explicit acceptance text is provided.
- `M57-04`: Sixth-slice G Zone / Stride decision artifact. **Blocked by M57-03 output; scaffold ready.**
  - Record an explicit G Zone / Stride boundary decision after M57-03 exists.
  - Support `main_deck_only_review_no_runtime_promotion` and
    `defer_until_g_zone_runtime_exists`.
  - Keep G Zone runtime, Stride runtime, runtime fixture promotion, saved deck
    injection, UI publication, bot/playbook integration, and `GameState`
    mutation disabled.
  - Scaffold status: spec/tool/tests are present. Targeted M57-03/M57-04 tests
    pass `16/16`; full Python unittest discovery passes `1155/1155`. The real
    `outputs/target_slice/m57_04_*` artifact is not generated until M57-03
    exists and an explicit G Zone / Stride option is provided.
- `M57-05`: Sixth-slice repaired recipe validation rerun. **Blocked by M57-04 output; scaffold ready.**
  - Validate the repaired main-deck preview from M57-03 after the explicit
    M57-04 boundary decision.
  - Reuse sixth-slice validator and combo consistency checks.
  - `main_deck_only_review_no_runtime_promotion` can suppress
    `g_zone_support_deferred` for main-deck validation only; G Zone and Stride
    runtime remain disabled.
  - Scaffold status: spec/tool/tests are present. Targeted M57-04/M57-05 tests
    pass `18/18`; full Python unittest discovery passes `1164/1164`. The real
    `outputs/target_slice/m57_05_*` report is not generated until M57-03 and
    M57-04 outputs exist.
- `M57-06`: Sixth-slice runtime fixture promotion gate. **Blocked by M57-05 output; scaffold ready.**
  - Promote only an offline runtime/test fixture after M57-03 accepted rows and
    M57-05 validation/consistency both pass.
  - Require the `main_deck_only_review_no_runtime_promotion` G Zone / Stride
    boundary, 50 cards, 16 triggers, grade profile `17/14/11/8`, Grade 4 main
    deck count `0`, pair-card consistency, and source-row integrity.
  - Keep saved deck injection, UI publication, bot/playbook integration,
    G Zone runtime, Stride runtime, and `GameState` mutation disabled.
  - Scaffold status: spec/tool/tests are present. Targeted M57-06 tests pass
    `9/9`; full Python unittest discovery passes `1173/1173`. The real
    `outputs/target_slice/m57_06_*` gate report and fixture are not generated
    until M57-03 and M57-05 outputs exist.
- `M57-closeout`: Sixth-slice fixture closeout. **Blocked by M57-06 output; scaffold ready.**
  - Close the sixth-slice fixture gate and select the next queue without
    mutating fixture artifacts, runtime deck libraries, saved decks, UI deck
    lists, bot/playbooks, G Zone/Stride runtime, or `GameState`.
  - If M57-06 passes, route to `M58` for sixth fixture schema validation, deck
    text export, headless load smoke, and six-fixture scale decision.
  - If M57-06 fails, route to `M57-repair`.
  - Scaffold status: spec/tool/tests are present. Targeted M57-closeout tests
    pass `6/6`; full Python unittest discovery passes `1179/1179`. The real
    `outputs/target_slice/m57_closeout_*` report is not generated until M57-06
    output exists.

### M58: Sixth Fixture Consumption and Six-Fixture Scale Gate

- `M58-01`: Sixth fixture schema validator. **Blocked by M57-06 output; scaffold ready.**
  - Validate the Shadow Paladin runtime fixture independently from the M57
    generator before any deck text, headless smoke, UI, or bot consumption.
  - Scaffold status: spec/tool/tests are present. Targeted M58-01 tests pass
    `11/11`; full Python unittest discovery passes `1190/1190`. Tests use an
    in-memory fixture built from the M57-02 through M57-06 chain. The real
    `outputs/target_slice/m58_01_*` validation report is not generated until
    the M57-06 runtime fixture file exists.
- `M58-02`: Sixth fixture deck text exporter. **Blocked by M58-01 real output; scaffold ready.**
  - Export the Shadow Paladin fixture as reviewable count-line deck text
    without adding it to saved decks.
  - Scaffold status: spec/tool/tests are present. Targeted M58-02 tests pass
    `7/7`; full Python unittest discovery passes `1197/1197`. Tests use an
    in-memory fixture and in-memory M58-01 validation report. The real
    `outputs/target_slice/m58_02_*` deck text/report artifacts are not
    generated until the M57-06 runtime fixture file and M58-01 validation
    report file exist.
- `M58-03`: Sixth fixture headless load smoke. **Blocked by M58-02 real output; scaffold ready.**
  - Load the Shadow Paladin fixture through offline/headless paths without UI,
    bot, G Zone, or Stride mutation.
  - Scaffold status: spec/tool/tests are present. Targeted M58-03 tests pass
    `9/9`; full Python unittest discovery passes `1206/1206`. Tests use
    in-memory M57-06/M58-01/M58-02 artifacts. Offline deck-code smoke is ready
    in scaffold, but real `outputs/target_slice/m58_03_*` artifacts and Unity
    headless evidence are not generated until upstream real files exist.
- `M58-04`: Six-fixture scale decision. **Blocked by M58-03 real Unity evidence; scaffold ready.**
  - Review all six fixture evidence before selecting any further slice.
  - Scaffold status: spec/tool/tests are present. Targeted M58-04 tests pass
    `8/8`; full Python unittest discovery passes `1214/1214`. Tests use the
    first five real smoke reports plus in-memory sixth fixture smoke with
    accepted Unity evidence. The real `outputs/target_slice/m58_04_*` scale
    decision artifacts are not generated until real M58-03 smoke and Unity
    evidence files exist.

### M59: Seventh Offline Slice Selection

- `M59-01`: Seventh target slice selection. **Blocked by M58-04 real output; scaffold ready.**
  - Select the next offline target from the M58-04 candidate queue only after
    the six-fixture scale gate allows `ready_for_m59`.
  - Scaffold status: spec/tool/tests are present. Targeted M59-01 tests pass
    `7/7`; full Python unittest discovery passes `1221/1221`. Tests use an
    in-memory M58-04 scale decision built from the first five real smoke reports
    plus in-memory sixth fixture smoke. The real
    `outputs/target_slice/m59_01_*` selection artifacts are not generated until
    the real M58-04 scale decision output exists.
- `M59-02`: Seventh-slice fixture/format readiness. **Blocked by M59-01 real output; scaffold ready.**
  - Start only after real or scaffold M59-01 selection evidence exists for the
    next offline target.
  - Scaffold status: spec/tool/tests are present. Targeted M59-02 tests pass
    `9/9`; full Python unittest discovery passes `1230/1230`. Tests use an
    in-memory M59-01 selection and verify `เนโอ เนคต้า` / `g_series_first`:
    source cards `78`, grade profile `17/23/18/12/8`, trigger capacity `48`,
    non-trigger capacity `264`, trigger gaps `[]`, and ready_for_m59_03 `true`.
    The real `outputs/target_slice/m59_02_*` readiness artifacts are not
    generated until the real M59-01 selection output exists.
- `M59-03`: Seventh-slice semantic/compatibility probe. **Blocked by M59-01/M59-02 real outputs; scaffold ready.**
  - Start after M59-02 readiness evidence.
  - Scaffold status: spec/tool/tests are present. Targeted M59-03 tests pass
    `8/8`; full Python unittest discovery passes `1238/1238`. Tests use
    in-memory M59-01/M59-02 reports and verify `เนโอ เนคต้า` /
    `g_series_first`: semantic cards `78`, manual-review cards `10`, pair
    graph edges `2885`, candidate edges `107`, and ready_for_m59_04 `true`.
    The real `outputs/target_slice/m59_03_*` probe artifacts are not generated
    until the real M59-01 and M59-02 outputs exist.
- `M59-04`: Seventh-slice recipe pipeline entry gate. **Blocked by M59-02/M59-03 real outputs; scaffold ready.**
  - Start after M59-03 probe evidence.
  - Scaffold status: spec/tool/tests are present. Targeted M59-04 tests pass
    `9/9`; full Python unittest discovery passes `1247/1247`. Tests use
    in-memory M59-02/M59-03 reports and verify the gate allows the M60 offline
    recipe pipeline only: source cards `78`, semantic cards `78`,
    manual-review cards `10`, pair graph edges `2885`, candidate edges `107`,
    blocking issues `0`, and ready_for_m60 `true`. The real
    `outputs/target_slice/m59_04_*` gate artifacts are not generated until the
    real M59-02 and M59-03 outputs exist.

### M60: Seventh Offline Recipe Pipeline

- `M60-01`: Seventh-slice fixture scaffold. **Blocked by M59-02/M59-03/M59-04 real outputs; scaffold ready.**
  - Start after M59-04 gate evidence. Define source-backed fixture policy for
    the Neo Nectar G-series slice before validator work.
  - Scaffold status: spec/tool/tests are present. Targeted M60-01 tests pass
    `9/9`; full Python unittest discovery passes `1256/1256`. Tests use
    in-memory M59-02/M59-03/M59-04 reports and verify source cards `78`,
    grade profile `17/23/18/12/8`, trigger profile `Critical=5`, `Draw=2`,
    `Heal=2`, `Stand=3`, candidate edges `107`, manual-review cards `10`,
    blocking issues `0`, and ready_for_m60_02 `true`. The real
    `outputs/target_slice/m60_01_*` scaffold artifacts are not generated until
    the real M59-02, M59-03, and M59-04 outputs exist.
- `M60-02`: Seventh-slice review packet. **Blocked by M59-01/M59-02/M59-03/M59-04/M60-01 real outputs; scaffold ready.**
  - Start after M60-01 scaffold evidence. Export candidate edges,
    manual-review cards, and format notes for human review without recipe
    drafts, runtime fixtures, saved deck injection, UI publication, bot/playbook
    promotion, G Zone runtime, Stride runtime, or `GameState` mutation.
  - Scaffold status: spec/tool/tests are present. Targeted M60-02 tests pass
    `9/9`; full Python unittest discovery passes `1265/1265`. Tests use
    in-memory M59-01/M59-02/M59-03/M59-04/M60-01 reports and verify fixture
    scaffold items `1`, manual-review cards `10`, candidate edges `107`, total
    review items `118`, ready_for_m60_03 `true`, and runtime/UI/bot/G Zone/
    Stride/GameState mutation disabled. The real
    `outputs/target_slice/m60_02_*` review packet artifacts are not generated
    until the real upstream outputs exist.
- `M60-03`: Seventh-slice recipe draft model. **Blocked by M60-02/M60-01 real outputs; scaffold ready.**
  - Start after M60-02 review packet evidence. Create advisory recipe drafts
    only; do not create saved decks, UI deck entries, runtime fixtures, bot
    playbooks, G Zone runtime, Stride runtime, or `GameState` mutation.
  - Scaffold status: spec/tool/tests are present. Targeted M60-03 tests pass
    `9/9`; full Python unittest discovery passes `1274/1274`. Tests use
    in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02 reports and verify
    candidate edge inputs `107`, skipped trigger/Grade 4/missing edges `84`,
    advisory recipe drafts `23`, quantity-complete recipes `23`, manual-overlap
    recipes `23`, fixture scaffold card count `14`, fixture scaffold total
    cards `50`, ready_for_m60_04 `true`, and runtime/UI/bot/G Zone/Stride/
    GameState mutation disabled. The real
    `outputs/target_slice/m60_03_*` draft artifacts are not generated until the
    real upstream outputs exist.
- `M60-04`: Seventh-slice recipe validator. **Blocked by M60-03 real output; scaffold ready.**
  - Start after M60-03 draft evidence. Validate advisory drafts for count,
    trigger profile, grade profile, identity, set scope, copy limits, missing
    cards, Grade 4 exclusion, manual-review dependencies, and fixture scaffold
    constraints.
  - Scaffold status: spec/tool/tests are present. Targeted M60-04 tests pass
    `7/7`; full Python unittest discovery passes `1281/1281`. Tests use
    in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02/M60-03 reports and
    verify validated drafts `23`, runtime-ready recipes `0`, blocked-by-manual
    review `23`, missing/copy/slot/trigger/Grade 4 main-deck blockers `0`,
    grade-profile review recipes `21`, G Zone deferred recipes `23`,
    Bloom/token deferred recipes `23`, ready_for_m60_05 `true`, and runtime/UI/
    bot/GameState mutation disabled. The real
    `outputs/target_slice/m60_04_*` validation artifacts are not generated
    until the real M60-03 output exists.
- `M60-05`: Seventh-slice combo-to-recipe consistency. **Blocked by M60-03/M60-04 real outputs; scaffold ready.**
  - Start after M60-04 validator evidence. Confirm each recipe still contains
    its candidate edge pair cards and carry validator blockers forward without
    mutating drafts or promoting runtime.
  - Scaffold status: spec/tool/tests are present. Targeted M60-05 tests pass
    `6/6`; full Python unittest discovery passes `1287/1287`. Tests use
    in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02/M60-03/M60-04 reports
    and verify consistency checks `23`, pair cards present `23`, missing pair
    checks `0`, recipe manual dependencies `23`, G Zone deferred checks `23`,
    Bloom/token deferred checks `23`, promotion_allowed `0`, ready_for_m60_06
    `true`, and runtime/UI/bot/GameState mutation disabled. The real
    `outputs/target_slice/m60_05_*` consistency artifacts are not generated
    until the real M60-03 and M60-04 outputs exist.
- `M60-06`: Seventh-slice blocker repair candidates. **Blocked by M60-03/M60-04/M60-05 real outputs; scaffold ready.**
  - Start after M60-05 consistency evidence. Convert validator/consistency
    blockers into source-backed repair options without accepting repairs,
    mutating drafts, or promoting runtime.
  - Scaffold status: spec/tool/tests are present. Targeted M60-06 tests pass
    `8/8`; full Python unittest discovery passes `1295/1295`. Tests use
    in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02/M60-03/M60-04/M60-05
    reports and verify repair items `23`, complete manual repair candidates
    `23`, grade-profile repair candidates `21`, complete grade-profile
    candidates `21`, G Zone deferred packages `23`, Bloom/token deferred
    packages `23`, unexpected structural blockers `0`, human repair review
    ready `23`, ready_for_m60_closeout `true`, and runtime/UI/bot/GameState
    mutation disabled. The real `outputs/target_slice/m60_06_*` repair
    artifacts are not generated until the real upstream outputs exist.
- `M60-closeout`: Seventh-slice runtime readiness decision. **Pending.**
  - Start after M60-06 repair evidence. Decide whether the seventh slice
    remains advisory or may enter a bounded human acceptance/runtime fixture
    gate. Do not record human acceptance, mutate drafts, create saved decks,
    publish UI deck lists, enable G Zone/Stride runtime, or promote bot use.

## Post-M28 Backlog (not in active queue)

Items noted but deferred until user instruction:

- Audio / sound effects foundation.
- Localization / i18n string table.
- Version / update distribution strategy.
- Photon server region / self-host configuration.
- Android / mobile / APK / release work.
- RL / self-play research (gated on M17 platform).
- NPC campaign / story mode.

## Project Structure

```text
/
  AGENTS.md
  README.md
  docs/
  outputs/
    kk_cardfight_export/
  tools/
    data/
    verification/
  client/
    unity/
      VanguardThaiSim/
  data/
    packs/
      vanguard_th/
  tests/
```

## Engineering Principles

- Vertical slice เล็กๆ ที่ทดสอบได้จริง
- Manual play ก่อน auto effect
- ใช้ข้อมูลการ์ดที่ดึงมาแล้วเป็น source of truth
- รูปเก็บเป็นไฟล์ ไม่เก็บ binary ใน SQLite
- ทุก action ต้องมี event log (undo, replay, bot, multiplayer)
- PC build มาก่อน แล้วค่อย Android/iOS
- เปลี่ยน stack/architecture ต้องเพิ่ม ADR

## Definition of Done

งานถือว่าเสร็จเมื่อ: ตรง spec, build/test ผ่าน, ไม่มี missing data/broken path,
docs อัปเดต, สรุปสิ่งที่ทำและข้อจำกัดชัดเจน. ดูรายละเอียดใน
`docs/DEFINITION_OF_DONE.md`.

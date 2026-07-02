# AI Context Brief

## One Paragraph Summary

เรากำลังสร้างโปรแกรม Cardfight!! Vanguard ภาษาไทยที่เล่นได้บนคอมและมือถือ โดยใช้ Unity เป็น client หลัก มีฐานข้อมูลการ์ดไทยและรูปครบจาก KK Card Fight, มี deck builder, manual play table, replay/action log, bot CPU fight และจะต่อ online/room fight ภายหลัง

## First File For AI Agents

อ่าน `docs/AI_QUICK_START.md` ก่อนเสมอ ไฟล์นั้นสรุปทิศทางปัจจุบัน, target ถัดไป, gate สำคัญ, command verification และเอกสารที่ต้องเปิดเฉพาะเมื่อแตะระบบนั้น ๆ

## Current Data

- การ์ด Vanguard TH: `10,836` ใบ
- รูปการ์ด: `10,836` ไฟล์
- ไฟล์หลัก: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- Verification: `outputs/kk_cardfight_export/data/verification_report.json`
- Runtime pack: `data/packs/vanguard_th/cards.sqlite`, `card_catalog.json`, `manifest.json`, `verification_report.json`

## Current Implementation Status

- Unity project: `client/unity/VanguardThaiSim/`
- Unity version: `6000.5.0f1`
- Completed: M0 bootstrap, M1 card data pipeline, M2 card data access, all M3 deck tasks, all M4 manual play/replay tasks, all M5 bot baseline/profile tasks, all M5.5 Rules Core Hardening tasks, all M6 mobile readiness tasks, all M7 custom card pack tasks, all M8 multiplayer foundation tasks, the Photon Realtime transport scaffold, Photon Realtime SDK 5.1.15 import with `VANGUARD_PHOTON_REALTIME` enabled, AppId-backed live Photon smoke-test runners, hidden-state player/spectator/replay views, Photon lobby/room UI foundation, reconnect request/batch UI/protocol surface, live Photon lobby controller smoke verification, multiplayer game-session event sync controller, live Photon game-session smoke verification, PlayTable online-session action publishing, lobby deck exchange/start-table handoff, deterministic room game ids, online event cursor/sync status, lobby pending reconnect batch handoff, deck privacy/commitment spec/ADR, canonical deck commitment hashing, public/ranked shared-deck-code validation guards, deck reveal verification service, private-event payload spec, gameplay guard for non-shared deck privacy modes, `NetworkPublicGameEvent` model/Photon codec, masking conversion from true `GameEvent` to public events, public event replay logs, owner/opponent public-event delivery surface, public reveal proof metadata, conservative commitment-only gameplay policy, deck reveal request/response transport flow, lobby reveal UI/status surface, owner-private room initialization spec/runtime initializer, M11 RulesCore completion, M12 structured card ability data, M13-01 owner-private room initialization, exact trigger probability engine, board/resource evaluator, battle sequence search prototype, opponent guard/shield estimator, archetype/rideline playbook model, offline combo discovery scaffold, trigger definition/resolver scaffold, pending auto ability queue model scaffold, pending auto ability queue GameState integration scaffold, pending auto ability queue session publish helper scaffold, pending auto ability PlayTable publish control scaffold, pending auto ability decoded item list formatter scaffold, pending auto ability PlayTable item list surface scaffold, pending auto ability selection state scaffold, pending auto ability selection status formatter scaffold, pending auto ability PlayTable selection status surface scaffold, pending auto ability PlayTable selection cycle control scaffold, pending auto ability PlayTable clear selection control scaffold, pending auto ability selected resolution request model scaffold, pending auto ability selected resolution request formatter scaffold, pending auto ability PlayTable selected resolution request preview scaffold, pending auto ability selected resolution request payload codec scaffold, pending auto ability selected resolution request Photon wrapper scaffold, pending auto ability selected resolution request transport hook scaffold, pending auto ability selected resolution request session storage scaffold, pending auto ability selected resolution request session publish helper scaffold, pending auto ability PlayTable selected resolution request publish control scaffold, pending auto ability selected resolution request summary surface scaffold, pending auto ability selected resolution request list formatter scaffold, pending auto ability selected resolution request PlayTable list surface scaffold, manual resolution apply preview log entry scaffold, manual resolution apply preview log formatter scaffold, manual resolution apply preview session log storage, manual resolution apply preview PlayTable surface, manual resolution apply preview flow verification, pending auto ability queue commit policy spec, pending auto ability queue commit helper, pending auto ability queue commit event model, trigger check commit event model, trigger allocation commit helper, trigger modifier cleanup integration, trigger allocation plan model, combat modifier ledger scaffold, trigger allocation modifier adapter, combat stat projection scaffold, combat modifier cleanup timing helper, trigger check resolution bundle scaffold, trigger check log entry scaffold, trigger check replay log scaffold, trigger check replay log masking scaffold, trigger check replay payload codec scaffold, trigger check Photon payload wrapper scaffold, trigger check transport hook scaffold, trigger check session storage scaffold, trigger check PlayTable UI surface scaffold, trigger check manual publish control scaffold, trigger check manual draft payload scaffold, trigger check draft session publish helper, trigger check draft PlayTable control scaffold, trigger check draft type selector scaffold, trigger check draft source selector scaffold, trigger check draft index selector scaffold, trigger check draft summary panel scaffold, trigger check draft selected-card summary scaffold, trigger check draft selected-zone summary scaffold, trigger check draft clear-selection control scaffold, trigger check draft selected-card status helper scaffold, trigger check draft metadata formatter scaffold, trigger check draft full summary formatter scaffold, trigger check draft control-state helper scaffold, trigger check draft selection validation helper scaffold, trigger check draft publish result message formatter scaffold, trigger check draft selector cycle helper scaffold, trigger check draft status message formatter scaffold, trigger check draft request factory scaffold, trigger check log publish result formatter scaffold, trigger check log summary formatter scaffold, trigger check log publish online validation helper scaffold, PlayTable mode summary formatter scaffold, PlayTable card selection status formatter scaffold, PlayTable action status formatter scaffold, and PlayTable event log formatter scaffold.
- Card browser bootstrap: `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/CardBrowserBootstrap.cs`
- Deck system: `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/`
- Game state: `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Game/`
- Manual table includes Gift marker state/actions/UI for Force, Accel, and Protect markers.
- Bot system: `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Bots/`
- Latest verified commands: `python tools\verification\verify_vanguard_th_pack.py`, `python tools\data\validate_custom_pack_schema.py data\templates\custom_pack`, `python tools\data\validate_custom_pack_schema.py data\templates\custom_pack_v2`, `python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite`, `python tools\data\import_custom_pack.py data\templates\custom_pack_v2 --output-dir work\custom_pack_v2_import --overwrite`, `python tools\data\validate_ability_schema.py data\packs\vanguard_th\abilities\structured_ability_pack_m12_10.json`, `python -m unittest discover -s tests -p "test_*.py"` 44/44 after runtime JSON catalog export and Android install-smoke `--push-pack` coverage, Unity compile log `unity_compile_json_catalog_fallback_b.log` with no compiler errors, Unity EditMode tests 885/885 at `unity_editmode_json_catalog_fallback_b.xml`, Windows build artifact and player smoke after taxonomy refresh, Android build artifact `android_build_json_catalog_fallback_b.log`, Android install-smoke `android_install_smoke_json_catalog_push_launch_b.json` with LDPlayer install/push/force-stop/launch passed, release candidate checklist after M18-10, `ClientSmokeFlowRunner`, `HeadlessSimulationCliRunner`, `HeadlessBatchSimulationCliRunner`, `HeadlessDatasetExporter`, `HeadlessObservationActionRewardApi`, `HeadlessPerformanceProfiler`, `PackedStateDecisionGate`, `DistributedWorkerPrototypeSpec`, `DistributedWorkerPrototype`, `CoreRegressionSuiteReportBuilder`, `AbilityRegressionSuiteReportBuilder`, `MultiplayerPayloadNoLeakSuiteReportBuilder`, `WindowsBuildArtifactRunner`, `AndroidBuildArtifactRunner`, `PhotonLobbySmokeTestRunner`, and `PhotonGameSessionSmokeTestRunner`. Latest CI after M18-04 adds `.github/workflows/unity-editmode.yml` for self-hosted Windows Unity EditMode tests with log/XML artifacts; latest QA/build docs add `CORE_REGRESSION_SUITE_SPEC.md`, `ABILITY_REGRESSION_SUITE_SPEC.md`, `MULTIPLAYER_PAYLOAD_NO_LEAK_SUITE_SPEC.md`, `WINDOWS_BUILD_ARTIFACT_SPEC.md`, `ANDROID_BUILD_ARTIFACT_SPEC.md`, `RELEASE_CANDIDATE_CHECKLIST.md`, and regression/build validation.
- Latest Home/Lobby layout polish: `unity_compile_home_lobby_layout_polish.log`
  has no compiler errors, `unity_editmode_home_lobby_layout_polish.xml` passed
  886/886, `windows_build_home_lobby_layout_polish.log` rebuilt the Windows
  artifact, and `player_smoke_home_lobby_layout_polish.json` reported 4 steps
  with 0 blockers.
- Latest M19-09 icon override loader: `UiGameSymbolRegistry` and
  `UserIconPackValidator` provide semantic symbol defaults plus safe
  user-provided icon override validation. Missing private files fall back to
  default English labels, path traversal is rejected, Home trigger badges use
  semantic keys, `unity_compile_m19_09_icon_override_loader.log` has no compiler
  errors, `unity_editmode_m19_09_icon_override_loader.xml` passed 892/892,
  `windows_build_m19_09_icon_override_loader.log` rebuilt the Windows artifact,
  and `player_smoke_m19_09_icon_override_loader.json` reported 4 steps with 0 blockers.
- Latest Android refresh after M19-09: `android_build_m19_09_icon_override_loader.log`
  rebuilt the APK with `errors=0`, `warnings=1`,
  `android_install_smoke_m19_09_icon_override_loader.json` passed on LDPlayer
  with runtime pack push/force-stop/launch, and
  `android_ldplayer_m19_09_icon_override_loader_home.png` shows Home loaded with
  `Pack: Vanguard TH / 251`, `Cards 10836`, and compact trigger badges.
- Latest comparator study: `outputs/comparator_study/COMPARATOR_SYSTEM_UX_STUDY_TH.md`
  reviews VangPro, `dragogodev/cgs`, local Vanguard Area, local Dear Days,
  local Card Game Simulator, and local Cardfight Connect under safe
  static-research boundaries. Main takeaways: VangPro informs lobby/deck
  builder/deck tools/custom card import/accessory UX, Vanguard Area informs
  manual simulator/deck/replay UX, CGS informs custom pack schema and
  human-readable deck export, Cardfight Connect shows Unity Netcode/UGS stack
  clues but does not justify a transport switch yet, and Dear Days local packs
  should remain high-level inspiration only.
- Latest product reset: `M20` Windows Product Reset is complete. Active work is
  now Windows-first program completion: Home Dashboard -> Card Workshop / Battle Center -> PlayTable -> Battle Center -> Home. Android, LDPlayer, APK, mobile QA, app packaging,
  release-candidate packaging, and public distribution are deferred until the
  user explicitly re-enables that track. Use
  `docs/WINDOWS_FIRST_PROGRAM_COMPLETION_SPEC.md`,
  `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`, and
  `docs/WINDOWS_ONLY_VERIFICATION_PROFILE.md` before starting new product work.
- Latest M28 Windows gameplay completion status: `M28-01` gameplay completion
  gate, `M28-02` local PlayTable seat toggle, and `M28-03` UI-level two-seat
  match smoke are complete. Latest verification: PlayMode
  `unity_playmode_m28_03_two_seat_playmode_smoke_r2.xml` passed `2/2`,
  EditMode `unity_editmode_m28_03_two_seat_playmode_smoke.xml` passed
  `1131/1131`, and client smoke
  `client_smoke_m28_03_two_seat_playmode_smoke.log` passed with
  `blockers=[]`. `M28-04` Windows manual match gap audit is complete. `M28-05`
  PlayTable guided next-action panel is complete with EditMode `1136/1136`,
  PlayMode `2/2`, and client smoke `blockers=[]`. `M28-06` Windows
  built-player smoke is complete with build `errors=0 warnings=0` and player
  smoke `blockers=[]`. `M28-07` PlayTable action grouping polish is complete
  with EditMode `1137/1137`, PlayMode `2/2`, client smoke `blockers=[]`, and
  Windows player smoke `blockers=[]`. `M28-08` PlayTable side-panel density
  audit is complete. `M28-09` Bot Plan Advanced drawer cleanup is complete with
  EditMode `1138/1138`, PlayMode `2/2`, client smoke `blockers=[]`, and Windows
  player smoke `blockers=[]`. `M29-01` Photon lobby navigation lockout is
  complete with EditMode `1140/1140`, client smoke `blockers=[]`, Windows build
  `errors=0 warnings=0`, and Windows player smoke `blockers=[]`. `M29-02`
  Photon lobby reconnect flow polish is complete with EditMode `1142/1142`,
  client smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows
  player smoke `blockers=[]`. `M29-03` Photon lobby Quick Deck Selector is
  complete with EditMode `1144/1144`, client smoke `blockers=[]`, Windows build
  `errors=0 warnings=0`, and Windows player smoke `blockers=[]`. `M29-04`
  Photon lobby Quick Edit modal is complete with EditMode `1146/1146`, client
  smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows player
  smoke `blockers=[]`. `M29-05` Online Room usability closeout audit is
  complete. `M29-06` Online deck readiness guard is complete with EditMode
  `1149/1149`, client smoke `blockers=[]`, Windows build `errors=0 warnings=0`,
  and Windows player smoke `blockers=[]`. `M28-10` Match Log / Preview density
  review is complete with EditMode `1151/1151`, client smoke `blockers=[]`,
  Windows build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
  `M30-01` Windows playable loop final audit is complete and found the Replay
  Home route still locked behind `ShowReplayLocked()`. `M30-02` Windows Replay
  entry/browser is complete with EditMode `1153/1153`, client smoke
  `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`. `M30-03` Windows Replay local file import is complete with
  EditMode `1157/1157`, client smoke `blockers=[]`, Windows build
  `errors=0 warnings=0`, and Windows player smoke `blockers=[]`. `M30-04`
  Windows Replay viewer launch is complete with EditMode `1158/1158`, client
  smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows player
  smoke `blockers=[]`. `M30-05` Windows PlayTable replay export is complete
  with EditMode `1160/1160`, client smoke `blockers=[]`, Windows build
  `errors=0 warnings=0`, and Windows player smoke `blockers=[]`. `M30-06`
  Windows playable loop closeout audit is complete and found no automated
  Windows-loop blocker. `M31-01` Windows UI evidence audit is complete and set
  the next UX slice from player-facing evidence. `M31-02` Card Workshop
  first-screen clarity pass is complete with EditMode `1166/1166`, client smoke
  `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`. `M31-03` Card Workshop toolbar density pass is complete with
  EditMode `1169/1169`, client smoke `blockers=[]`, Windows build
  `errors=0 warnings=0`, and Windows player smoke `blockers=[]`. `M31-04`
  Windows UI visual evidence pass is complete with five screenshot artifacts
  and identified stretched selected-card detail preview images as the next
  concrete UI issue. M32-02 zone placement polish is now complete with Unity
  compile, EditMode `1175/1175`, Windows build `errors=0 warnings=0`, Windows
  player smoke `blockers=[]`, and visual evidence at
  `client/unity/VanguardThaiSim/work/m32_02_digital_reference_ui_visual_evidence_r3/play_table.png`.
  M32-02b letsplay layout research alignment is complete at
  `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/LETSPLAY_LAYOUT_RESEARCH_SUMMARY.md`.
  That research is layout-only: field, card placement, zones, hand, phase/action
  prompts, trigger/check/guard surfaces, online table HUD, and deck-builder
  structure. Story, character, dialogue, campaign UI, copied assets, official
  icons, playmats, code, and extracted data are out of scope. M32-03 PlayTable
  de-dashboard field/HUD pass is complete with EditMode `1178/1178`, Windows
  build `errors=0 warnings=0`, Windows player smoke `blockers=[]`, and visual
  evidence at
  `client/unity/VanguardThaiSim/work/m32_03_de_dashboard_field_hud_visual_evidence_r4/play_table.png`.
  M32-04 playmat slot layout / visual evidence closeout is complete with
  EditMode `1179/1179`, Windows build `errors=0 warnings=0`, Windows player
  smoke `blockers=[]`, and visual evidence at
  `client/unity/VanguardThaiSim/work/m32_04_playmat_slots_visual_evidence_r9/play_table.png`.
  `M32-05` hand strip and compact pile interaction polish is paused by user
  instruction. `M33-01` offline clan combo pairing logic is complete for
  `TD01-TD06`, `BT01-BT09`, and `EB01-EB05`, grouped by clan from
  `data/packs/vanguard_th/cards.sqlite`. It generated
  `outputs/combo_discovery/td01_td06_bt01_bt09_eb01_eb05_clan_combos.json`
  with `1052` cards, `21` clans, and `6846` thresholded candidate pairs.
  `M33-02` expanded the same offline algorithm into era presets for
  Link Joker/Legion, G, V, D, and DZ ranges. It generated per-preset reports
  under `outputs/combo_discovery/` plus
  `outputs/combo_discovery/era_combo_report_summary.json`. `M33-03` generated
  matrix artifacts under `outputs/combo_discovery/`: era summary, group
  candidate counts, group card counts, top pair scores, synergy tags, and a
  matrix summary JSON. `M34-01` added offline deck possibility reports under
  `outputs/deck_possibility/`, including per-preset JSON reports,
  `deck_possibility_summary.csv`, and `deck_possibility_summary.json`. This is
  an offline advisory/math layer only; it must not mutate `GameState`, consume
  RNG, parse live match text, or claim full ability legality. `M34-02` copied
  and reviewed the 2026-06-29 rule/development corpus at
  `outputs/research_2026_06_29_new_chat/`, added
  `docs/specs/cards_and_decks/RULE_CORPUS_DECK_COMBO_PLAN_SPEC.md`. `M34-03`
  added archetype priority ranking under `outputs/archetype_priority/` and is
  Phase A1 of the Hybrid Vertical-Slice Strategy. `M35-A2` selected the first
  target slice as Classic Core / โนว่า เกรปเปอร์ and wrote reports under
  `outputs/target_slice/`. `M35-A3` added minimal deck legality fixtures for
  that selected slice. `M35-A4` reports Phase A ready for Phase B with no
  blocking gaps for semantic tagging. `M35-B1` created the selected-slice
  semantic vocabulary and `M35-B2` extracted advisory semantic tags for `112`
  selected-slice cards with `6` manual-review cards. `M35-B3` created the
  selected-slice requirement/provider model (`94` cards with requirements,
  `93` with providers). `M35-B4` exported a manual review queue of `6` cards
  blocked from playbook promotion/high-confidence compatibility until reviewed.
  `M35-C1` built the selected-slice pair compatibility graph (`112` nodes,
  `3919` directed candidate edges, `363` manual-review-gated edges). `M35-C2`
  added resource conflict detection (`2826` resource-relevant edges, `286`
  shared-resource-pressure edges, `0` missing recovery resource types).
  `M35-C3` added timing compatibility detection (`3876` timing-relevant edges,
  `990` provider-after-consumer-window edges, `804` same-window review edges).
  `M35-C4` added zone/target detection (`3704` zone-relevant edges, `402`
  Vanguard-role-conflict edges, `436` rear-guard-slot-pressure edges).
  `M35-C5` closed Phase C with selected-slice compatibility output (`3919`
  edges, `604` clean M35-D1 candidate edges, `2045` mixed, `907` missing data,
  `363` manual-review-required). The active plan is Phase A-E: foundation
  slice, semantic slice, compatibility slice, deck skeleton/safe playbook seed,
  then scale out. `M35-D1` selected `25` candidate packages from `604` clean
  synergy edges. `M35-D2` produced `25` advisory deck skeleton ratio plans and
  package-local guard/shield profiles from runtime SQLite (`44` package cards
  loaded). `M35-D3` produced `25` human-reviewable combo line explanations
  from clean M35-C5 edges. `M35-D4` exported `1` static-reviewed advisory
  playbook seed and retained `24` rejected lines with review reasons.
  `M35-E1` selected the second slice as `Classic Core / Oracle Think Tank`
  after excluding the first completed Nova Grappler slice. `M35-E2` confirmed
  fixture readiness and Classic Core policy reuse for that second slice.
  `M35-E3` proved the selected-report input contract by running B1-B4 and
  C1-C5 for the second slice (`103` cards, `2660` edges, `259` candidate
  edges). `M35-E4` passed the bot integration gate with `1` reviewed future
  hint candidate, blocked `M35-E3` probe edges from runtime bot use, and kept
  runtime bot integration disabled. `M35-closeout` closed the Hybrid
  Vertical-Slice Strategy and selected `M36` Human-review-assisted deck recipe
  validation. `M36-01` exported a first-slice review packet with `31` review
  items (`1` accepted seed, `24` rejected lines, `6` manual-review cards).
  `M36-02` exported `25` advisory deck recipe drafts (`1` accepted-seed draft,
  `24` rejected-line drafts, `16` slot-gap drafts). `M36-03` validated all
  `25` drafts and found `0` runtime-ready recipes, `0` missing-card recipes,
  and `0` copy-limit violations. `M36-04` confirmed all `25` combo lines have
  their combo cards present in recipe drafts, but `0` lines are promotable.
  `M36-05` confirmed the second slice is ready for future offline recipe work,
  while broader runtime/bot scale-out remains disabled. `M36-closeout` closed
  the queue with `0` runtime-ready recipes, `0` promotable combo lines, `1`
  invalid draft, `24` blocked-by-review recipes, `16` slot-gap recipes, and
  `12` trigger-count mismatch recipes. `M37-01` generated `18` source-backed
  trigger candidate cards and `5` advisory completion packages for accepted
  seed `recipe_003`; runtime promotion remains disabled. `M37-02` recommends
  advisory package `m37_01_pkg_001` / `balanced_classic`, resolving
  `main_deck_size_mismatch`, `trigger_count_mismatch`, and `unfilled_slots`,
  while `grade_profile_review` and `human_acceptance_pending` remain. `M37-03`
  classified `24` rejected lines into `5` support-gap groups for manual mapping.
  `M37-04` generated `5` non-executable manual semantic mapping candidates
  from `49` triage line links. `M37-05` reran validation in memory and moved
  accepted seed `recipe_003` to `validator_passed_pending_human_acceptance` /
  `consistent_pending_human_acceptance`; runtime promotion remains disabled.
  `M37-closeout` decided `recipe_003` remains advisory because human acceptance,
  grade-profile review, and promotion allowance are still open. `M38-01`
  exported a human review packet for `recipe_003` but did not record
  acceptance. `M38-02` generated `2` complete substitution-preview grade
  repair packages that reach `G0=17/G1=14/G2=11/G3=8`. `M38-03` recorded
  human acceptance for `m38_02_grade_pkg_001` plus trigger repair
  `m37_01_pkg_001`; the accepted artifact has `50` cards, `16` triggers, grade
  profile `G0=17/G1=14/G2=11/G3=8`, and `0` blockers. `M38-04` passed all
  `5` runtime fixture gate checks and created an offline runtime/test fixture
  artifact without mutating saved decks, UI, bot, or `GameState`.
  `M38-closeout` closed the queue: `recipe_003` enters offline fixture scope
  only, live runtime deck UI and bot playbooks remain disabled, and next queue
  is `M39`. `M39-01` independently validated the fixture schema and counts
  against SQLite with `0` blockers. `M39-02` exported reviewable count-line
  deck text with `17` importable card lines and `50` total cards without
  mutating saved decks, UI, bot, or `GameState`. `M39-03` generated a
  deterministic `VGTH1.` deck code artifact and Unity headless accepted it with
  `deck_source=deck_code`, `actions_executed=4`, and `event_count=4`.
  `M39-04` allows Oracle Think Tank to enter the offline recipe pipeline only;
  saved deck publication, runtime promotion, and bot/playbook promotion remain
  blocked. `M40-01` exported the second-slice review packet with `6` fixture
  notes, `7` manual-review cards, and `259` candidate edges while keeping all
  runtime/saved deck/UI/bot promotion blocked. `M40-02` exported `25`
  pair-anchored, fixture-scaffolded advisory recipe drafts, all
  quantity-complete at `50` cards with `16` triggers. `M40-03` validated all
  `25` drafts with no missing cards, copy-limit violations, slot gaps, or
  trigger-count mismatches, but `25` remain blocked by manual-review card
  overlap and `0` are runtime-ready. `M40-04` confirmed all `25` drafts contain
  their candidate edge pair cards, with `0` missing pair-card checks and `0`
  promotion-allowed checks because recipe-level manual-review dependencies
  remain open. `M40-05` created `25` advisory repair items and `25`
  grade-profile candidates that can clear manual overlap for human review
  without mutating drafts or allowing runtime/saved deck/UI/bot promotion.
  `M40-closeout` is complete: M40 has `0` runtime-ready recipes, `0`
  promotion-allowed checks, and `25` repair candidates ready for human review.
  `M41-01` exported all `25` repair candidates as a review-only packet without
  recording acceptance. `M41-02` records acceptance for `m40_recipe_001` but
  does not declare the recipe valid or enable runtime promotion. `M41-03`
  validates the repaired recipe and finds trigger count `2/16`; grade profile
  and manual-review overlap are cleared, but M41-04 was blocked until the
  repair loop. `M41-repair`
  exports `3` complete trigger/profile repair candidates. `M41-repair-accept`
  records acceptance of balanced package `m41_repair_pkg_001`, and
  `M41-repair-validate` passes with `50` cards, `16` triggers, target grade
  profile, manual-review overlap cleared, and `0` blockers. `M41-04` passes
  all `6` promotion-gate checks and creates an offline runtime/test fixture
  artifact without saved deck/UI/bot/GameState mutation. `M41-closeout`
  records Oracle Think Tank in offline fixture scope only and selects `M42`.
  `M42-01` validates the second fixture schema with `0` blockers. `M42-02`
  exports reviewable count-line deck text with `15` importable card lines and
  no saved deck/UI/bot/GameState mutation. `M42-03` confirms Unity headless
  accepts the generated deck code with actions `4` and events `4`. `M42-04`
  opens the third-slice offline pipeline with `2` passed fixtures and `5`
  candidates. `M43-01` selects `เบอร์มิวด้า ไทรแองเกิล` for offline analysis
  only. `M43-02` confirms third-slice fixture/format readiness with `127`
  source-backed cards, grade 0-3 coverage, trigger capacity `84`, and no
  runtime/UI/saved-deck/bot/GameState mutation. `M43-03` passes the
  semantic/compatibility probe with `127` semantic cards, `61` manual-review
  cards, `4835` pair edges, and `109` candidate edges. `M43-04` opens the
  offline M44 queue with blockers `0`, while runtime/UI/saved-deck/bot
  promotion remains disabled. `M44-01` fixture scaffold is done with
  source-backed validator policy and blockers `0`. `M44-02` review packet is
  done with `171` review items. `M44-03` recipe draft model is done with `25`
  quantity-complete advisory drafts, `25` manual-review overlap blockers, and
  no runtime/UI/saved-deck/bot/GameState mutation. `M44-04` recipe validator
  is done with `25` validated drafts, `0` runtime-ready recipes, and `25`
  manual-review overlap blockers. `M44-05` combo-to-recipe consistency is done
  with candidate pair cards present in all `25` drafts, `0` missing pair-card
  checks, and `0` promotion-allowed checks. `M44-06` blocker repair candidates
  is done with `25` advisory repair items, `25` complete manual repair
  packages, and `25` complete grade-profile repair packages. `M44-closeout` is
  done with `m44_complete=True`, runtime-ready recipe unavailable, and next
  queue `M45`. `M45-01` third-slice human repair review packet is done with
  `25` review items, `25` complete manual repair packages, `25` complete
  grade-profile candidates, runtime promotion disabled, and
  `ready_for_m45_02=True`. `M45-02` third-slice human-accepted repair artifact
  is done with accepted recipe `m44_recipe_001`, `2` source grade-package
  conflicts detected after manual substitution, recomputed combined grade
  repair, a `50`-card preview, grade counts `0:17 / 1:14 / 2:11 / 3:8`, and
  `ready_for_m45_03=True`. `M45-03` repaired recipe validation rerun is done
  with `validator_passed=1`, `runtime_ready=1`, no missing/copy-limit/slot/
  trigger/manual-overlap/grade-profile issues, and `ready_for_m45_04=True`.
  `M45-04` runtime fixture promotion gate is done with
  `promotion_allowed=True`, `7/7` gate checks passed, and an offline
  runtime/test fixture artifact created without saved-deck/UI/bot/GameState
  mutation. `M45-closeout` is done with `m45_complete=True`, third runtime
  fixture available, and next queue `M46`. `M46-01` third fixture schema
  validator is done with `schema_valid=True`, blockers `0`, main deck count
  `50`, trigger profile `Critical=4 / Draw=4 / Heal=4 / Stand=4`, grade
  profile `0:17 / 1:14 / 2:11 / 3:8`, and runtime/UI/saved-deck/bot/GameState
  mutation disabled. `M46-02` third fixture deck text exporter is done with
  `export_ready=True`, blockers `0`, `15` exported card lines, review-only
  count-line deck text generated, and runtime/UI/saved-deck/bot/GameState
  mutation disabled. `M46-03` third fixture headless load smoke is done with
  `offline_load_ready=True`, `unity_headless_smoke_passed=True`,
  `deck_source=deck_code`, `actions_executed=4`, `event_count=4`, blockers
  `0`, and runtime/UI/saved-deck/bot/GameState mutation disabled. `M46-04`
  three-fixture scale decision is done with `ready_for_m47=True`,
  `passed_fixture_count=3`, `failed_fixture_count=0`, `candidate_count=5`,
  fourth-slice offline pipeline allowed, and runtime/UI/saved-deck/bot/
  GameState mutation disabled. Current target is `M47-01` fourth target slice
  selection. `M47-01` is done, selecting `รอยัล พาลาดิน` /
  `g_series_first` for offline analysis only, with no recipe/runtime/UI/
  saved-deck/bot/GameState mutation. `M47-02` fourth-slice fixture/format
  readiness is done with source-backed card count `71`, trigger gap `Heal`,
  `all_fixture_expectations_met=False`, `repair_required=True`, and no recipe/
  runtime/UI/saved-deck/bot/GameState mutation. Current target is `M47-repair`
  fourth-slice readiness blocker repair. `M47-repair` is done, finding
  same-group Heal triggers outside the selected scope and recommending
  `review_same_group_source_expansion` without card data/runtime/UI/saved-deck/
  bot/GameState mutation. `M47-repair-expand-scope` is done,
  recommending `g_era_heal_expansion`, expanded source count `190`, and trigger
  gaps cleared without applying the expansion or mutating card data/runtime/UI/
  saved-deck/bot/GameState. `M47-repair-apply-scope` is done, applying that
  expansion to an offline fixture pipeline scope artifact with blockers `0` and
  no card data/runtime/UI/saved-deck/bot/GameState mutation. `M47-03` is done
  with semantic cards `190`, pair graph edges `14150`, candidate edges `785`,
  all stage readiness flags passing, and no card data/runtime/UI/saved-deck/bot/
  GameState mutation. `M47-04` is done, allowing the offline M48 recipe
  pipeline with blockers `0`, fixture scaffold required, and no card data/
  recipe/runtime/UI/saved-deck/bot/GameState mutation. `M48-01` is done with
  scaffold ready, blockers `0`, Grade 4/G Zone deferred as advisory/manual-review
  only, and no card data/recipe/runtime/UI/saved-deck/bot/GameState mutation.
  `M48-02` is done with `1` fixture scaffold item, `15` manual-review cards,
  `785` candidate edges, `801` total review items, and no card data/recipe/
  runtime/UI/saved-deck/bot/GameState mutation. `M48-03` is done with `25`
  advisory drafts, `25` quantity-complete drafts, skipped `35` trigger/G4/
  missing candidate edges, and no card data/runtime/UI/saved-deck/bot/GameState
  mutation. `M48-04` is done with `25` drafts validated, runtime-ready recipes
  `0`, manual-review blocked recipes `25`, Grade 4 main-deck violations `0`, and
  no card data/runtime/UI/saved-deck/bot/GameState mutation. `M48-05` is done
  with `25` consistency checks, pair cards present `25`, promotion allowed `0`,
  G Zone deferred checks `25`, and no card data/runtime/UI/saved-deck/bot/
  GameState mutation. `M48-06` is done with `25` recipes reviewed, complete
  manual repair candidates `25`, complete grade-profile candidates `24`, G Zone
  deferred recipes `25`, unexpected structural blockers `0`, and no card data/
  runtime/UI/saved-deck/bot/GameState mutation. `M48-closeout` is done with M48
  complete `true`, runtime-ready recipe available `false`, human/G-Zone review
  allowed `true`, G Zone deferred recipes `25`, next queue `M49`, and no card
  data/runtime/UI/saved-deck/bot/GameState mutation. `M49-01` is done with `25`
  review items, `25` G Zone decision items, ready_for_m49_02 `true`, and no card
  data/runtime/UI/saved-deck/bot/GameState mutation. `M49-02` is done with
  selected option `main_deck_only_for_current_windows_fixture`, `25` decision
  items, G Zone runtime `false`, Stride runtime `false`, runtime promotion
  `false`, and ready_for_m49_03 `true`. `M49-03` is done with accepted
  `m48_recipe_001`, main deck `50`, grade profile `17/14/11/8`, repair issues
  `0`, G Zone runtime `false`, Stride runtime `false`, runtime promotion
  `false`, and ready_for_m49_04 `true`. `M49-04` is done with
  validator_passed `1`, runtime_ready `1`, issue_counts `{}`, G Zone runtime
  `false`, Stride runtime `false`, runtime promotion `false`, and
  ready_for_m49_05 `true`. `M49-05` is done with promotion_allowed `true`,
  passed checks `8`, fixture created `true`, G Zone runtime `false`, Stride
  runtime `false`, and saved-deck/UI/bot/GameState mutation disabled.
  `M49-closeout` is done with M49 complete `true`, fourth runtime fixture
  available `true`, next queue `M50`, G Zone runtime `false`, Stride runtime
  `false`, and saved-deck/UI/bot/GameState mutation disabled. `M50-01` is done
  with schema valid `true`, blockers `0`, main deck `50`, unique cards `14`,
  trigger profile `4/4/4/4`, grade profile `17/14/11/8`, G Zone runtime
  `false`, Stride runtime `false`, and ready_for_m50_02 `true`. `M50-02` is
  done with export ready `true`, blockers `0`, main deck `50`, exported card
  lines `14`, G section comment-only, and ready_for_m50_03 `true`. `M50-03` is
  done with offline load ready `true`, deck code created `true`, Unity headless
  accepted `deck_source=deck_code`, actions/events `4/4`, G Zone count `0`,
  and ready_for_m50_04 `true`. `M50-04` is done with fixture evidence `4`,
  passed fixtures `4`, failed fixtures `0`, candidates `5`, fifth-slice offline
  pipeline allowed `true`, G Zone runtime `false`, Stride runtime `false`, and
  ready_for_m51 `true`. `M51-01` is done, selecting `โกลด์ พาลาดิน` /
  `link_joker_legion_mate` for offline analysis only with runtime/UI/saved-deck
  / bot / GameState mutation disabled. `M51-02` is done with source_card_count
  `106`, trigger capacity `36`, non-trigger capacity `388`, trigger gaps `[]`,
  fixture expectations met `true`, semantic probe ready `true`, and the same
  runtime/UI/saved-deck/bot/GameState boundary preserved. `M51-03` is done with
  semantic cards `106`, manual-review cards `4`, pair graph edges `3075`,
  candidate edges `142`, all stage readiness passed `true`, and the same
  runtime/UI/saved-deck/bot/GameState boundary preserved. `M51-04` is done with
  offline recipe pipeline allowed `true`, blockers `0`, fixture scaffold
  required `true`, ready_for_m52 `true`, and the same runtime/UI/saved-deck/
  bot/GameState boundary preserved. `M52-01` is done with source-backed cards
  `106`, trigger capacity `36`, candidate edges `142`, scaffold_ready `true`,
  blockers `0`, ready_for_m52_02 `true`, and the same runtime/UI/saved-deck/
  bot/GameState boundary preserved. `M52-02` is done with fixture scaffold
  items `1`, manual-review cards `4`, candidate edges `142`, total review
  items `147`, ready_for_m52_03 `true`, and the same runtime/UI/saved-deck/bot/
  GameState boundary preserved. `M52-03` is done with `25` advisory recipe
  drafts, `25` quantity-complete drafts, trigger/missing skipped edges `0`,
  manual-overlap recipes `0`, ready_for_m52_04 `true`, and the same runtime/UI/
  saved-deck/bot/GameState boundary preserved. `M52-04` is done with `25`
  recipes validated, `0` runtime-ready recipes, `25` validator-passed pending
  human selection, `0` invalid drafts, `0` missing/copy/slot/trigger/manual
  blockers, `25` grade-profile review recipes, and ready_for_m52_05 `true`.
  `M52-05` is done with `25` consistency checks, pair cards present `25`,
  missing pair-card checks `0`, promotion allowed `0`, status
  `consistent_pending_human_selection=25`, and ready_for_m52_06 `true`.
  `M52-06` is done with `25` repair candidates, `25` complete grade-profile
  candidates, human selection required `25`, unexpected structural blockers
  `0`, and ready_for_m52_closeout `true`. `M52-closeout` is done: M52 complete
  `true`, runtime-ready recipe available `false`, human selection review
  allowed `true`, and next queue `M53`. `M53-01` is done with `25` review
  items, `25` complete grade-profile candidates, human selection required
  `25`, and ready_for_m53_02 `true`. `M53-02` is done with selected review item
  `m53_01_m52_recipe_001_repair_review`, recipe `m52_recipe_001`, grade package
  `m52_recipe_001_grade_profile_pkg_001`, and ready_for_m53_03 `true`.
  `M53-03` is done with human acceptance recorded, repaired main deck count
  `50`, repair application issues `0`, and ready_for_m53_04 `true`. `M53-04`
  is done with validation `validator_passed`, consistency
  `consistent_validator_passed`, runtime-ready recipes `1`, blockers `0`, and
  ready_for_m53_05 `true`. `M53-05` is done with promotion_allowed `true`,
  passed checks `5`, failed checks `0`, and offline fixture
  `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`.
  `M53-closeout` is done with m53_complete `true`, fifth runtime fixture
  available `true`, next queue `M54`, and saved-deck/UI/bot/GameState mutation
  disabled. `M54-01` is done with schema_valid `true`, blockers `0`, main deck
  `50`, unique card count `16`, trigger profile `4/4/4/4`, grade profile
  `17/14/11/8`, ready_for_m54_02 `true`, targeted tests `8/8`, and full
  Python unittest discovery `1008/1008`. `M54-02` is done with export_ready
  `true`, blockers `0`, main deck `50`, exported card lines `16`, review-only
  deck text `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt`,
  ready_for_m54_03 `true`, targeted tests `7/7`, and full Python unittest
  discovery `1015/1015`. `M54-03` is done with offline load ready `true`,
  Unity headless accepted `true`, deck source `deck_code`, actions/events
  `4/4`, blockers `0`, targeted tests `9/9`, and full Python unittest
  discovery `1024/1024`. `M54-04` is done with five fixtures passed, failed
  fixtures `0`, candidate count `5`, sixth-slice offline pipeline allowed
  `true`, targeted tests `8/8`, and full Python unittest discovery `1032/1032`.
  `M55-01` is done with selected group `ชาโดว์ พาลาดิน`, era `g_next_z`,
  selected rank `4`, candidate count `5`, ready_for_m55_02 `true`, targeted
  tests `7/7`, and full Python unittest discovery `1039/1039`. Current target
  is `M55-02` sixth-slice fixture/format readiness. `M55-02` is done with
  source cards `77`, grade profile `19/20/16/11/11`, trigger profile
  `Critical=4/Draw=4/Heal=2/Stand=2`, semantic probe ready `true`, targeted
  tests `8/8`, and full Python unittest discovery `1047/1047`. Current target
  is `M55-03` sixth-slice semantic/compatibility probe. `M55-03` is done with
  semantic cards `77`, manual-review cards `11`, pair graph edges `2069`,
  candidate edges `70`, all stage readiness `true`, targeted tests `8/8`, and
  full Python unittest discovery `1055/1055`. `M55-04` is done with offline
  recipe pipeline allowed `true`, blockers `0`, ready_for_m56 `true`, targeted
  tests `9/9`, and full Python unittest discovery `1064/1064`. `M56-01` is
  done with scaffold_ready `true`, blockers `0`, source cards `77`, Grade 4
  cards advisory only until G Zone support, targeted tests `9/9`, and full
  Python unittest discovery `1073/1073`. `M56-02` is done with review items
  `82`, manual-review cards `11`, candidate edges `70`, ready_for_m56_03
  `true`, targeted tests `8/8`, and full Python unittest discovery
  `1081/1081`. `M56-03` is done with recipe drafts `12`,
  quantity-complete recipes `12`, skipped trigger/Grade 4/missing edges `58`,
  manual-overlap recipes `12`, targeted tests `9/9`, and full Python unittest
  discovery `1090/1090`. `M56-04` is done with validated drafts `12`,
  runtime-ready recipes `0`, manual-review blocked recipes `12`, missing/copy/
  slot/trigger/Grade 4 blockers `0`, targeted tests `7/7`, and full Python
  unittest discovery `1097/1097`. `M56-05` is done with consistency checks
  `12`, pair cards present `12`, missing pair-card checks `0`, promotion
  allowed `0`, recipe manual dependencies `12`, G Zone deferred `12`, targeted
  tests `6/6`, and full Python unittest discovery `1103/1103`. `M56-06` is
  done with repair items `12`, manual repair complete `12`, grade repair
  complete `12`, G Zone deferred `12`, ready for human repair review `12`,
  targeted tests `8/8`, and full Python unittest discovery `1111/1111`.
  `M56-closeout` is done with M56 complete `true`, runtime-ready recipe
  available `false`, next queue `M57`, manual review blocked `12`, G Zone
  deferred `12`, targeted tests `9/9`, and full Python unittest discovery
  `1120/1120`. `M57-01` is done with review items `12`, complete manual repairs
  `12`, complete grade repairs `12`, G Zone deferred items `12`, targeted
  tests `10/10`, and full Python unittest discovery `1130/1130`. Current
  target is `M57-02` sixth-slice human-selected recipe artifact. M57-02
  spec/tool/tests are scaffolded and verified (`9/9` targeted, `1139/1139`
  full Python), but the real selected artifact still requires an explicit
  valid M57 review item id. M57-03 spec/tool/tests are scaffolded and verified
  (`7/7` targeted, `1146/1146` full Python); it applies manual substitutions
  before recomputing grade repair, but its real output remains gated on M57-02
  and explicit acceptance text. M57-04 spec/tool/tests are scaffolded and
  verified (`16/16` targeted across M57-03/M57-04, `1155/1155` full Python);
  it records explicit G Zone / Stride boundary decisions while keeping G Zone,
  Stride, runtime fixture, saved deck/UI publication, bot/playbook, and
  `GameState` mutation disabled. M57-05 spec/tool/tests are scaffolded and
  verified (`18/18` targeted across M57-04/M57-05, `1164/1164` full Python);
  it validates the repaired main-deck preview and combo consistency after the
  M57-04 boundary while keeping runtime fixture promotion deferred to M57-06.
  M57-06 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1173/1173` full Python); it can create only an offline runtime/test fixture
  after M57-03 accepted rows and M57-05 validation exist, while saved deck/UI,
  bot/playbook, G Zone/Stride runtime, and `GameState` mutation remain
  disabled. M57-closeout spec/tool/tests are scaffolded and verified (`6/6`
  targeted, `1179/1179` full Python); it routes a completed sixth fixture to
  M58 consumption/scale work, but the real closeout output remains gated on
  M57-06 output. M58-01 spec/tool/tests are scaffolded and verified (`11/11`
  targeted, `1190/1190` full Python) against an in-memory M57-06 fixture; the
  real M58-01 report remains gated on the real M57-06 fixture file. M58-02
  spec/tool/tests are scaffolded and verified (`7/7` targeted, `1197/1197`
  full Python) against an in-memory fixture plus M58-01 validation report; the
  real M58-02 artifacts remain gated on real M57-06 and M58-01 files.
  M58-03 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1206/1206` full Python) against in-memory M57-06/M58-01/M58-02 artifacts;
  offline deck-code smoke is ready in scaffold, while real outputs and Unity
  headless evidence remain gated on real upstream files. M58-04 spec/tool/tests
  are scaffolded and verified (`8/8` targeted, `1214/1214` full Python) using
  the first five real smoke reports plus in-memory sixth fixture smoke; the
  real scale decision remains gated on real M58-03 Unity evidence. M59-01
  spec/tool/tests are scaffolded and verified (`7/7` targeted, `1221/1221`
  full Python) using an in-memory M58-04 scale decision; the real seventh
  target selection remains gated on real M58-04 output. M59-02 spec/tool/tests
  are scaffolded and verified (`9/9` targeted, `1230/1230` full Python) using
  an in-memory M59-01 selection; selected scaffold target `เนโอ เนคต้า` /
  `g_series_first` has source cards `78`, grade profile `17/23/18/12/8`,
  trigger capacity `48`, no trigger gaps, and readiness for M59-03. The real
  readiness report remains gated on real M59-01 output. M59-03 spec/tool/tests
  are scaffolded and verified (`8/8` targeted, `1238/1238` full Python) using
  in-memory M59-01/M59-02 reports; the semantic probe reports semantic cards
  `78`, manual-review cards `10`, pair graph edges `2885`, candidate edges
  `107`, and readiness for M59-04. The real probe report remains gated on real
  M59-01 and M59-02 outputs. M59-04 spec/tool/tests are scaffolded and verified
  (`9/9` targeted, `1247/1247` full Python) using in-memory M59-02/M59-03
  reports; the gate allows M60 offline recipe pipeline only with source cards
  `78`, semantic cards `78`, manual-review cards `10`, pair graph edges
  `2885`, candidate edges `107`, blocking issues `0`, and readiness for M60.
  The real gate report remains gated on real M59-02 and M59-03 outputs. M60-01
  spec/tool/tests are scaffolded and verified (`9/9` targeted, `1256/1256`
  full Python) using in-memory M59-02/M59-03/M59-04 reports; the fixture
  scaffold is ready for M60-02 with source cards `78`, grade profile
  `17/23/18/12/8`, trigger profile `Critical=5`, `Draw=2`, `Heal=2`,
  `Stand=3`, candidate edges `107`, and manual-review cards `10`. The real
  scaffold report remains gated on real M59-02, M59-03, and M59-04 outputs.
  M60-02 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1265/1265` full Python) using in-memory M59-01/M59-02/M59-03/M59-04/M60-01
  reports; the review packet is ready for M60-03 with fixture scaffold items
  `1`, manual-review cards `10`, candidate edges `107`, total review items
  `118`, and runtime/UI/bot/G Zone/Stride/GameState mutation disabled. The real
  review packet remains gated on real upstream outputs. M60-03 spec/tool/tests
  are scaffolded and verified (`9/9` targeted, `1274/1274` full Python) using
  in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02 reports; the draft model
  is ready for M60-04 with candidate edge inputs `107`, skipped trigger/Grade
  4/missing edges `84`, advisory recipe drafts `23`, quantity-complete recipes
  `23`, manual-overlap recipes `23`, fixture scaffold cards `14`, fixture
  scaffold total cards `50`, and runtime/UI/bot/G Zone/Stride/GameState
  mutation disabled. The real draft artifacts remain gated on real upstream
  outputs. M60-04 spec/tool/tests are scaffolded and verified (`7/7` targeted,
  `1281/1281` full Python) using in-memory M59-01/M59-02/M59-03/M59-04/
  M60-01/M60-02/M60-03 reports; the validator is ready for M60-05 with
  validated drafts `23`, runtime-ready recipes `0`, manual-review blockers
  `23`, missing/copy/slot/trigger/Grade 4 main-deck blockers `0`,
  grade-profile review recipes `21`, G Zone deferred recipes `23`, Bloom/token
  deferred recipes `23`, and runtime/UI/bot/GameState mutation disabled. The
  real validation artifacts remain gated on real upstream outputs. M60-05
  spec/tool/tests are scaffolded and verified (`6/6` targeted, `1287/1287`
  full Python) using in-memory M59-01/M59-02/M59-03/M59-04/M60-01/M60-02/
  M60-03/M60-04 reports; the consistency check is ready for M60-06 with
  consistency checks `23`, pair cards present `23`, missing pair checks `0`,
  recipe manual dependencies `23`, G Zone deferred checks `23`, Bloom/token
  deferred checks `23`, promotion_allowed `0`, and runtime/UI/bot/GameState
  mutation disabled. The real consistency artifacts remain gated on real
  upstream outputs. M60-06 spec/tool/tests are scaffolded and verified (`8/8`
  targeted, `1295/1295` full Python) using in-memory M59-01/M59-02/M59-03/
  M59-04/M60-01/M60-02/M60-03/M60-04/M60-05 reports; the repair candidate
  report is ready for M60-closeout with repair items `23`, complete manual
  repair previews `23`, complete grade-profile repair candidates `21`, G Zone
  deferred packages `23`, Bloom/token deferred packages `23`, unexpected
  structural blockers `0`, human repair review ready `23`, and runtime/UI/bot/
  GameState mutation disabled. The real repair artifacts remain gated on real
  upstream outputs. M60-closeout spec/tool/tests are scaffolded and verified
  (`9/9` targeted, `1304/1304` full Python) using in-memory M59-01/M59-02/
  M59-03/M59-04/M60-01/M60-02/M60-03/M60-04/M60-05/M60-06 reports; it selects
  next queue `M61`, reports m60_scaffold_complete `true`,
  real_artifacts_available `false`, runtime_ready_recipe_available `false`,
  human_selection_review_allowed `true`, G Zone deferred recipes `23`,
  Bloom/token deferred recipes `23`, and runtime/UI/bot/GameState mutation
  disabled. The real closeout artifacts remain gated on real upstream outputs.
  M61-01 spec/tool/tests are scaffolded and verified (`10/10` targeted,
  `1314/1314` full Python) using in-memory M59-01/M59-02/M59-03/M59-04/
  M60-01/M60-02/M60-03/M60-04/M60-05/M60-06/M60-closeout reports; it exports
  review items `23`, complete manual repair candidates `23`, complete
  grade-profile candidates `21`, grade-profile-not-needed items `2`, G Zone
  deferred items `23`, Bloom/token deferred items `23`, ready_for_m61_02
  `true`, and runtime/UI/bot/GameState mutation disabled. The real M61-01
  artifacts remain gated on real upstream outputs.
  M61-02 spec/tool/tests are scaffolded and verified (`10/10` targeted,
  `1324/1324` full Python) using an explicit in-memory M61-01 review-item
  selection; it records exactly one selected recipe, preserves selected
  pair/manual/grade/G Zone/Bloom context,
  records no acceptance or G Zone/Bloom decision, keeps runtime/UI/bot/GameState
  mutation disabled, and reports ready_for_m61_03 `true` for valid selected
  items. Real M61-02 artifacts remain gated on the real M61-01 packet plus
  explicit human `review_item_id` and `selection_text`.
  M61-03 spec/tool/tests are scaffolded and verified (`8/8` targeted,
  `1332/1332` full Python) using an explicit in-memory M61-02 selection and
  acceptance text; it records human
  acceptance, applies selected manual repair in memory, recomputes grade repair
  after manual substitution, produces a 50-card repaired preview with target
  grade profile for valid selected items, leaves G Zone/Stride and Bloom/token
  decisions deferred to M61-04, declares no validation/runtime promotion, and
  does not mutate runtime/UI/bot/GameState. Real M61-03 artifacts remain gated
  on real M61-02 output plus explicit `acceptance_text`.
  M61-04 spec/tool/tests are scaffolded and verified (`10/10` targeted,
  `1342/1342` full Python) using an explicit in-memory M61-03 accepted artifact
  and explicit G Zone / Bloom-token options; it records both boundary decisions,
  opens M61-05 only
  when both selected options allow main-deck/manual-semantic validation, keeps
  G Zone/Stride/Bloom-token runtime disabled, declares no validation/runtime
  promotion, and does not mutate runtime/UI/bot/GameState. Real M61-04
  artifacts remain gated on real M61-03 output plus explicit `g_zone_option`
  and `bloom_token_option`.
- Latest bot/combo milestone: `M14-10` bounded readiness-gated one-ply advanced search prototype over legal actions and snapshot simulation.
- Latest custom pack milestone: `M15-01` backward-compatible custom pack v2 schema envelope with capability/dependency validation and v2 template.
- Latest custom ability-data milestone: `M15-02` custom pack v2 ability file validation with same-pack card id checks and manifest ability hash metadata.
- Latest pack UI/status milestone: `M15-03` read-only runtime pack validation status model and Card Browser status surface.
- Latest pack manager milestone: `M15-04` pure enable/disable registry helper with clone-only mutation and no active-pack switching.
- Latest custom format milestone: `M15-05` custom format profile data model referencing existing RuleSet profiles.
- Latest format flag milestone: `M15-08` Premium custom format preset delegating to core Premium flags.
- Latest custom format sandbox milestone: `M15-09` read-only format/alias/pack preview resolving to cloned RuleSet profile summaries.
- Latest custom pack/format closeout milestone: `M15-10` validation tests and closeout doc with Python 31/31 and Unity 762/762 verification.
- Latest UI/mobile milestone: `M16-01` compact pending AUTO PlayTable panel formatter with hidden-source-safe output.
- Latest trigger UI milestone: `M16-02` compact trigger PlayTable panel formatter with checked-card-safe latest summaries.
- Latest manual resolution UI milestone: `M16-03` compact manual resolution PlayTable panel formatter with hidden-source-safe output.
- Latest event/replay UI milestone: `M16-04` compact event/replay PlayTable panel formatter with resource instance id redaction.
- Latest online status UI milestone: `M16-05` polished PlayTable online status/cursor key-value summary with reconnect segment.
- Latest deck builder UI milestone: `M16-06` pure Deck Builder filter/status formatter wired into card pool and deck panels.
- Latest card browser search UI milestone: `M16-07` Thai/English no-result search feedback formatter wired into detail panel.
- Latest image UI milestone: `M16-08` deterministic Card Browser fallback image status with cache fallback detection.
- Latest Android UI QA milestone: `M16-09` responsive layout QA verifier with 48px touch-target gate for Android profiles.
- Latest client smoke milestone: `M16-10` Windows/Android smoke flow runner with no blockers.
- Latest headless milestone: `M17-01` deterministic headless CLI runner with default generated deck and minimal result JSON.
- Latest headless input milestone: `M17-02` bounded seed/ruleset/deck-code/result-path CLI input.
- Latest headless output milestone: `M17-03` result/replay artifact output with card instance id redaction.
- Latest headless batch milestone: `M17-04` bounded batch runner with sequential seeds and run-count guard.
- Latest headless dataset milestone: `M17-05` dataset export schema v1 over headless batch summaries.
- Latest headless research API milestone: `M17-06` observation/action/reward contract with sanitized action masks.
- Latest headless profiling milestone: `M17-07` bounded performance profiling harness.
- Latest packed-state milestone: `M17-08` decision gate with readable `GameState` source-of-truth policy.
- Latest distributed worker spec milestone: `M17-09` spec artifact and validator.
- Latest distributed worker milestone: `M17-10` local bounded worker prototype.
- Latest CI milestone: `M18-01` Python unit-test workflow.
- Latest CI data milestone: `M18-02` data-validation workflow.
- Latest CI Unity milestone: `M18-03` self-hosted Unity compile workflow.
- Latest CI Unity test milestone: `M18-04` self-hosted Unity EditMode workflow.
- Latest release milestone: `M18-10` Release candidate checklist.
- Latest artifact hotfix: Windows player build now copies
  `data/packs/vanguard_th` beside the `.exe`, resolves nested artifact pack
  paths, keeps Active Input Handling set to `Both` for current UGUI input, and
  passed normal launch log smoke plus `VanguardThaiSim.exe -vanguardPlayerSmoke`
  with card browser, deck, Play Table, and layout checks.
- Latest PlayTable Windows UX spec: `M21-01` is complete at
  `docs/PLAYTABLE_WINDOWS_UX_V2_SPEC.md`.
- Latest PlayTable board-first implementation: `M21-02` is complete at
  `docs/history/M21_02_PLAYTABLE_BOARD_FIRST_CLOSEOUT.md`. Desktop PlayTable toolbar
  height and side-panel width were reduced, board/table layout weight was
  increased, Windows board-first QA was added, and final Windows player smoke
  passed with `blockers=[]`.
- Latest PlayTable zone status implementation: `M21-03` is complete at
  `docs/history/M21_03_PLAYTABLE_ZONE_STATUS_CLOSEOUT.md`. The PlayTable now has a
  read-only Zone Status panel for deck, hand, drop, damage, bind, order, ride
  deck, and trigger zone counts.
- Latest PlayTable hand/preview implementation: `M21-04` is complete at
  `docs/history/M21_04_HAND_STRIP_SELECTED_PREVIEW_CLOSEOUT.md`. The hand strip
  now has compact player-facing labels and selected-card preview shows readable
  card id/name/zone/type/stats/skill text plus legal-action hints without
  leaking hidden card detail.
- Latest PlayTable Soul status/ledger implementation: `M21-04b` is complete at
  `docs/history/M21_04B_SOUL_STATUS_LEDGER_CLOSEOUT.md`. PlayTable status now
  shows real Soul/G Zone/Guardian counts, the resource row has a Soul panel, and
  ResourceLedger derives available Soul from `PlayerGameState.soul`.
- Latest ride/Soul resource command implementation: `M21-04d` is complete at
  `docs/history/M21_04D_RIDE_SOUL_RESOURCE_COMMAND_CLOSEOUT.md`. MoveCard into
  Vanguard now moves displaced Vanguard cards to Soul with undo/replay support,
  and structured SoulCharge/SoulBlast execute through RulesCore.
- Latest board thumbnail implementation: `M21-04c` is complete at
  `docs/history/M21_04C_BOARD_THUMBNAIL_CLOSEOUT.md`. PlayTable now renders
  compact vanguard/rear-guard card-face buttons with safe lazy thumbnails or
  readable fallback text.
- Latest action availability implementation: `M21-05a` is complete at
  `docs/history/M21_05A_ACTION_AVAILABILITY_CLOSEOUT.md`. PlayTable primary
  and move buttons now enable/disable from phase, selected card, legal actions,
  and local/online mode, with Stand/Ride phase buttons added.
- Latest Check/Guard surface implementation: `M21-05a2` is complete at
  `docs/history/M21_05A2_CHECK_GUARD_SURFACE_CLOSEOUT.md`. PlayTable now has
  player-facing Check and Guard buttons backed by RulesCore.
- Latest trigger-check source split implementation: `M21-05a3` is complete at
  `docs/history/M21_05A3_TRIGGER_SOURCE_SPLIT_CLOSEOUT.md`. Legal actions,
  committed events, PlayTable buttons, and event/replay text now distinguish
  Drive vs Damage trigger checks without leaking checked card instance ids.
- Latest attack-vanguard surface implementation: `M21-05a4` is complete at
  `docs/history/M21_05A4_ATTACK_VANGUARD_SURFACE_CLOSEOUT.md`. PlayTable now
  has a player-facing `Atk VG` shortcut that lets a selected local vanguard or
  rear-guard declare a legal attack against the opponent vanguard through the
  RulesCore/legal-action facade, with event/replay text that avoids private
  instance ids. Verification passed Unity compile, EditMode `945/945`,
  Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest attack target selection implementation: `M21-05a5` is complete at
  `docs/history/M21_05A5_ATTACK_TARGET_SELECTION_CLOSEOUT.md`. PlayTable now
  shows compact public opponent Vanguard/Rear-guard target surfaces, stores
  target selection separately from the local selected card, and exposes
  `Atk Target` only when the selected attacker and target match a legal
  `DeclareAttack`. Verification passed Unity compile, EditMode `949/949`,
  Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest Battle Flow status implementation: `M21-05a6` is complete at
  `docs/history/M21_05A6_BATTLE_FLOW_STATUS_CLOSEOUT.md`. PlayTable now shows
  player-facing guidance for ready-to-attack, attack-declared, guard-placed,
  and trigger-checked states from the display event log without leaking private
  instance ids. Verification passed Unity compile, EditMode `955/955`,
  Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest Manual Note surface implementation: `M21-05a7` is complete at
  `docs/history/M21_05A7_MANUAL_NOTE_SURFACE_CLOSEOUT.md`. PlayTable now has a
  local `Note` action and Manual Notes panel for manual-session reminders.
  Notes do not mutate `GameState`, do not append to `GameState.event_log`, and
  are not published to Photon. Verification passed Unity compile, EditMode
  `960/960`, Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest setup readiness implementation: `M21-05b1` is complete at
  `docs/history/M21_05B1_SETUP_READINESS_GUARD_CLOSEOUT.md`. Home Solo Play and
  Deck Builder Start Game now require `DeckValidator.IsPlayable` before
  PlayTable creates a game state. Verification passed Unity compile, EditMode
  `965/965`, Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest first Vanguard setup implementation: `M21-05b2` is complete at
  `docs/history/M21_05B2_FIRST_VANGUARD_SETUP_CLOSEOUT.md`. PlayTable now shows
  a Ride Deck panel and allows a selected ride-deck card to move to Vanguard in
  `Mulligan` through RulesCore when the Vanguard circle is empty. Verification
  passed Unity compile, EditMode `969/969`, Windows build `errors=0 warnings=0`,
  and Windows player smoke `blockers=[]`.
- Latest selected-card mulligan implementation: `M21-05b3` is complete at
  `docs/history/M21_05B3_MULLIGAN_SELECTED_CLOSEOUT.md`. PlayTable now has a
  `Mulligan` action that returns one selected hand card and redraws through
  `RulesCore`. Verification passed Unity compile, EditMode `972/972`, Windows
  build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
- Latest phase action implementation: `M21-05b4` is complete at
  `docs/history/M21_05B4_PHASE_ACTIONS_CLOSEOUT.md`. Legal action generation
  now includes `StandAndDraw` and `Ride`, so PlayTable phase buttons execute
  through legal actions after button-state refresh. Verification passed Unity
  compile, EditMode `973/973`, Windows build `errors=0 warnings=0`, and Windows
  player smoke `blockers=[]`.
- Latest setup status guidance implementation: `M21-05b5` is complete at
  `docs/history/M21_05B5_SETUP_STATUS_GUIDANCE_CLOSEOUT.md`. PlayTable now has
  a read-only `Setup` panel that guides first Vanguard placement, selected-card
  mulligan, and pressing `Stand` to begin without leaking private card ids or
  mutating `GameState`. Verification passed Unity compile, EditMode `981/981`,
  Windows build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
- Latest Advanced debug surface cleanup: `M21-06` is complete at
  `docs/history/M21_06_ADVANCED_DEBUG_SURFACE_CLOSEOUT.md`. Trigger draft,
  trigger check, pending AUTO, manual resolution, and apply preview diagnostics
  now live inside a hidden-by-default Advanced drawer in both local and online
  PlayTable modes. Verification passed Unity compile, EditMode `981/981`,
  Windows build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
- Latest player-readable event log implementation: `M21-07` is complete at
  `docs/history/M21_07_PLAYER_READABLE_EVENT_LOG_CLOSEOUT.md`. The default
  PlayTable match log now shows player-facing lines such as `P1 drew 1 card.`
  and omits raw action names, private card instance ids, target ids, and
  reducer-style zone traces. Verification passed Unity compile, EditMode
  `982/982`, Windows build `errors=0 warnings=0`, and Windows player smoke
  `blockers=[]`.
- Latest test and Windows smoke roll-up: `M21-08` is complete at
  `docs/history/M21_08_TESTS_WINDOWS_SMOKE_CLOSEOUT.md`. It records current
  formatter/layout helper coverage, hidden-by-default Advanced drawer checks,
  no-direct-mutation coverage, and Windows player smoke evidence from `M21-07`.
- M21 PlayTable Windows UX pass is closed at
  `docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md`.
- Latest settings implementation: `M22-01` is complete at
  `docs/history/M22_01_PLAYER_SETTINGS_CLOSEOUT.md`. `PlayerSettings` now
  covers player name, default deck, preferred format, UI scale, and image/cache
  mode as pure local preferences with JSON round-trip tests.
- Latest deck appearance implementation: `M22-02` is complete at
  `docs/history/M22_02_DECK_APPEARANCE_METADATA_CLOSEOUT.md`.
  `DeckAppearanceMetadata` now stores safe semantic keys for sleeve, card back,
  playmat, crest, persona shield, gift marker, and quick shield, while deck
  validation remains unchanged.
- Latest Home settings implementation: `M22-03` is complete at
  `docs/history/M22_03_HOME_SETTINGS_SCREEN_CLOSEOUT.md`. Home now opens a
  session-local Settings screen backed by `PlayerSettings`.
- Latest deck accessories implementation: `M22-04` is complete at
  `docs/history/M22_04_DECK_TYPE_ACCESSORIES_DIALOG_CLOSEOUT.md`. Deck Builder
  now opens a dedicated Deck Type / Accessories dialog for deck format and
  appearance keys.
- Latest cosmetic legality guard: `M22-05` is complete at
  `docs/history/M22_05_COSMETIC_LEGALITY_SEPARATION_CLOSEOUT.md`.
  Regression tests now verify cosmetic metadata does not change deck
  validation and `DeckValidator` does not reference cosmetic models.
- Latest user deck asset slot validator: `M22-06` is complete at
  `docs/history/M22_06_USER_DECK_ASSET_SLOT_CLOSEOUT.md`. User-provided deck
  accessory files are accepted only through manifest entries with matching
  SHA-256 hashes and safe root-contained paths; missing/mismatched files fall
  back.
- Latest settings/accessories test roll-up: `M22-07` is complete at
  `docs/history/M22_07_SETTINGS_ACCESSORIES_TEST_ROLLUP_CLOSEOUT.md`.
- M22 Windows Settings / Deck Type / Accessories is closed at
  `docs/history/M22_WINDOWS_SETTINGS_ACCESSORIES_CLOSEOUT.md`.
- Latest manual milestones: `M23-01` is complete at
  `docs/history/M23_01_MANUAL_CONTENT_SPEC_CLOSEOUT.md`, and `M23-02` is
  complete at `docs/history/M23_02_MANUAL_SCREEN_CLOSEOUT.md`, and `M23-03`
  is complete at `docs/history/M23_03_LOADING_TIPS_CLOSEOUT.md`, and `M23-04`
  is complete at `docs/history/M23_04_ORIGINAL_CONTENT_GATE_CLOSEOUT.md`, and
  `M23-05` is complete at `docs/history/M23_05_MANUAL_FILTER_CLOSEOUT.md`, and
  `M23-06` is complete at `docs/history/M23_06_MANUAL_TESTS_CLOSEOUT.md`.
  The in-app Manual opens from Home and PlayTable, loading tips are wired,
  runtime manual text has an original-content guard, search/category filtering
  is available, and test coverage covers content load, fallback, and
  navigation. `M23` is closed at
  `docs/history/M23_MANUAL_TUTORIAL_CLOSEOUT.md`.
- `M24-01` Deck Builder Windows landscape is complete at
  `docs/history/M24_01_DECK_BUILDER_WINDOWS_LANDSCAPE_CLOSEOUT.md`.
- `M24-02` Count-line deck text is complete at
  `docs/history/M24_02_COUNT_LINE_DECK_TEXT_CLOSEOUT.md`.
- `M24-03` Deck import compatibility UI is complete at
  `docs/history/M24_03_DECK_IMPORT_MISMATCH_UI_CLOSEOUT.md`.
- `M24-04` CGS-like custom pack adapter spec is complete at
  `docs/history/M24_04_CGS_LIKE_CUSTOM_PACK_ADAPTER_SPEC_CLOSEOUT.md`.
- `M24-05` VangPro-like custom import spec is complete at
  `docs/history/M24_05_VANGPRO_LIKE_CUSTOM_IMPORT_SPEC_CLOSEOUT.md`.
- `M24-06` local custom import validator is complete at
  `docs/history/M24_06_LOCAL_CUSTOM_IMPORT_VALIDATOR_CLOSEOUT.md`.
- `M24-07` Pack Validation UI is complete at
  `docs/history/M24_07_PACK_VALIDATION_UI_CLOSEOUT.md`.
- `M24-08` Deck image export is complete at
  `docs/history/M24_08_DECK_IMAGE_EXPORT_CLOSEOUT.md`.
- `M24-09` import/custom pack workflow test rollup is complete at
  `docs/history/M24_09_CUSTOM_IMPORT_WORKFLOW_TEST_ROLLUP_CLOSEOUT.md`.
- `M24` Deck Builder / Import / Custom Pack UX is closed at
  `docs/history/M24_DECK_BUILDER_IMPORT_CUSTOM_PACK_UX_CLOSEOUT.md`.
- `M25-01` Photon trusted-client room policy is complete at
  `docs/history/M25_01_PHOTON_TRUSTED_CLIENT_ROOM_POLICY_CLOSEOUT.md`.
- `M25-02` lobby flow is complete at
  `docs/history/M25_02_LOBBY_FLOW_CLOSEOUT.md`.
- `M25-03` room status is complete at
  `docs/history/M25_03_ROOM_STATUS_CLOSEOUT.md`.
- `M25-04` reconnect UX is complete at
  `docs/history/M25_04_RECONNECT_UX_CLOSEOUT.md`.
- `M25-05` online PlayTable default UI is complete at
  `docs/history/M25_05_ONLINE_PLAYTABLE_DEFAULT_UI_CLOSEOUT.md`.
- `M25-06` replay sync/status is complete at
  `docs/history/M25_06_REPLAY_SYNC_STATUS_CLOSEOUT.md`.
- `M25-07` online room test rollup is complete at
  `docs/history/M25_07_ONLINE_ROOM_TEST_ROLLUP_CLOSEOUT.md`.
- `M25` Windows Online Room usability is closed at
  `docs/history/M25_WINDOWS_ONLINE_ROOM_USABILITY_CLOSEOUT.md`.
- `M26-01` bot/automation return audit is complete at
  `docs/history/M26_01_BOT_AUTOMATION_RETURN_AUDIT_CLOSEOUT.md`.
- `M26-02` bot legal-action/masked-state gate is complete at
  `docs/history/M26_02_BOT_LEGAL_ACTION_MASKED_STATE_GATE_CLOSEOUT.md`.
- `M26-03` bot explanation panel is complete at
  `docs/history/M26_03_BOT_EXPLANATION_PANEL_CLOSEOUT.md`.
- `M26-04` structured ability template gate is complete at
  `docs/history/M26_04_STRUCTURED_ABILITY_TEMPLATE_GATE_CLOSEOUT.md`.
- `M26-05` live effect no text parsing gate is complete at
  `docs/history/M26_05_LIVE_EFFECT_NO_TEXT_PARSING_GATE_CLOSEOUT.md`.
- `M26-06` Solo Play entry flow from Home is complete at
  `docs/history/M26_06_SOLO_PLAY_HOME_ENTRY_FLOW_CLOSEOUT.md`.
- `M26-07` bot automation safety regression gate is complete at
  `docs/history/M26_07_BOT_AUTOMATION_SAFETY_REGRESSION_CLOSEOUT.md`.
- `M26` Bot / Automation Return Gate is closed at
  `docs/history/M26_BOT_AUTOMATION_RETURN_GATE_CLOSEOUT.md`.
- `M27-01` Windows stability smoke coverage is complete at
  `docs/history/M27_01_WINDOWS_STABILITY_SMOKE_CLOSEOUT.md`.
- `M27-02` Windows smoke blocker review is complete at
  `docs/history/M27_02_WINDOWS_SMOKE_BLOCKER_REVIEW_CLOSEOUT.md`.
- `M27-03` Windows performance baseline is complete at
  `docs/history/M27_03_WINDOWS_PERFORMANCE_BASELINE_CLOSEOUT.md`.
- `M27-04` memory / performance gate is complete at
  `docs/history/M27_04_WINDOWS_PERFORMANCE_GATE_CLOSEOUT.md`.
- `M27-05` graceful error handling is complete at
  `docs/history/M27_05_WINDOWS_GRACEFUL_ERROR_HANDLING_CLOSEOUT.md`.
- `M27-06` integration / PlayMode test is complete at
  `docs/history/M27_06_WINDOWS_PLAYMODE_INTEGRATION_CLOSEOUT.md`.
- `M27-07` known limitations list is complete at
  `docs/history/M27_07_WINDOWS_KNOWN_LIMITATIONS_CLOSEOUT.md`.
- `M27-08` no-public-release gate is complete at
  `docs/history/M27_08_NO_PUBLIC_RELEASE_GATE_CLOSEOUT.md`.
- No active post-M27 implementation target is defined yet; plan the next
  development pass explicitly before starting new work.
  Continue the
  VangPro-style organization and Vanguard Area-style simulator mental model
  without copying comparator assets/code/data; Card Browser/Deck Builder
  taxonomy remains `VanguardAreaClanTaxonomy` unless an explicit later
  milestone replaces it.
  M19-02 through M19-08 completed the first player-experience reset:
  Home/Lobby entry, separated Deck Builder/Card Browser, Deck Tools dialog,
  zone-first PlayTable, Advanced drawer, state text cleanup, and
  Windows/Android reference visual smoke gates. The Online Room runtime surface
  now has Connection, Room, and Safety/Reveal panels, player-facing status text,
  Back Home, and formatter tests that avoid deck-code/revealed-code leaks. The
  reconnect lobby flow clears stale request/batch state across room changes,
  rejects pre-room/mismatched batches, and shows handoff readiness text for
  event `0` vs non-zero cursor batches. Windows and Android artifacts were
  rebuilt after taxonomy wiring and again after Android JSON catalog fallback.
  Home/Lobby forced-expand layout polish and M19-09 safe icon override loader
  are complete and covered by QA/tests plus Windows player smoke.
  Android LDPlayer smoke now passes install/push/force-stop/launch at
  `android_install_smoke_m19_09_icon_override_loader.json`; Home shows
  `Pack: Vanguard TH / 251`, `Cards 10836`, and compact semantic trigger badges.
  Android currently uses `card_catalog.json` to avoid
  native SQLite provider failures and shows image fallback until the external
  2.16 GiB image dump is provisioned.
- Temporary M19 UI baseline: use VangPro-style UX flow and organization because
  it is easier for players to understand than the current UI. Do not copy
  VangPro assets, exact frames, logos, button art, code, or package contents.
- Product decision 2026-06-27: use Vanguard Area-style clan/nation grouping
  for Card Browser and Deck Builder filters for now. This is the active
  player-facing taxonomy until a later explicit UI milestone replaces it.
  `VanguardAreaClanTaxonomy` is wired at runtime: classic clans are ordered
  into familiar buckets, D-era nations are selectable as nation filters, and
  raw pack data remains unchanged. Do not copy Vanguard Area files or assets.
- UI icon direction: follow `docs/UI_ICON_SYSTEM_SPEC.md`. A selected Lucide
  subset is already stored at
  `client/unity/VanguardThaiSim/Assets/UI/Icons/Lucide/` with license and
  package metadata. Use those icons for M19 navigation/actions instead of
  copying VangPro/Vanguard Area/official game assets.
- Consolidated architecture summary: `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`.
- Core/bot/simulation checklist: `docs/CORE_DEVELOPMENT_GUARDRAILS.md`.
- Expanded core/rule/format/timing-window reference:
  `docs/VANGUARD_CORE_RULE_ARCHITECTURE_REFERENCE.md`.
- Deck privacy/public-room commitment plan: `docs/DECK_PRIVACY_COMMITMENT_SPEC.md` and `docs/ADR/ADR-0004-deck-privacy-before-ranked.md`.
- Private event payload boundary: `docs/PRIVATE_EVENT_PAYLOAD_SPEC.md`.
- New research note: `docs/MASTER_DUEL_RESEARCH_NOTES.md`.
- Master Duel-informed direction: `RulesCore` command/query facade, snapshot/RNG foundation, AbilityCore foundation, content-addressed pack verification, image cache, responsive runtime UI, custom pack source/import pipeline, multiplayer event-sync foundation, Photon Realtime payload layer, hidden-state observation views, lobby/reconnect surface, game-session sync controller, PlayTable online action path, and lobby start-table handoff now exist; next work should keep online transport separate from core game logic.
- M6 mobile readiness is complete; do not redo it unless a concrete mobile QA issue appears.

## Key Product Direction

```text
Vanguard Area-style manual freedom
+ Dear Days/Cardfight Online-style field, hand, zone, phase, and action layout
+ VangPro-style deck builder/import/share
+ Thai card database/images
+ bot layer ของเราเอง
```

## MVP Definition

MVP แรกไม่ใช่ online และไม่ใช่ auto effect เต็มระบบ MVP คือ:

- Card browser
- Deck builder
- Deck validator
- Save/load deck
- Manual play table
- Action log/replay พื้นฐาน

## Important Constraints

- ห้ามลอก code/asset ของ VangPro หรือเกม official
- ห้ามทำ online ก่อน local engine เสถียร
- ห้ามพยายาม auto-resolve ทุกสกิลใน phase แรก
- ห้ามเอารูปการ์ด 2GB เข้า repo หลักโดยตรง

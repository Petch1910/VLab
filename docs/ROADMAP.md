# Roadmap

## Active Phase: Windows-First Program Completion

Status: current

The active roadmap now prioritizes finishing the Windows program workflow before
returning to mobile, APK, app packaging, release candidate, or public
distribution work.

Active order:

- `M20` Windows Product Reset. Done.
- `M21` PlayTable Windows UX Pass. Done; `M21-01` through `M21-04d`,
  `M21-04c`, `M21-05a`, `M21-05a2`, `M21-05a3`, `M21-05a4`, `M21-05a5`,
  `M21-05a6`, `M21-05a7`, `M21-05b1`, `M21-05b2`, `M21-05b3`, and
  `M21-05b4`, `M21-05b5`, `M21-06`, `M21-07`, `M21-08`, `M21-09` done.
- `M22` Windows Settings / Deck Type / Accessories. Done.
- `M23` In-App Manual / Tutorial. Done.
- `M24` Deck Builder / Import / Custom Pack UX. Done; `M24-01` Windows
  landscape Deck Builder, `M24-02` count-line deck text, `M24-03` deck import
  compatibility UI, `M24-04` CGS-like custom pack adapter spec, `M24-05`
  VangPro-like custom import spec, `M24-06` local custom import validator,
  `M24-07` Pack Validation UI, `M24-08` Deck image export, `M24-09`
  import/custom pack workflow test rollup, and `M24-10` closeout complete.
- `M25` Windows Online Room Usability. Done; `M25-01` Photon trusted-client
  room policy, `M25-02` lobby flow, and `M25-03` room status complete;
  `M25-04` reconnect UX and `M25-05` online PlayTable default UI complete;
  `M25-06` replay sync/status, `M25-07` test rollup, and `M25-08` closeout
  complete.
- `M26` Bot / Automation Return Gate. Done; `M26-01` audit, `M26-02`
  legal-action/masked-state gate, `M26-03` explanation panel, `M26-04`
  structured ability template gate, and `M26-05` no-live-text parsing gate
  complete; `M26-06` Solo Play entry flow from Home and `M26-07`
  no-hidden-leak / simulation / replay regression gate complete; `M26-08`
  closeout complete.
- `M27` Windows Stability Pass. Done. `M27-01` through `M27-08` are complete.
- `M28` Windows Gameplay Completion Pass. Done after the online-room
  reopen pass; `M28-01` gameplay completion
  gate, `M28-02` local PlayTable seat toggle, and `M28-03` UI-level two-seat
  match smoke are done. `M28-04` Windows manual match gap audit is done.
  `M28-05` PlayTable guided next-action panel and `M28-06` Windows
  built-player smoke are done. `M28-07` PlayTable action grouping polish is
  done. `M28-08` PlayTable side-panel density audit is done. `M28-09` Bot Plan
  primary-panel cleanup is done. `M28-10` Match Log / Preview density review is
  done.
- `M29` Photon Lobby / Room UI Reopen Pass. Done; `M29-01` Photon lobby
  navigation lockout, `M29-02` reconnect flow polish, and `M29-03` Quick Deck
  Selector are done. `M29-04` Quick Edit modal and `M29-05` Online Room
  usability audit are done. `M29-06` Online deck readiness guard is done.
- `M30` Windows Playable Loop Final Audit. Done; `M30-01` found the Replay
  Home route was still locked. `M30-02` unlocked the Replay screen. `M30-03`
  added local replay JSON validation. `M30-04` added a read-only replay preview.
  `M30-05` added local PlayTable replay export. `M30-06` closed the Windows
  playable loop audit with no automated blocker.
- `M31` Windows UI Evidence / Polish Pass. Done for the current route;
  `M31-01` evidence capture
  and blocker/polish audit is done. `M31-02` Card Workshop first-screen clarity
  pass is done. `M31-03` Card Workshop toolbar density pass is done. `M31-04`
  Windows UI visual evidence pass is done. `M31-05` Card detail preview
  aspect-ratio fix is deferred while the PlayTable reset is active.
- `M32` Vanguard Area-style PlayTable Reset. Paused by user instruction.
  `M32-01` field-mat shell,
  `M32-01b` digital-client UX blueprint, `M32-02` zone placement polish,
  `M32-02b` letsplay layout research alignment, and `M32-03` PlayTable
  de-dashboard field/HUD pass are done. `M32-04` playmat slot layout and
  Windows visual evidence closeout is done. `M32-05` hand strip and compact
  pile interaction polish is deferred. The M32 research scope is
  layout-only: field, card placement, zones, hand, phase/action prompts,
  trigger/check/guard surfaces, online table HUD, and deck-builder structure.
  Story, character, dialogue, and campaign UI are out of scope.
- `M33` Offline Clan Combo Pairing Logic. Done through matrix artifacts:
  M33-01 first clan combo report, M33-02 era preset expansion, and M33-03
  combo matrix CSV/JSON artifacts.
- `M34` Offline Deck Construction Possibility. Done through Phase A1:
  `M34-01` deck possibility analysis, `M34-02` rule corpus intake, and
  `M34-03` deck-feasible archetype priority ranking.
- `Hybrid Vertical-Slice Strategy`. Done. The goal was one selected deck/combo
  slice moving end-to-end before broad scale-out:
  - **Phase A**: Foundation Slice. Done. `M34-03` ranking is done, `M35-A2`
    selected `Classic Core / โนว่า เกรปเปอร์`, `M35-A3` generated minimal
    deck legality fixtures, and `M35-A4` reports Phase A ready for Phase B.
  - **Phase B**: Semantic Slice. Done. `M35-B1` created the selected-slice
    semantic vocabulary, `M35-B2` extracted selected-slice semantic tags, and
    `M35-B3` created the requirement/provider model. `M35-B4` exported the
    manual review queue and closes Phase B for the first slice.
  - **Phase C**: Compatibility Slice. Done. `M35-C1` created the selected
    slice pair compatibility graph, `M35-C2` added the resource conflict
    detector, `M35-C3` added the timing compatibility detector, and `M35-C4`
    added the zone/target detector. `M35-C5` closed Phase C with selected-slice
    compatibility output and `604` clean D1 candidate edges.
  - **Phase D**: Deck Skeleton + Safe Playbook Seed. Done. `M35-D1`
    selected `25` candidate packages from `604` clean synergy edges. `M35-D2`
    produced `25` advisory deck skeleton ratio plans with SQLite-backed
    package-local shield profiles. `M35-D3` produced `25` human-reviewable
    combo line explanations. `M35-D4` exported `1` static-reviewed advisory
    playbook seed and retained `24` rejected lines with review reasons.
  - **Phase E**: Scale Out. Done. `M35-E1` selected the second slice:
    `Classic Core / Oracle Think Tank`. `M35-E2` confirmed fixture readiness
    and Classic Core policy reuse for that second slice. `M35-E3` proved the
    selected-report input contract by running B1-B4 and C1-C5 for the second
    slice. `M35-E4` passed the bot integration gate with `1` reviewed future
    hint candidate, blocked `M35-E3` probe edges from runtime bot use, and kept
    runtime bot integration disabled. `M35-closeout` closed the phase and
    selected `M36`.
- `M36` Human-review-assisted Deck Recipe Validation. Done. `M36-01`
  first-slice review packet is done with `31` review items. `M36-02` deck
  recipe draft model is done with `25` drafts and `16` slot-gap drafts.
  `M36-03` deck recipe validator is done with `25` checked drafts, `0`
  runtime-ready recipes, `0` missing-card recipes, and `0` copy-limit
  violation recipes. `M36-04` combo-line consistency is done with `25` combo
  lines present, `0` missing combo-card checks, and `0` promotable lines.
  `M36-05` second-slice readiness comparison is done: Oracle Think Tank is
  ready for future offline recipe work, but broader runtime scale-out remains
  disabled. `M36-closeout` is done and selected `M37` because M36 still has
  `0` runtime-ready recipes and `0` promotable combo lines.
- `M37` First-slice Blocker Resolution and Recipe Repair. Active. `M37-01`
  accepted seed slot-gap completion candidates is done with `18` source-backed
  trigger candidate cards and `5` advisory completion packages for
  `recipe_003`. `M37-02` trigger package repair proposal is done and
  recommends advisory package `m37_01_pkg_001` / `balanced_classic`, resolving
  trigger/deck-size blockers while keeping runtime promotion disabled.
  `M37-03` rejected-line support-gap triage is done with `24` rejected lines
  classified into `5` support-gap groups. `M37-04` manual semantic mapping
  candidates is done with `5` non-executable mapping candidates from `49`
  triage line links. `M37-05` revised recipe validation rerun is done; accepted
  seed `recipe_003` clears trigger/deck-size blockers in memory but remains
  `validator_passed_pending_human_acceptance`. `M37-closeout` is done and
  decides `recipe_003` remains advisory because human acceptance, grade-profile
  review, and promotion allowance are still open.
- `M38` Human Acceptance and Grade-profile Repair Gate. Active. `M38-01`
  accepted seed human review packet is done for `recipe_003`, but it does not
  record acceptance. `M38-02` grade profile repair candidates is done with `2`
  complete substitution-preview packages reaching `G0=17/G1=14/G2=11/G3=8`.
  `M38-03` human-accepted recipe artifact is done with accepted grade package
  `m38_02_grade_pkg_001`, trigger package `m37_01_pkg_001`, blockers `0`, and
  runtime promotion still disabled. `M38-04` runtime fixture promotion gate is
  done with all `5` checks passed and an offline runtime/test fixture artifact
  created without mutating saved decks, UI, bot, or `GameState`. `M38-closeout`
  is done: `recipe_003` enters offline fixture scope only and next queue is
  `M39`.
- `M39` Fixture Consumption and Second-Slice Scale Gate. Done. `M39-01`
  offline fixture schema validator is done with `0` blockers and
  SQLite-recomputed counts. `M39-02` fixture-to-deck text exporter is done with
  reviewable count-line deck text, `17` importable card lines, `50` total
  cards, and no saved deck/UI/bot/GameState mutation. `M39-03` headless
  fixture load smoke is done: the generated `VGTH1.` deck code was accepted by
  Unity headless with `deck_source=deck_code`, `actions_executed=4`, and
  `event_count=4`. `M39-04` second-slice recipe scale decision is done and
  opens only the offline Oracle Think Tank recipe pipeline; saved deck/UI,
  runtime, and bot/playbook promotion remain blocked.
- `M40` Second-slice Offline Recipe Pipeline. Done. `M40-01` second-slice
  review packet is done with `6` fixture notes, `7` manual-review cards, and
  `259` candidate edges for Classic Core / Oracle Think Tank. `M40-02`
  second-slice recipe draft model is done with `25` quantity-complete advisory
  drafts. `M40-03` second-slice recipe validator is done with `25` validated
  drafts, `0` runtime-ready recipes, and `25` manual-review overlap blockers.
  `M40-04` combo-to-recipe consistency is done with all `25` candidate edge
  pairs present, `0` missing pair-card checks, and `0` promotion-allowed
  checks. `M40-05` blocker repair candidates is done with `25` advisory repair
  items and `25` grade-profile candidates that can clear manual overlap for
  human review. `M40-closeout` is done with `m40_complete=True`, `0`
  runtime-ready recipes, `0` promotion-allowed checks, and `25` repair
  candidates ready for human review.
- `M41` Second-slice Human Repair Review Gate. Active. `M41-01` exported `25`
  repair review items and remains review-only. `M41-02` records acceptance for
  `m40_recipe_001` but does not declare it valid. `M41-03` validates the
  repaired recipe and finds trigger count `2/16`, so M41-04 was blocked until
  the repair loop.
  `M41-repair` exports `3` complete trigger/profile repair candidates.
  `M41-repair-accept` records acceptance of balanced package
  `m41_repair_pkg_001`. `M41-repair-validate` passes with `50` cards, `16`
  triggers, target grade profile, manual-review overlap cleared, and `0`
  blockers. `M41-04` passes all `6` promotion-gate checks and creates an
  offline runtime/test fixture artifact. Current next target is `M41-closeout`
  second-slice fixture closeout. Saved deck/UI publication, bot/playbook
  promotion, and `GameState` mutation remain blocked.
- `M41-closeout` Second-slice Fixture Closeout. Done. Oracle Think Tank enters
  offline runtime/test fixture scope only, live runtime deck/UI/saved deck/bot
  use remains disabled, and next queue is `M42`.
- `M42` Second Fixture Consumption and Third-Slice Scale Gate. Active.
  `M42-01` second fixture schema validator is done with `0` blockers.
  `M42-02` second fixture deck text exporter is done with `15` importable card
  lines and `0` blockers. `M42-03` second fixture headless load smoke is done
  with Unity accepted deck-code evidence. `M42-04` multi-fixture scale decision
  is done and opens the third-slice offline pipeline only.
- `M43` Third-Slice Offline Pipeline Entry. Active. `M43-01` selected
  `เบอร์มิวด้า ไทรแองเกิล` for offline analysis only. `M43-02` third-slice
  fixture/format readiness is done with `127` source-backed cards, grade 0-3
  coverage, trigger capacity `84`, and `requires_third_slice_fixture_scaffold`.
  `M43-03` semantic/compatibility probe is done with `127` semantic cards,
  `61` manual-review cards, `4835` pair edges, and `109` candidate edges.
  `M43-04` recipe pipeline entry gate is done with blockers `0` and opens the
  offline M44 queue. `M44-01` fixture scaffold is done with source-backed
  validator policy and blockers `0`. `M44-02` review packet is done with
  `171` review items. `M44-03` recipe draft model is done with `25`
  quantity-complete advisory drafts, all still blocked by manual-review
  overlap. `M44-04` recipe validator is done with `25` validated drafts, `0`
  runtime-ready recipes, and `25` manual-review overlap blockers. `M44-05` is
  done with candidate pair cards present in all `25` drafts, `0` missing
  pair-card checks, and `0` promotion-allowed checks. `M44-06` is done with
  `25` advisory repair items, `25` complete manual repair packages, and `25`
  complete grade-profile repair packages, while runtime promotion remains
  disabled. `M44-closeout` is done with `m44_complete=True`, runtime-ready
  recipe unavailable, and next queue `M45`. `M45-01` is done with `25`
  review items, `25` complete manual repair packages, `25` complete
  grade-profile candidates, runtime promotion disabled, and
  `ready_for_m45_02=True`. `M45-02` is done with accepted recipe
  `m44_recipe_001`, `2` source grade-package conflicts detected after manual
  substitution, recomputed combined grade repair, a `50`-card preview, and
  `ready_for_m45_03=True`. `M45-03` is done with `validator_passed=1`,
  `runtime_ready=1`, no missing/copy-limit/slot/trigger/manual-overlap/
  grade-profile issues, and `ready_for_m45_04=True`. `M45-04` is done with
  `promotion_allowed=True`, `7/7` gate checks passed, and an offline
  runtime/test fixture artifact created without saved-deck/UI/bot/GameState
  mutation. `M45-closeout` is done with `m45_complete=True`, third runtime
  fixture available, and next queue `M46`. `M46-01` is done with
  `schema_valid=True`, blockers `0`, third fixture counts recomputed from
  SQLite, and no runtime/UI/saved-deck/bot/GameState mutation. Current next
  target is `M46-02` third fixture deck text exporter. `M46-02` is done with
  `export_ready=True`, blockers `0`, `15` exported card lines, review-only
  count-line deck text generated, and no runtime/UI/saved-deck/bot/GameState
  mutation. `M46-03` is done with `offline_load_ready=True`,
  `unity_headless_smoke_passed=True`, `deck_source=deck_code`,
  `actions_executed=4`, `event_count=4`, blockers `0`, and no runtime/UI/
  saved-deck/bot/GameState mutation. `M46-04` is done with
  `ready_for_m47=True`, `passed_fixture_count=3`, `failed_fixture_count=0`,
  `candidate_count=5`, fourth-slice offline pipeline allowed, and no runtime/
  UI/saved-deck/bot/GameState mutation. Current next target is `M47-01`
  fourth target slice selection. `M47-01` is done, selecting
  `รอยัล พาลาดิน` / `g_series_first` for offline analysis only, with no
  recipe/runtime/UI/saved-deck/bot/GameState mutation. Current next target is
  `M47-02` fourth-slice fixture/format readiness. `M47-02` is done with
  source-backed card count `71`, trigger gap `Heal`,
  `all_fixture_expectations_met=False`, `repair_required=True`, and no
  recipe/runtime/UI/saved-deck/bot/GameState mutation. Current next target is
  `M47-repair` fourth-slice readiness blocker repair. `M47-repair` is done,
  finding same-group Heal triggers outside the selected scope and recommending
  `review_same_group_source_expansion` without card data/runtime/UI/saved-deck/
  bot/GameState mutation. `M47-repair-expand-scope` is done,
  recommending `g_era_heal_expansion`, expanding source count to `190`, and
  clearing trigger gaps without applying the expansion or mutating card data/
  runtime/UI/saved-deck/bot/GameState. `M47-repair-apply-scope` is done,
  applying that expansion to an offline fixture pipeline scope artifact with
  blockers `0` and no card data/runtime/UI/saved-deck/bot/GameState mutation.
  `M47-03` is done with semantic cards `190`, pair graph edges `14150`,
  candidate edges `785`, all stage readiness flags passing, and no card data/
  runtime/UI/saved-deck/bot/GameState mutation. `M47-04` is done, allowing the
  offline M48 recipe pipeline with blockers `0`, fixture scaffold required, and
  no card data/recipe/runtime/UI/saved-deck/bot/GameState mutation. `M48-01` is
  done with scaffold ready, blockers `0`, Grade 4/G Zone deferred as advisory/
  manual-review only, and no card data/recipe/runtime/UI/saved-deck/bot/GameState
  mutation. `M48-02` is done with `1` fixture scaffold item, `15` manual-review
  cards, `785` candidate edges, `801` total review items, and no card data/
  recipe/runtime/UI/saved-deck/bot/GameState mutation. `M48-03` is done with
  `25` advisory drafts, `25` quantity-complete drafts, skipped `35` trigger/G4/
  missing candidate edges, and no card data/runtime/UI/saved-deck/bot/GameState
  mutation. `M48-04` is done with `25` drafts validated, runtime-ready recipes
  `0`, manual-review blocked recipes `25`, Grade 4 main-deck violations `0`, and
  no card data/runtime/UI/saved-deck/bot/GameState mutation. `M48-05` is done
  with `25` consistency checks, pair cards present `25`, promotion allowed `0`,
  G Zone deferred checks `25`, and no card data/runtime/UI/saved-deck/bot/
  GameState mutation. `M48-06` is done with `25` recipes reviewed, complete
  manual repair candidates `25`, complete grade-profile candidates `24`, G Zone
  deferred recipes `25`, unexpected structural blockers `0`, and no card data/
  runtime/UI/saved-deck/bot/GameState mutation. `M48-closeout` is done with
  M48 complete `true`, runtime-ready recipe available `false`, human/G-Zone
  review allowed `true`, G Zone deferred recipes `25`, next queue `M49`, and no
  card data/runtime/UI/saved-deck/bot/GameState mutation. `M49-01` is done with
  `25` review items, `25` G Zone decision items, ready_for_m49_02 `true`, and no
  card data/runtime/UI/saved-deck/bot/GameState mutation. `M49-02` is done with
  selected option `main_deck_only_for_current_windows_fixture`, `25` decision
  items, G Zone runtime `false`, Stride runtime `false`, runtime promotion
  `false`, and ready_for_m49_03 `true`. `M49-03` is done with accepted
  `m48_recipe_001`, main deck `50`, grade profile `17/14/11/8`, repair issues
  `0`, G Zone runtime `false`, Stride runtime `false`, runtime promotion
  `false`, and ready_for_m49_04 `true`. `M49-04` is done with validator_passed
  `1`, runtime_ready `1`, issue_counts `{}`, G Zone runtime `false`, Stride
  runtime `false`, runtime promotion `false`, and ready_for_m49_05 `true`.
  `M49-05` is done with promotion_allowed `true`, passed checks `8`, fixture
  created `true`, G Zone runtime `false`, Stride runtime `false`, and
  saved-deck/UI/bot/GameState mutation disabled. `M49-closeout` is done with
  M49 complete `true`, fourth fixture available `true`, next queue `M50`, G
  Zone runtime `false`, Stride runtime `false`, and saved-deck/UI/bot/
  GameState mutation disabled. `M50-01` is done with schema valid `true`,
  blockers `0`, main deck `50`, unique cards `14`, G Zone runtime `false`, and
  Stride runtime `false`. `M50-02` exported 14 count-line main-deck lines.
  `M50-03` passed Unity headless with `deck_source=deck_code`, actions/events
  `4/4`, and G Zone count `0`. `M50-04` is done with four passed fixtures,
  candidates `5`, fifth-slice offline pipeline allowed `true`, G Zone runtime
  `false`, and Stride runtime `false`. `M51-01` is done, selecting
  `โกลด์ พาลาดิน` / `link_joker_legion_mate` for offline analysis only.
  `M51-02` is done with source_card_count `106`, trigger capacity `36`,
  non-trigger capacity `388`, trigger gaps `[]`, fixture expectations met
  `true`, and semantic probe ready `true`. `M51-03` is done with semantic
  cards `106`, manual-review cards `4`, pair graph edges `3075`, candidate
  edges `142`, and all stage readiness passed `true`. `M51-04` is done with
  offline recipe pipeline allowed `true`, blockers `0`, fixture scaffold
  required `true`, and runtime/UI/saved-deck/bot/GameState mutation disabled.
  `M52-01` is done with source-backed cards `106`, trigger capacity `36`,
  candidate edges `142`, scaffold_ready `true`, blockers `0`, and
  runtime/UI/saved-deck/bot/GameState mutation disabled. `M52-02` is done with
  fixture scaffold items `1`, manual-review cards `4`, candidate edges `142`,
  total review items `147`, ready_for_m52_03 `true`, and runtime/UI/saved-deck/
  bot/GameState mutation disabled. `M52-03` is done with `25` advisory recipe
  drafts, `25` quantity-complete drafts, trigger/missing skipped edges `0`,
  manual-overlap recipes `0`, ready_for_m52_04 `true`, and runtime/UI/
  saved-deck/bot/GameState mutation disabled. `M52-04` is done with `25`
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
  `M53-03` is done with accepted repaired recipe `m52_recipe_001`, repaired
  main deck count `50`, repair application issues `0`, and ready_for_m53_04
  `true`. `M53-04` is done with validation `validator_passed`, consistency
  `consistent_validator_passed`, runtime-ready recipes `1`, blockers `0`, and
  ready_for_m53_05 `true`. `M53-05` is done with promotion_allowed `true`,
  passed checks `5`, failed checks `0`, and offline fixture
  `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`.
  `M53-closeout` is done with M53 complete `true`, fifth runtime fixture
  available `true`, next queue `M54`, and saved-deck/UI/bot/GameState mutation
  disabled. `M54-01` is done with schema valid `true`, blockers `0`, main deck
  `50`, unique card count `16`, trigger profile `4/4/4/4`, grade profile
  `17/14/11/8`, ready_for_m54_02 `true`, and full Python tests `1008/1008`.
  `M54-02` is done with export_ready `true`, blockers `0`, main deck `50`,
  exported card lines `16`, ready_for_m54_03 `true`, and full Python tests
  `1015/1015`. `M54-03` is done with offline load ready `true`, Unity
  headless accepted `true`, deck source `deck_code`, actions/events `4/4`,
  blockers `0`, ready_for_m54_04 `true`, and full Python tests `1024/1024`.
  `M54-04` is done with five fixtures passed, failed fixtures `0`, candidate
  count `5`, sixth-slice offline pipeline allowed `true`, and full Python tests
  `1032/1032`. `M55-01` is done with selected group `ชาโดว์ พาลาดิน`, era
  `g_next_z`, rank `4`, candidate count `5`, ready_for_m55_02 `true`, and full
  Python tests `1039/1039`. `M55-02` is done with source cards `77`, grade
  profile `19/20/16/11/11`, trigger profile `Critical=4/Draw=4/Heal=2/Stand=2`,
  semantic probe ready `true`, and full Python tests `1047/1047`. Current next
  target is `M55-03` sixth-slice semantic/compatibility probe. `M55-03` is done
  with semantic cards `77`, manual-review cards `11`, pair graph edges `2069`,
  candidate edges `70`, all stage readiness `true`, and full Python tests
  `1055/1055`. `M55-04` is done with offline recipe pipeline allowed `true`,
  blockers `0`, ready_for_m56 `true`, and full Python tests `1064/1064`.
  `M56-01` is done with scaffold_ready `true`, blockers `0`, source cards
  `77`, Grade 4 cards advisory only until G Zone support, and full Python
  tests `1073/1073`. `M56-02` is done with review items `82`, manual-review
  cards `11`, candidate edges `70`, ready_for_m56_03 `true`, and full Python
  tests `1081/1081`. `M56-03` is done with recipe drafts `12`,
  quantity-complete recipes `12`, skipped trigger/Grade 4/missing edges `58`,
  manual-overlap recipes `12`, and full Python tests `1090/1090`. `M56-04` is
  done with validated drafts `12`, runtime-ready recipes `0`,
  manual-review blocked recipes `12`, missing/copy/slot/trigger/Grade 4
  blockers `0`, and full Python tests `1097/1097`. `M56-05` is done with
  consistency checks `12`, pair cards present `12`, missing pair-card checks
  `0`, promotion allowed `0`, recipe manual dependencies `12`, G Zone
  deferred `12`, and full Python tests `1103/1103`. `M56-06` is done with
  repair items `12`, manual repair complete `12`, grade repair complete `12`,
  G Zone deferred `12`, ready for human repair review `12`, and full Python
  tests `1111/1111`. `M56-closeout` is done with M56 complete `true`,
  runtime-ready recipe available `false`, next queue `M57`, manual review
  blocked `12`, G Zone deferred `12`, and full Python tests `1120/1120`.
  `M57-01` is done with review items `12`, complete manual repairs `12`,
  complete grade repairs `12`, G Zone deferred items `12`, and full Python
  tests `1130/1130`. Current next target is `M57-02` sixth-slice
  human-selected recipe artifact. M57-02 spec/tool/tests are scaffolded and
  verified (`9/9` targeted, `1139/1139` full Python), but the real selected
  artifact still requires an explicit valid M57 review item id. M57-03
  spec/tool/tests are also scaffolded and verified (`7/7` targeted,
  `1146/1146` full Python), but its real accepted artifact remains gated on
  M57-02 output and explicit acceptance text. M57-04 spec/tool/tests are
  scaffolded and verified (`16/16` targeted across M57-03/M57-04, `1155/1155`
  full Python), but its real G Zone / Stride decision artifact remains gated on
  M57-03 output and explicit option selection. M57-05 spec/tool/tests are
  scaffolded and verified (`18/18` targeted across M57-04/M57-05, `1164/1164`
  full Python), but its real repaired validation report remains gated on M57-03
  and M57-04 outputs. M57-06 spec/tool/tests are scaffolded and verified
  (`9/9` targeted, `1173/1173` full Python); it can create only an offline
  runtime/test fixture after M57-03 accepted rows and M57-05 validation exist,
  while saved deck/UI, bot/playbook, G Zone/Stride runtime, and `GameState`
  mutation remain disabled. M57-closeout spec/tool/tests are scaffolded and
  verified (`6/6` targeted, `1179/1179` full Python); it routes a completed
  sixth fixture to M58 consumption/scale work, but the real closeout output
  remains gated on M57-06 output. Current next target is `M58-01` sixth fixture
  schema validator. M58-01 spec/tool/tests are scaffolded and verified (`11/11`
  targeted, `1190/1190` full Python) against an in-memory M57-06 fixture; the
  real report remains gated on the real M57-06 fixture file. M58-02
  spec/tool/tests are scaffolded and verified (`7/7` targeted, `1197/1197`
  full Python) against an in-memory fixture plus M58-01 validation report; the
  real deck text/report artifacts remain gated on real M57-06 and M58-01
  files. M58-03 spec/tool/tests are scaffolded and verified (`9/9` targeted,
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
  M61-05 spec/tool/tests are scaffolded and verified (`10/10` targeted,
  `1352/1352` full Python) using in-memory M61-03/M61-04 artifacts; it
  validates the repaired main-deck preview, suppresses G Zone and Bloom/token
  deferred review codes only for boundary decisions that explicitly allow
  main-deck validation, passes
  validation/consistency for valid accepted items, reports ready_for_m61_06
  `true`, creates no runtime fixture, declares no runtime promotion, and does
  not mutate runtime/UI/bot/GameState. Real M61-05 artifacts remain gated on
  real M61-03 and M61-04 outputs.
  M61-06 spec/tool/tests are scaffolded and verified (`10/10` targeted,
  `1362/1362` full Python) using in-memory M61-03/M61-05 artifacts; it
  creates an offline runtime/test fixture artifact only when human acceptance,
  validation, consistency, G Zone boundary, and Bloom/token boundary all pass,
  blocks deferred G Zone or Bloom/token choices, keeps G Zone/Stride/Bloom/token
  runtime disabled, creates no saved deck, publishes no UI deck, enables no
  bot/playbook, and does not mutate runtime/UI/bot/GameState. Real M61-06
  artifacts remain gated on real M61-03 and M61-05 outputs.
  M61-closeout spec/tool/tests are scaffolded and verified (`6/6` targeted,
  `1368/1368` full Python) using in-memory M61-06 evidence; it routes passing
  evidence to M62 seventh fixture consumption only, routes failed evidence to
  M61-repair, keeps saved deck/UI/bot/GameState mutation disabled, and keeps G
  Zone/Stride/Bloom/token runtime disabled. Real M61-closeout artifacts remain
  gated on real M61-06 output.
  M62-01 spec/tool/tests are scaffolded and verified (`13/13` targeted,
  `1381/1381` full Python) using an in-memory M61-06 fixture; it validates the
  Neo Nectar fixture independently from the M61 generator, accepts M61-06
  `source_artifacts` compatibility, recomputes counts from SQLite, and rejects
  unsafe G Zone/Stride/Bloom/token boundaries without enabling saved
  deck/UI/bot/GameState mutation. Real M62-01 artifacts remain gated on the
  real M61-06 fixture file.
  M62-02 spec/tool/tests are scaffolded and verified (`8/8` targeted,
  `1389/1389` full Python) using an in-memory M61-06 fixture plus M62-01
  validation evidence; it exports review-only count-line deck text for the Neo
  Nectar fixture, preserves empty Ride/G sections, records G Zone/Stride/
  Bloom/token runtime as disabled, rejects invalid validation gates and
  malformed main-deck rows, and avoids saved deck/UI/bot/GameState mutation.
  Real M62-02 artifacts remain gated on the real M61-06 fixture and M62-01
  validation files.
  M62-03 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1398/1398` full Python) using an in-memory M61-06 fixture and M62-02 deck
  text; it parses the count-line deck text, verifies fixture/deck text
  equality, creates a `VGTH1.` deck code, verifies deck-code round-trip, keeps
  Ride/G empty, keeps G Zone/Stride/Bloom/token runtime disabled, accepts
  optional Unity headless evidence only when `deck_source=deck_code`, and
  avoids saved deck/UI/bot/GameState mutation. Real M62-03 artifacts remain
  gated on the real M61-06, M62-01, and M62-02 files.
  M62-04 spec/tool/tests are scaffolded and verified (`8/8` targeted,
  `1406/1406` full Python) using first-five real smoke reports plus in-memory
  sixth and seventh smoke reports with Unity deck-code evidence; it reviews
  seven fixture evidence records, excludes completed groups from the candidate
  queue, allows only the next offline pipeline, selects no runtime deck,
  creates no fixture, publishes no UI deck, enables no bot/playbook, keeps G
  Zone/Stride/Bloom-token runtime disabled, and avoids GameState mutation.
  Real M62-04 artifacts remain gated on the real M58-03 and M62-03 evidence
  files.
  M63-01 spec/tool/tests are scaffolded and verified (`7/7` targeted,
  `1413/1413` full Python) using an in-memory M62-04 scale decision; it selects
  the first remaining candidate as the eighth offline target, snapshots the
  candidate queue, covers blocked and empty-queue paths, and avoids recipe
  draft/runtime fixture/saved deck/UI/bot/G Zone/Stride/Bloom-token/GameState
  mutation. Real M63-01 artifacts remain gated on the real M62-04 output file.
  M63-02 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1422/1422` full Python) using an
  in-memory M63-01 selection; it checks SQLite source cards, grade 0-3 setup,
  G-series Grade 4 readiness where required, trigger families, trigger
  capacity, main-deck capacity, repair routing, JSON/Markdown round-trip, and
  avoids runtime pack/deck/UI/bot/G Zone/Stride/Bloom-token/GameState mutation.
  Real M63-02 artifacts remain gated on the real M63-01 output file.
  M63-03 spec/tool/tests are scaffolded and verified (`8/8` targeted,
  `1430/1430` full Python) using in-memory M63-01/M63-02 reports; it reuses
  the M35 B/C semantic pipeline in
  memory for `คาเงโร่ / link_joker_legion_mate`, reports 121 source/semantic
  cards, 6 manual-review cards, 3398 pair graph edges, and 355 candidate
  edges, and avoids runtime pack/deck/UI/bot/G Zone/Stride/Bloom-token/
  GameState mutation. Real M63-03 artifacts remain gated on the real M63-01
  and M63-02 output files.
  M63-04 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1439/1439` full Python) using in-memory M63-02/M63-03 reports; it verifies
  the M63 readiness/probe
  evidence, opens only the offline M64 recipe pipeline, and avoids runtime
  pack/recipe draft/deck/UI/bot/G Zone/Stride/Bloom-token/GameState mutation.
  Real M63-04 artifacts remain gated on the real M63-02 and M63-03 output
  files.
  M64-01 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1448/1448` full Python) using in-memory M63-02/M63-03/M63-04 reports; it
  defines the eighth-slice offline
  fixture policy for `คาเงโร่ / link_joker_legion_mate`, reports 121 source
  cards and 355 candidate edges, defers Lock/Legion runtime, and avoids
  runtime pack/recipe draft/deck/UI/bot/G Zone/Stride/Lock/Legion/Bloom-token/
  GameState mutation. Real M64-01 artifacts remain gated on the real M63-02,
  M63-03, and M63-04 output files.
  M64-02 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1457/1457` full Python) using in-memory M63-01/M63-02/M63-03/M63-04/M64-01
  reports; it exports the review-only packet shape with 1 fixture scaffold
  item, 6 manual-review card items, 355 candidate edge items, and avoids
  recipe draft/runtime pack/deck/UI/bot/Lock/Legion/GameState mutation. Real
  M64-02 artifacts remain gated on the real upstream output files.
  M64-03 spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1466/1466` full Python) using in-memory M64-02/M64-01 reports plus runtime
  SQLite; it creates 25 advisory quantity-complete recipe drafts, skips 5
  trigger/Grade4/missing candidate edges, preserves a 50-card / 16-trigger
  structure, and avoids saved deck/UI/runtime deck/bot/Lock/Legion/GameState
  mutation. Real M64-03 artifacts remain gated on the real M64-02 and M64-01
  output files.
  M64-04 spec/tool/tests are scaffolded and verified (`7/7` targeted,
  `1473/1473` full Python) using in-memory M64-03 drafts plus runtime SQLite;
  it validates 25 drafts with no missing/copy/slot/trigger/Grade4/clan
  blockers, keeps all drafts pending human selection, records grade-profile
  plus Lock/Legion deferred review evidence, and avoids saved deck/UI/runtime
  deck/bot/GameState mutation. Real M64-04 artifacts remain gated on the real
  M64-03 output file.
  M64-05 spec/tool/tests are scaffolded and verified (`6/6` targeted,
  `1479/1479` full Python) using in-memory M64-03/M64-04 reports; it checks
  all 25 recipe drafts, confirms every candidate pair is present, records no
  manual dependency overlap, carries Lock/Legion deferred review evidence, and
  avoids saved deck/UI/runtime deck/bot/GameState mutation. Real M64-05
  artifacts remain gated on the real M64-03 and M64-04 output files.
  M64-06 spec/tool/tests are scaffolded and verified (`7/7` targeted,
  `1486/1486` full Python) using in-memory M64-03/M64-04/M64-05 reports; it
  creates 25 repair items, records 0 manual-overlap recipes, 25 human-selection
  candidates, 25 complete grade-profile repair candidates, 25 Lock-deferred
  packages, 25 Legion-deferred packages, and avoids saved
  deck/UI/runtime deck/Lock runtime/Legion runtime/bot/GameState mutation.
  Real M64-06 artifacts remain gated on the real M64-03, M64-04, and M64-05
  output files and fed M64-closeout.
  M64-closeout spec/tool/tests are scaffolded and verified (`9/9` targeted,
  `1495/1495` full Python) using in-memory M64-01..M64-06 reports; it confirms
  M64 scaffold evidence is complete, keeps runtime promotion disabled, carries
  forward 25 human-selection candidates and 25 complete grade-profile repair
  candidates, preserves Lock/Legion deferred system work, selects M65 as the
  next queue, and avoids saved deck/UI/runtime fixture/Lock runtime/Legion
  runtime/bot/GameState mutation. Real M64-closeout artifacts remain gated on
  the real M64-01..M64-06 output files. The current next target is M65-01.

Deferred until explicit user instruction:

- Android build and APK work.
- LDPlayer/ADB/emulator smoke.
- Android/mobile layout QA.
- App packaging and release-candidate packaging.
- Public distribution.

## Phase 0: Data Foundation

Status: done

- Export KK Vanguard TH data.
- Download card images.
- Verify card/image counts.
- Build runtime pack manifest and SQLite database.

## Phase 1: Unity Card Browser

Status: done

- Unity project.
- SQLite card repository.
- Card grid.
- Search and filters.
- Card detail viewer.

## Phase 2: Deck Builder

Status: done

- Deck model.
- Main/Ride/G zones.
- Deck validation.
- Save/load decks.
- Deck code import/export.

## Phase 3: Manual Play Table

Status: done

- Game state model.
- Zones.
- Manual actions.
- Action log.
- Undo/replay.
- Basic Gift marker actions.

## Phase 4: Baseline Bot

Status: done

- Legal action generator.
- Easy bot.
- Aggro/Balanced/Defensive profiles.
- Deterministic profile choices.

## Phase 5: Rules Core Hardening

Status: done through M11 closeout; M12 structured ability data next

- RulesCore command/query facade.
- Shared legal action mask.
- Hidden state / observation masking.
- Seeded RNG service.
- Snapshot / rollback.
- AbilityCore foundation.
- Trigger resolver foundation.
- Resource ledger foundation.
- Core guardrail test fixtures.
- Timing/window audit.
- Phase / Timing Matrix.
- RulesCore command facade coverage.
- Legal action mask usage report.

## Phase 6: Mobile Readiness

Status: done

- Image cache strategy.
- Responsive play table/card browser pass.
- Android build smoke test.

## Phase 6.1: UI / Mobile UX Polish

Status: partial; M16-01 pending AUTO PlayTable panel polish, M16-02 trigger
panel polish, M16-03 manual resolution panel polish, M16-04 event/replay
panel polish, M16-05 online status/cursor panel polish, M16-06 Deck Builder
filter polish, M16-07 Card Browser Thai search polish, M16-08 broken image
fallback, M16-09 Android touch/layout QA, and M16-10 Windows + Android smoke
flow are done.

- Pending AUTO panel polish. Done.
- Trigger panel polish. Done.
- Manual resolution panel polish. Done.
- Event/replay panel polish. Done.
- Online status/cursor panel polish. Done.
- Deck builder filter polish. Done.
- Card browser Thai search polish. Done.
- Broken image fallback. Done.
- Android touch/layout QA. Done.
- Windows + Android smoke flow. Done.

## Phase 7: Ability Automation

Status: done

- Structured ability definitions.
- Common cost/effect templates.
- Pending auto ability queue.
- Pending auto ability queue model scaffold done.
- Pending auto ability queue GameState integration scaffold done.
- Pending AUTO real timing event integration done.
- Pending AUTO manual-resolved event metadata done.
- M10 ability/trigger automation closeout done.
- Ability trigger event collector scaffold done.
- Ability trigger collector GameState adapter scaffold done.
- Pending auto ability queue masking scaffold done.
- Pending auto ability queue payload codec scaffold done.
- Pending auto ability queue Photon wrapper scaffold done.
- Pending auto ability queue transport hook scaffold done.
- Pending auto ability queue session storage scaffold done.
- Pending auto ability queue session publish helper scaffold done.
- Pending auto ability PlayTable summary surface scaffold done.
- Pending auto ability PlayTable publish control scaffold done.
- Pending auto ability decoded item list formatter scaffold done.
- Pending auto ability PlayTable item list surface scaffold done.
- Pending auto ability selection state scaffold done.
- Pending auto ability selection status formatter scaffold done.
- Pending auto ability PlayTable selection status surface scaffold done.
- Pending auto ability PlayTable selection cycle control scaffold done.
- Pending auto ability PlayTable clear selection control scaffold done.
- Pending auto ability selected resolution request model scaffold done.
- Pending auto ability selected resolution request formatter scaffold done.
- Pending auto ability PlayTable selected resolution request preview scaffold done.
- Pending auto ability selected resolution request payload codec scaffold done.
- Pending auto ability selected resolution request Photon wrapper scaffold done.
- Pending auto ability selected resolution request transport hook scaffold done.
- Pending auto ability selected resolution request session storage scaffold done.
- Pending auto ability selected resolution request session publish helper scaffold done.
- Pending auto ability PlayTable selected resolution request publish control scaffold done.
- Pending auto ability selected resolution request summary surface scaffold done.
- Pending auto ability selected resolution request list formatter scaffold done.
- Pending auto ability selected resolution request PlayTable list surface scaffold done.
- Pending auto ability manual resolution decision model scaffold done.
- Pending auto ability manual resolution decision payload codec scaffold done.
- Pending auto ability manual resolution decision Photon wrapper scaffold done.
- Pending auto ability manual resolution decision transport hook scaffold done.
- Pending auto ability manual resolution decision session storage scaffold done.
- Pending auto ability manual resolution decision session publish helper scaffold done.
- Pending auto ability manual resolution decision PlayTable publish control scaffold done.
- Pending auto ability manual resolution decision summary formatter scaffold done.
- Pending auto ability manual resolution decision PlayTable summary surface scaffold done.
- Pending auto ability manual resolution decision list formatter scaffold done.
- Pending auto ability manual resolution decision PlayTable list surface scaffold done.
- Pending auto ability manual resolution decision publish result formatter scaffold done.
- Pending auto ability manual resolution decision draft factory scaffold done.
- Pending auto ability manual resolution decision PlayTable draft control scaffold done.
- Pending auto ability manual resolution decision type selector scaffold done.
- Pending auto ability manual resolution decision draft result formatter scaffold done.
- Pending auto ability manual resolution decision validation scaffold done.
- Pending auto ability manual resolution decision validation result formatter scaffold done.
- Pending auto ability manual resolution decision PlayTable validation preview scaffold done.
- Pending auto ability manual resolution apply command contract scaffold done.
- Pending auto ability manual resolution apply command validator scaffold done.
- Pending auto ability manual resolution apply result formatter scaffold done.
- Pending auto ability manual resolution apply executor scaffold done.
- Pending auto ability manual resolution session apply preview scaffold done.
- Pending auto ability manual resolution PlayTable apply preview control scaffold done.
- Pending auto ability manual resolution apply preview log entry scaffold done.
- Pending auto ability manual resolution apply preview log formatter scaffold done.
- Pending auto ability manual resolution apply preview session log storage done.
- Pending auto ability manual resolution apply preview PlayTable surface done.
- Pending auto ability manual resolution apply preview flow verification done.
- Pending auto ability queue commit policy spec done.
- Pending auto ability queue commit helper done.
- Pending auto ability queue commit event model done.
- Trigger check commit event model done.
- Trigger allocation commit helper done.
- Trigger modifier cleanup integration done.
- Dynamic power/critical tracking. Modifier ledger, projection, and cleanup
  timing scaffolds done.
- Trigger allocation automation. Resolver and allocation plan scaffolds done.
- Trigger check resolution bundle scaffold done.
- Trigger check log entry scaffold done.
- Trigger check replay log scaffold done.
- Trigger check replay log masking scaffold done.
- Trigger check replay payload codec scaffold done.
- Trigger check Photon payload wrapper scaffold done.
- Trigger check transport hook scaffold done.
- Trigger check session storage scaffold done.
- Trigger check PlayTable UI surface scaffold done.
- Trigger check manual publish control scaffold done.
- Trigger check manual draft payload scaffold done.
- Trigger check draft session publish helper done.
- Trigger check draft PlayTable control scaffold done.
- Trigger check draft type selector scaffold done.
- Trigger check draft source selector scaffold done.
- Trigger check draft index selector scaffold done.
- Trigger check draft summary panel scaffold done.
- Trigger check draft selected-card summary scaffold done.
- Trigger check draft selected-zone summary scaffold done.
- Trigger check draft clear-selection control scaffold done.
- Trigger check draft selected-card status helper scaffold done.
- Trigger check draft metadata formatter scaffold done.
- Trigger check draft full summary formatter scaffold done.
- Trigger check draft control-state helper scaffold done.
- Trigger check draft selection validation helper scaffold done.
- Trigger check draft publish result message formatter scaffold done.
- Trigger check draft selector cycle helper scaffold done.
- Trigger check draft status message formatter scaffold done.
- Trigger check draft request factory scaffold done.
- Trigger check log publish result formatter scaffold done.
- Trigger check log summary formatter scaffold done.
- Trigger check log publish online validation helper scaffold done.
- PlayTable mode summary formatter scaffold done.
- PlayTable card selection status formatter scaffold done.
- PlayTable action status formatter scaffold done.
- PlayTable event log formatter scaffold done.

## Phase 8: Smarter Bot And Combo Tools

Status: partial; exact trigger probability engine, board/resource evaluator, battle sequence search prototype/v2, guard estimator, playbook model/integration, combo discovery output, heuristic bot v2, guard decision bot, trigger-risk attack choice, snapshot simulation path, bot debug trace, ISMCTS readiness gate, bounded advanced search prototype, and offline clan/nation combo pairing for the requested classic, Link Joker/Legion, G, V, D, and DZ era presets are done.

- Board/resource evaluator. Done.
- Exact trigger probability engine. Done.
- Battle sequence search. Prototype done.
- Opponent guard predictor. Shield estimator done.
- Archetype playbooks. Model done.
- Offline combo discovery. Scaffold done.
- Heuristic bot v2. Done.
- Guard decision bot. Done.
- Trigger-risk attack choice. Done.
- Battle sequence search v2. Done.
- Snapshot simulation path. Done.
- Archetype playbook integration. Done.
- Offline combo discovery output. Done.
- Offline clan combo pairing by early set/clan. Done.
- Bot debug trace. Done.
- ISMCTS readiness gate. Done.
- Advanced search prototype. Done.

## Phase 9: Custom Packs And Custom Formats

Status: done for Master Plan V2 M15; custom pack v1/importer, v2 schema
envelope, v2 ability data validation, read-only pack validation status, and
pure pack enable/disable registry are done. Custom format profile model,
Standard/V-Premium/Premium preset flags, read-only custom format sandbox, and
M15 closeout validation are done. Custom format gameplay enforcement remains
future.

- Custom card pack schema.
- Custom pack v2 schema envelope. Done.
- Ability data in custom packs. Done.
- Pack validation UI/status. Done.
- Pack manager enable/disable. Done.
- RuleSet/custom format profile model. Done.
- Standard format flags. Done.
- V-Premium format flags. Done.
- Premium format flags. Done.
- Custom sandbox validation preview. Done.
- Custom format validation closeout. Done.
- CSV/XLSX import.
- Image ZIP import.
- RuleSet/format flags.
- Custom sandbox validation.

## Phase 10: Multiplayer And Tournament Foundation

Status: current; event sync, Photon foundation, hidden-state views,
owner-private room initialization, explicit trusted-client UX, command envelope
payloads, command envelope state validation, public event masking delivery,
public reconnect recovery, deck hash start guards, and room lifecycle helpers
are done. Public spectator replay sync from public event batches is done.
Public-safe tournament audit log export is done.
Live Photon gameplay smoke now covers lifecycle start/play/reconnect/end.

- Event sync protocol.
- Server-authoritative rule validation.
- Card pack hash check.
- Owner-private commitment room initialization with local true state and
  opponent hidden placeholders.
- Explicit trusted-client warning/acknowledgment for commitment rooms.
- Online command envelope payload with player id, sequence, room game id, and
  state cursor.
- Stale cursor, player ownership, actor mismatch, and out-of-turn command
  validation.
- Public event application to masked public/opponent views without true
  opponent card ids.
- Public reconnect batches for commitment rooms and true reconnect block for
  non-shared deck privacy.
- Shared deck-code room start validates imported deck code against canonical
  deck hash metadata.
- Room ready/start/end/rematch lifecycle transitions with no source mutation.
- Spectator/replay viewer with public-event state sync.
- Tournament audit log with public-safe room/pack/player/event/result export.
- Live Photon smoke for start/play/reconnect/end.

## Phase 11: Research Platform

Status: done; M17-01 headless CLI runner, M17-02 bounded CLI input,
M17-03 replay/result output, M17-04 batch runner, M17-05 dataset export
schema, M17-06 observation/action/reward API, M17-07 performance profiling
harness, M17-08 packed state decision gate, M17-09 distributed worker
prototype spec, and M17-10 local worker prototype are done.

- Headless CLI runner. Done.
- CLI deck/ruleset/seed input. Done.
- CLI replay/result output. Done.
- Batch self-play. Done.
- Dataset export. Done.
- RL-compatible observation/action/reward API. Done.
- Performance profiling harness. Done.
- Packed state decision gate. Done.
- Distributed worker prototype spec. Done.
- Distributed worker prototype. Done.

## Phase 12: QA / CI / Release

Status: current; M18-01 CI Python tests, M18-02 CI data validation,
M18-03 CI Unity compile, M18-04 CI Unity EditMode tests, M18-05 Core
regression suite, M18-06 Ability regression suite, M18-07 Multiplayer
payload/no-leak tests, M18-08 Windows build artifact, M18-09 Android build
artifact, and M18-10 Release candidate checklist are done.
Post-M18 Windows player hotfix is done: runtime pack is copied beside the
`.exe`, nested artifact pack paths resolve, current UGUI input receives clicks,
normal launch log smoke found no runtime error patterns, and
`-vanguardPlayerSmoke` passed card browser/deck/Play Table/layout checks from
the built player.

- CI Python tests. Done.
- CI data validation. Done.
- CI Unity compile. Done.
- CI Unity EditMode tests. Done.
- Core regression suite. Done.
- Ability regression suite. Done.
- Multiplayer payload/no-leak tests. Done.
- Windows build artifact. Done.
- Android build artifact. Done.
- Release candidate checklist. Done.

## Phase 13: Player Experience Reset

Status: superseded by the Windows-first program-completion phase. M19 remains
historical evidence for the first player-experience reset. Current active work
continues this direction through M21-M27 with Android/mobile/release work
deferred.

- M19-01 UI audit and screen map closeout.
- M19-02 Home/Lobby shell.
- M19-03 Deck Builder dedicated layout.
- M19-04 Deck load/save/code dialog.
- M19-05 PlayTable zone-first layout.
- M19-06 Advanced PlayTable drawer for debug/network/automation tools.
- M19-07 Loading, empty, and error states.
- M19-08 Windows and Android visual smoke.
- M19-09 User-provided icon override loader with semantic-key fallback and no
  proprietary assets.

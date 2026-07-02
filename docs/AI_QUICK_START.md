# AI Quick Start

ไฟล์นี้คือทางเข้าแรกสำหรับ AI ทุกตัวที่เข้ามาช่วยโปรเจกต์นี้ อ่านไฟล์นี้ก่อนเพื่อเข้าใจว่าโปรเจกต์คืออะไร ตอนนี้อยู่ตรงไหน ทำอะไรต่อได้ และห้ามทำอะไร

## Project Snapshot

เรากำลังสร้างโปรแกรม Cardfight!! Vanguard ภาษาไทยที่เล่นได้บนคอมและมือถือ

Stack ปัจจุบัน:

- Main client: Unity `6000.5.0f1` + C#
- Local database: SQLite
- Data/build tools: Python
- Multiplayer scaffold: Photon Realtime SDK, ตอนนี้เป็น trusted-client/casual foundation
- Runtime card pack: `data/packs/vanguard_th/`
- Main Unity project: `client/unity/VanguardThaiSim/`

Card data source of truth:

- Card JSON: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- Card images: `outputs/kk_cardfight_export/data/images/`
- Verification: `outputs/kk_cardfight_export/data/verification_report.json`
- Runtime SQLite: `data/packs/vanguard_th/cards.sqlite`

Current verified data:

- Vanguard TH cards: `10,836`
- Card images: `10,836`

## Current Direction

Active reset as of 2026-06-27:

```text
Windows-first program completion
-> finish Home -> Deck Builder -> PlayTable -> Replay / Online Room on Windows
-> defer Android, mobile QA, APK, app packaging, release-candidate packaging,
   and public distribution until the user explicitly re-enables that track
-> M32 PlayTable UI work is paused by user instruction
-> current target: M62-01 seventh fixture schema validator
   after M61-closeout; M58-01 through M61-closeout
   spec/tool/tests scaffolds are ready
-> first slice closed through M35-D4 reviewed playbook seed export for selected
   Classic Core / Nova Grappler
-> second slice selected by M35-E1: Classic Core / Oracle Think Tank
-> second slice fixture readiness closed by M35-E2; Classic Core policy is reusable
-> selected-report semantic/compatibility probe closed by M35-E3 for Oracle
   Think Tank: 103 cards, 2660 edges, 259 candidate edges
-> active M33 scope: Python offline logic, runtime SQLite card data, grouped by
   clan, advisory combo-pair report only
-> latest PlayTable target closed: M32-04 playmat slot layout / visual evidence closeout
-> latest combo logic target closed: M33-01 TD01-TD06 / BT01-BT09 / EB01-EB05
   clan combo pairing report
-> latest era combo target closed: M33-02 Link Joker/Legion, G, V, D, and DZ
   preset report generation
-> latest matrix target closed: M33-03 combo matrix CSV/JSON artifacts
-> latest deck-analysis target closed: M34-01 deck possibility analysis by
   clan/nation group
-> latest planning target closed: M34-02 2026-06-29 rule corpus intake and
   deck/combo plan refresh
-> latest ranking target closed: M34-03 deck-feasible archetype priority v2 /
   Phase A1
-> latest target-slice target closed: M35-A2 first target slice selection +
   format policy + taxonomy gap report
-> selected first slice: Classic Core / โนว่า เกรปเปอร์
-> latest first-slice legality target closed: M35-A3 minimal deck legality
   fixtures for Classic Core / โนว่า เกรปเปอร์
-> latest Phase A target closed: M35-A4 first-slice feasibility report refresh
   reports Phase A ready for Phase B
-> latest semantic target closed: M35-B1 semantic vocabulary for Classic Core /
   โนว่า เกรปเปอร์
-> latest semantic extraction target closed: M35-B2 offline semantic extractor
   for Classic Core / โนว่า เกรปเปอร์
-> latest semantic model target closed: M35-B3 requirement/provider model for
   Classic Core / โนว่า เกรปเปอร์
-> latest semantic review target closed: M35-B4 manual review queue for Classic
   Core / โนว่า เกรปเปอร์
-> latest compatibility target closed: M35-C1 pair compatibility graph for
   Classic Core / โนว่า เกรปเปอร์
-> latest resource target closed: M35-C2 resource conflict detector for
   Classic Core / โนว่า เกรปเปอร์
-> latest timing target closed: M35-C3 timing compatibility detector for
   Classic Core / โนว่า เกรปเปอร์
-> latest zone target closed: M35-C4 zone/target compatibility detector for
   Classic Core / โนว่า เกรปเปอร์
-> latest compatibility output closed: M35-C5 selected-slice compatibility
   output for Classic Core / โนว่า เกรปเปอร์
-> latest package target closed: M35-D1 candidate package selection for
   Classic Core / โนว่า เกรปเปอร์
-> latest skeleton target closed: M35-D2 deck skeleton ratio planner for
   Classic Core / โนว่า เกรปเปอร์
-> latest combo line target closed: M35-D3 combo line explainer for
   Classic Core / โนว่า เกรปเปอร์
-> latest reviewed seed target closed: M35-D4 reviewed playbook seed export for
   Classic Core / Nova Grappler
-> latest second-slice target closed: M35-E1 second slice selection for
   Classic Core / Oracle Think Tank
-> latest second-slice fixture target closed: M35-E2 fixture/format readiness
   for Classic Core / Oracle Think Tank
-> latest generalized pipeline target closed: M35-E3 semantic/compatibility
   probe for Classic Core / Oracle Think Tank
-> latest bot gate target closed: M35-E4 bot integration gate; 1 reviewed
   future hint candidate, M35-E3 probe edges blocked from runtime bot use, and
   runtime bot integration remains disabled
-> latest phase closeout closed: M35-closeout Hybrid Vertical-Slice closeout;
   M35 is complete and next queue is M36 Human-review-assisted deck recipe
   validation
-> active deck/combo plan: M41 Second-slice human repair review gate after
   M40 second-slice runtime readiness closeout
-> latest M36 target closed: M36-01 first-slice review packet; 31 review
   items exported for accepted seed, rejected combo lines, and manual-review
   cards
-> latest M36 target closed: M36-02 deck recipe draft model; 25 advisory
   recipe drafts exported, including 1 accepted-seed draft, 24 rejected-line
   drafts, and 16 drafts with slot gaps for validator review
-> latest M36 target closed: M36-03 deck recipe validator; 25 drafts checked,
   0 runtime-ready recipes, 0 missing-card recipes, and 0 copy-limit
   violations
-> latest M36 target closed: M36-04 combo-line to recipe consistency check;
   25 combo lines have recipe cards present, 0 missing combo-card checks, and
   0 promotable lines
-> latest M36 target closed: M36-05 second-slice readiness comparison; second
   slice is ready for future offline recipe work, but runtime/bot scale-out
   remains disabled
-> latest M36 target closed: M36-closeout deck recipe validation closeout; M36
   produced reviewable recipe artifacts but 0 runtime-ready recipes and 0
   promotable combo lines
-> latest M37 target closed: M37-01 accepted seed slot-gap completion
   candidates; accepted seed recipe_003 has 18 source-backed trigger
   candidate cards and 5 advisory completion packages
-> latest M37 target closed: M37-02 trigger package repair proposal; recommended
   advisory package is m37_01_pkg_001 / balanced_classic, resolving trigger
   blockers while leaving grade profile review and human acceptance pending
-> latest M37 target closed: M37-03 rejected-line support-gap triage; 24
   rejected lines classified into 5 multi-label support-gap groups
-> latest M37 target closed: M37-04 manual semantic mapping candidates; 5
   non-executable mapping candidates generated from 49 triage line links
-> latest M37 target closed: M37-05 revised recipe validation rerun; accepted
   seed trigger blockers clear in-memory, but status remains
   validator_passed_pending_human_acceptance
-> latest M37 target closed: M37-closeout first runtime-ready recipe decision;
   recipe_003 remains advisory because human acceptance and grade-profile review
   are still open
-> latest M38 target closed: M38-01 accepted seed human review packet; packet
   exported for recipe_003 but does not record acceptance
-> latest M38 target closed: M38-02 grade profile repair candidates; 2
   substitution-preview packages can reach G0=17/G1=14/G2=11/G3=8
-> latest M38 target closed: M38-03 human-accepted recipe artifact; recipe_003
   has accepted trigger and grade repairs, blockers=0, and is ready for M38-04
-> latest M38 target closed: M38-04 runtime fixture promotion gate; all 5 gate
   checks passed and an offline runtime/test fixture artifact was created
-> latest M38 target closed: M38-closeout first runtime fixture closeout;
   recipe_003 enters offline fixture scope only and next queue is M39
-> latest M39 target closed: M39-01 offline fixture schema validator; fixture
   schema is valid, blockers=0, and counts were recomputed from SQLite
-> latest M39 target closed: M39-02 fixture-to-deck text exporter; reviewable
   count-line deck text was generated with 17 importable card lines and no
   runtime/UI/bot/GameState mutation
-> latest M39 target closed: M39-03 headless fixture load smoke; fixture deck
   code was generated, Unity headless accepted it with deck_source=deck_code,
   actions=4, events=4, and no saved deck/UI/bot promotion occurred
-> latest M39 target closed: M39-04 second-slice recipe scale decision; Oracle
   Think Tank is allowed to enter an offline recipe pipeline only, while saved
   deck publication, runtime promotion, and bot/playbook promotion remain
   blocked
-> latest M40 target closed: M40-01 second-slice review packet; 6 fixture
   notes, 7 manual-review cards, and 259 candidate edges were exported for
   Oracle Think Tank review, with runtime/saved deck/UI/bot promotion still
   blocked
-> latest M40 target closed: M40-02 second-slice recipe draft model; 25
   pair-anchored, fixture-scaffolded advisory recipe drafts were generated for
   Oracle Think Tank, all quantity-complete at 50 cards with 16 triggers, while
   runtime/saved deck/UI/bot promotion remains blocked
-> latest M40 target closed: M40-03 second-slice recipe validator; all 25
   Oracle Think Tank advisory drafts validate for card existence, copy limit,
   main count, trigger count, and clan identity, but 25 remain blocked by
   manual-review card overlap and 0 are runtime-ready
-> latest M40 target closed: M40-04 second-slice combo-to-recipe consistency;
   all 25 Oracle Think Tank drafts contain their candidate edge pair cards,
   but 25 recipe-level manual-review dependencies keep promotion blocked
-> latest M40 target closed: M40-05 second-slice blocker repair candidates;
   25 advisory repair items were generated, grade-profile candidates can clear
   manual overlap for human review, but no draft mutation, acceptance,
   runtime/saved deck/UI/bot promotion is allowed
-> latest M40 target closed: M40-closeout second-slice runtime readiness
   decision; M40 is complete, but Oracle Think Tank has 0 runtime-ready
   recipes and 0 promotion-allowed checks, so it remains advisory and moves to
   M41 human repair review
-> latest M41 target closed: M41-01 second-slice human repair review packet;
   25 repair candidates were exported for review, but no acceptance, draft
   mutation, runtime fixture, saved deck/UI, or bot/playbook promotion occurred
-> latest M41 target closed: M41-02 second-slice human-accepted repair
   artifact; m40_recipe_001 acceptance was recorded for validation rerun, but
   the artifact does not declare the recipe valid and does not promote runtime
-> latest M41 target closed: M41-03 second-slice repaired recipe validation
   rerun; m40_recipe_001 is invalid because trigger count is 2/16, so M41-04
   is blocked until a repair loop fixes the trigger profile
-> latest M41 target closed: M41-repair second-slice trigger/profile repair
   candidates; 3 complete trigger repair packages were exported, including a
   balanced 16-trigger package, but no acceptance/runtime promotion occurred
-> latest M41 target closed: M41-repair-accept second-slice trigger repair
   acceptance artifact; balanced package m41_repair_pkg_001 was accepted for
   validation rerun without declaring the recipe valid
-> latest M41 target closed: M41-repair-validate second-slice repaired recipe
   validation after trigger repair; m40_recipe_001 passed with 50 cards, 16
   triggers, target grade profile, 0 blockers, and is ready for M41-04
-> latest M41 target closed: M41-04 second-slice runtime fixture promotion
   gate; 6 gate checks passed and an offline runtime/test fixture artifact was
   created without saved deck, UI deck list, bot/playbook, or GameState
   mutation
-> latest M41 target closed: M41-closeout second-slice fixture closeout;
   Oracle Think Tank enters offline runtime/test fixture scope only and next
   queue is M42
-> latest M42 target closed: M42-01 second fixture schema validator; Oracle
   Think Tank fixture schema is valid, blockers=0, counts were recomputed from
   SQLite, and runtime/UI/saved-deck/bot mutation remains disabled
-> latest M42 target closed: M42-02 second fixture deck text exporter; reviewable
   count-line deck text was generated with 15 importable card lines and no
   saved deck/UI/bot/GameState mutation
-> latest M42 target closed: M42-03 second fixture headless load smoke; Unity
   headless accepted the generated Oracle Think Tank deck code with
   deck_source=deck_code, actions=4, events=4, and no saved deck/UI/bot
   mutation
-> latest M42 target closed: M42-04 multi-fixture scale decision; two fixtures
   passed offline and Unity smoke, third-slice offline pipeline is allowed,
   but no third slice is selected yet and live/UI/saved-deck/bot use remains
   disabled
-> latest M43 target closed: M43-01 third target slice selection; selected
   เบอร์มิวด้า ไทรแองเกิล for offline analysis only with no recipe/runtime/UI/bot
   mutation
-> latest M43 target closed: M43-02 third-slice fixture/format readiness;
   127 source-backed cards, grade 0-3 coverage, trigger capacity 84, and
   EB06/EB10 source coverage passed, while runtime/UI/saved-deck/bot use
   remains disabled
-> latest M43 target closed: M43-03 third-slice semantic/compatibility probe;
   127 semantic cards, 61 manual-review cards, 4835 pair edges, and 109
   candidate edges passed advisory probe readiness, while runtime/UI/saved-deck
   and bot promotion remain disabled
-> latest M43 target closed: M43-04 third-slice recipe pipeline entry gate;
   offline M44 pipeline is allowed with blockers=0, but fixture scaffold is
   required before recipe validation and runtime/UI/saved-deck/bot promotion
   remains disabled
-> latest M44 target closed: M44-01 third-slice fixture scaffold; validator
   scaffold is source-backed with 50-card main deck policy, 16 triggers,
   4/4/4/4 recommended trigger profile, grade 0-3 coverage, and no
   runtime/UI/saved-deck/bot promotion
-> latest M44 target closed: M44-02 third-slice review packet; exported 1
   fixture scaffold item, 61 manual-review cards, 109 candidate edges, and
   171 total review items without recipe/runtime/UI/bot promotion
-> latest M44 target closed: M44-03 third-slice recipe draft model; generated
   25 quantity-complete advisory drafts with 50 cards and 16 triggers each,
   all still blocked by manual-review overlap and no runtime/UI/saved-deck/bot
   promotion
-> latest M44 target closed: M44-04 third-slice recipe validator; validated
   all 25 advisory drafts with 0 missing-card, copy-limit, slot-gap, or
   trigger-count blockers, while 25 manual-review overlap blockers keep
   runtime-ready recipes at 0
-> latest M44 target closed: M44-05 third-slice combo-to-recipe consistency;
   checked all 25 drafts, confirmed candidate pair cards are present in all
   drafts, and kept promotion_allowed at 0 because recipe-level manual-review
   dependencies remain
-> latest M44 target closed: M44-06 third-slice blocker repair candidates;
   generated 25 advisory repair items, 25 complete manual same-grade repair
   packages, and 25 complete grade-profile repair packages, while runtime
   promotion remains disabled
-> latest M44 target closed: M44-closeout third-slice runtime readiness
   decision; M44 is complete, runtime-ready recipe remains unavailable, and
   next queue is M45 human repair review gate
-> latest M45 target closed: M45-01 third-slice human repair review packet;
   exported 25 review items, 25 complete manual repair packages, 25 complete
   grade-profile candidates, runtime promotion disabled, and ready_for_m45_02
   true
-> latest M45 target closed: M45-02 third-slice human-accepted repair artifact;
   accepted m45_01_m44_recipe_001_repair_review, detected 2 source grade-package
   conflicts after manual substitution, recomputed the combined grade repair,
   produced a 50-card preview with grade counts 0:17/1:14/2:11/3:8, and kept
   runtime promotion disabled
-> latest M45 target closed: M45-03 third-slice repaired recipe validation
   rerun; validated accepted recipe m44_recipe_001, validator_passed=1,
   runtime_ready=1, no missing/copy-limit/slot/trigger/manual-overlap/
   grade-profile issues, runtime fixture still not created, and
   ready_for_m45_04=true
-> latest M45 target closed: M45-04 third-slice runtime fixture promotion
   gate; promotion_allowed=true, 7/7 gate checks passed, offline fixture
   artifact created at outputs/target_slice/runtime_fixtures/
   m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json, and
   saved-deck/UI/bot/GameState mutation remains disabled
-> latest M45 target closed: M45-closeout third-slice fixture closeout;
   m45_complete=true, third runtime fixture available, live runtime deck UI /
   saved deck / UI deck list / bot playbook remain disabled, and next queue is
   M46 third fixture consumption and multi-fixture scale gate
-> latest M46 target closed: M46-01 third fixture schema validator;
   schema_valid=true, blockers=0, main deck count 50, trigger profile
   Critical=4/Draw=4/Heal=4/Stand=4, grade profile 0:17/1:14/2:11/3:8,
   runtime UI/saved-deck/bot/GameState mutation disabled, and ready_for_m46_02
   true
-> latest M46 target closed: M46-02 third fixture deck text exporter;
   export_ready=true, blockers=0, 15 card lines exported to
   outputs/target_slice/m46_02_third_fixture_deck_text_export.txt,
   review-only boundary preserved, and ready_for_m46_03 true
-> latest M46 target closed: M46-03 third fixture headless load smoke;
   offline_load_ready=true, unity_headless_smoke_passed=true,
   deck_source=deck_code, actions_executed=4, event_count=4, blockers=0,
   saved-deck/UI/bot/GameState mutation disabled, and ready_for_m46_04 true
-> latest M46 target closed: M46-04 three-fixture scale decision;
   ready_for_m47=true, passed_fixture_count=3, failed_fixture_count=0,
   candidate_count=5, fourth-slice offline pipeline allowed, and runtime UI /
   saved-deck / bot / GameState mutation disabled
-> latest M47 target closed: M47-01 fourth target slice selection;
   selected group รอยัล พาลาดิน, era preset g_series_first, offline analysis
   only, and recipe/runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest M47 target closed: M47-02 fourth-slice fixture/format readiness;
   source_card_count=71, trigger gap Heal, all_fixture_expectations_met=false,
   repair_required=true, and recipe/runtime/UI/saved-deck/bot/GameState
   mutation disabled
-> latest M47 target closed: M47-repair fourth-slice readiness blocker repair;
   same-group Heal triggers exist outside selected scope, recommended action
   review_same_group_source_expansion, and card data/runtime/UI/saved-deck/bot/
   GameState mutation disabled
-> latest target closed: M49-01 fourth-slice human repair and G Zone review
   packet; 25 review items, 25 G Zone decision items, ready_for_m49_02 true,
   runtime/card data/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M49-02 fourth-slice G Zone support decision;
   selected main_deck_only_for_current_windows_fixture, 25 decision items,
   G Zone runtime false, Stride runtime false, runtime promotion false, and
   ready_for_m49_03 true
-> latest target closed: M49-03 fourth-slice human-accepted repair artifact;
   accepted m48_recipe_001 under the main-deck-only G Zone boundary, 50 cards,
   grade profile 17/14/11/8, repair issues 0, runtime promotion false, and
   ready_for_m49_04 true
-> latest target closed: M49-04 fourth-slice repaired recipe validation rerun;
   validator_passed 1, runtime_ready 1, issue_counts {}, G Zone runtime false,
   Stride runtime false, runtime promotion false, and ready_for_m49_05 true
-> latest target closed: M49-05 fourth-slice runtime fixture gate;
   promotion_allowed true, passed checks 8, fixture created true, G Zone runtime
   false, Stride runtime false, saved-deck/UI/bot/GameState mutation disabled,
   and ready_for_m49_closeout true
-> latest target closed: M49-closeout fourth-slice fixture closeout;
   M49 complete true, fourth fixture available true, next queue M50, G Zone
   runtime false, Stride runtime false, saved-deck/UI/bot/GameState mutation
   disabled
-> latest target closed: M50-01 fourth fixture schema validator;
   schema_valid true, blockers 0, main deck 50, unique cards 14, trigger
   profile 4/4/4/4, grade profile 17/14/11/8, G Zone runtime false, Stride
   runtime false, and ready_for_m50_02 true
-> latest target closed: M50-02 fourth fixture deck text exporter;
   export_ready true, blockers 0, main deck 50, exported card lines 14, G
   section comment-only, and ready_for_m50_03 true
-> latest target closed: M50-03 fourth fixture headless load smoke;
   offline_load_ready true, deck_code_created true, Unity headless accepted
   deck_source deck_code, actions/events 4/4, G Zone count 0, and
   ready_for_m50_04 true
-> latest target closed: M50-04 four-fixture scale decision;
   fixture evidence 4, passed fixtures 4, failed fixtures 0, candidates 5,
   fifth-slice offline pipeline allowed true, G Zone runtime false, Stride
   runtime false, and ready_for_m51 true
-> latest target closed: M51-01 fifth target slice selection;
   selected group โกลด์ พาลาดิน, era preset link_joker_legion_mate, offline
   analysis only, G Zone runtime false, Stride runtime false, and
   runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M51-02 fifth-slice fixture/format readiness;
   source_card_count 106, trigger capacity 36, non-trigger capacity 388,
   trigger gaps [], all fixture expectations met true, semantic probe ready
   true, and runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M51-03 fifth-slice semantic/compatibility probe;
   semantic cards 106, manual-review cards 4, pair graph edges 3075,
   candidate edges 142, all stage readiness passed true, and
   runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M51-04 fifth-slice recipe pipeline entry gate;
   offline recipe pipeline allowed true, blockers 0, ready_for_m52 true,
   fixture scaffold required true, and runtime/UI/saved-deck/bot/GameState
   mutation disabled
-> latest target closed: M52-01 fifth-slice fixture scaffold;
   source-backed cards 106, trigger capacity 36, non-trigger capacity 388,
   candidate edges 142, scaffold_ready true, blockers 0, ready_for_m52_02
   true, and runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M52-02 fifth-slice review packet;
   fixture scaffold items 1, manual-review cards 4, candidate edges 142,
   total review items 147, ready_for_m52_03 true, and runtime/UI/saved-deck/
   bot/GameState mutation disabled
-> latest target closed: M52-03 fifth-slice recipe draft model;
   recipe drafts 25, quantity-complete recipes 25, trigger/missing skipped
   edges 0, manual-overlap recipes 0, ready_for_m52_04 true, and runtime/UI/
   saved-deck/bot/GameState mutation disabled
-> latest target closed: M52-04 fifth-slice recipe validator;
   recipes validated 25, runtime-ready recipes 0, validator-passed pending
   human selection 25, invalid drafts 0, manual-review overlap recipes 0,
   grade-profile review recipes 25, ready_for_m52_05 true, and runtime/UI/
   saved-deck/bot/GameState mutation disabled
-> latest target closed: M52-05 fifth-slice combo-to-recipe consistency;
   consistency checks 25, pair cards present 25, missing pair-card checks 0,
   promotion allowed 0, status consistent_pending_human_selection 25,
   ready_for_m52_06 true, and runtime/UI/saved-deck/bot/GameState mutation
   disabled
-> latest target closed: M52-06 fifth-slice blocker repair candidates;
   repair candidates 25, complete grade-profile candidates 25, human selection
   required 25, unexpected structural blockers 0, ready_for_m52_closeout true,
   and runtime/UI/saved-deck/bot/GameState mutation disabled
-> latest target closed: M52-closeout fifth-slice runtime readiness decision;
   M52 complete true, runtime-ready recipe available false, human selection
   review allowed true, next queue M53, and runtime/UI/saved-deck/bot/GameState
   mutation disabled
-> latest target closed: M53-01 fifth-slice human repair review packet;
   review items 25, complete grade-profile candidates 25, human selection
   required 25, ready_for_m53_02 true, and runtime/UI/saved-deck/bot/GameState
   mutation disabled
-> latest target closed: M53-02 fifth-slice human-selected recipe artifact;
   selected review item m53_01_m52_recipe_001_repair_review, recipe
   m52_recipe_001, grade package m52_recipe_001_grade_profile_pkg_001,
   ready_for_m53_03 true, runtime promotion disabled
-> latest target closed: M53-03 fifth-slice human-accepted repair artifact;
   accepted selected recipe m52_recipe_001, repaired main deck count 50,
   repair application issues 0, ready_for_m53_04 true, recipe validity and
   runtime promotion still not declared
-> latest target closed: M53-04 fifth-slice repaired recipe validation rerun;
   validation validator_passed, consistency consistent_validator_passed,
   runtime-ready recipes 1, blockers 0, ready_for_m53_05 true, rerun is
   in-memory only
-> latest target closed: M53-05 fifth-slice runtime fixture promotion gate;
   promotion_allowed true, passed checks 5, failed checks 0, offline fixture
   created at outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json
-> latest target closed: M53-closeout fifth-slice fixture closeout;
   M53 complete true, fifth runtime fixture available true, next queue M54,
   saved deck/UI/bot/GameState mutation disabled
-> latest target closed: M54-01 fifth fixture schema validator;
   schema valid true, blockers 0, main deck 50, unique cards 16,
   trigger profile Critical/Draw/Heal/Stand 4/4/4/4, grade profile 17/14/11/8,
   ready_for_m54_02 true, targeted tests 8/8, full Python tests 1008/1008
-> latest target closed: M54-02 fifth fixture deck text exporter;
   export ready true, blockers 0, main deck 50, exported card lines 16,
   deck text outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt,
   targeted tests 7/7, full Python tests 1015/1015
-> latest target closed: M54-03 fifth fixture headless load smoke;
   offline load ready true, Unity headless accepted true, deck source deck_code,
   actions/events 4/4, blockers 0, targeted tests 9/9, full Python tests
   1024/1024
-> latest target closed: M54-04 five-fixture scale decision;
   five fixtures passed, failed fixtures 0, candidate count 5,
   sixth-slice offline pipeline allowed true, targeted tests 8/8, full Python
   tests 1032/1032
-> latest target closed: M55-01 sixth target slice selection;
   selected group Shadow Paladin, era g_next_z, rank 4, candidate count 5,
   ready_for_m55_02 true, targeted tests 7/7, full Python tests 1039/1039
-> latest target closed: M55-02 sixth-slice fixture/format readiness;
   source cards 77, grade profile 19/20/16/11/11, triggers Critical/Draw/Heal/Stand
   4/4/2/2, semantic probe ready true, targeted tests 8/8, full Python tests
   1047/1047
-> latest target closed: M55-03 sixth-slice semantic/compatibility probe;
   semantic cards 77, manual-review cards 11, pair graph edges 2069,
   candidate edges 70, all stage readiness true, targeted tests 8/8,
   full Python tests 1055/1055
-> latest target closed: M55-04 sixth-slice recipe pipeline entry gate;
   offline recipe pipeline allowed true, blockers 0, ready_for_m56 true,
   targeted tests 9/9, full Python tests 1064/1064
-> latest target closed: M56-01 sixth-slice fixture scaffold;
   scaffold_ready true, blockers 0, source cards 77, Grade 4 cards advisory
   only until G Zone support, targeted tests 9/9, full Python tests 1073/1073
-> latest target closed: M56-02 sixth-slice review packet;
   review items 82, manual-review cards 11, candidate edges 70,
   ready_for_m56_03 true, targeted tests 8/8, full Python tests 1081/1081
-> latest target closed: M56-03 sixth-slice recipe draft model;
   recipe drafts 12, quantity-complete 12, skipped trigger/Grade4/missing
   edges 58, manual-overlap recipes 12, targeted tests 9/9, full Python tests
   1090/1090
-> latest target closed: M56-04 sixth-slice recipe validator;
   validated drafts 12, runtime-ready 0, manual-review blocked 12,
   missing/copy/slot/trigger/Grade4 blockers 0, targeted tests 7/7,
   full Python tests 1097/1097
-> latest target closed: M56-05 sixth-slice combo-to-recipe consistency;
   consistency checks 12, pair cards present 12, missing pair-card checks 0,
   promotion allowed 0, recipe manual dependencies 12, G Zone deferred 12,
   targeted tests 6/6, full Python tests 1103/1103
-> latest target closed: M56-06 sixth-slice blocker repair candidates;
   repair items 12, manual repair complete 12, grade repair complete 12,
   G Zone deferred 12, ready for human repair review 12, targeted tests 8/8,
   full Python tests 1111/1111
-> latest target closed: M56-closeout sixth-slice runtime readiness decision;
   M56 complete true, runtime-ready recipe available false, next queue M57,
   manual review blocked 12, G Zone deferred 12, targeted tests 9/9,
   full Python tests 1120/1120
-> latest target closed: M57-01 sixth-slice human repair review packet;
   review items 12, complete manual repairs 12, complete grade repairs 12,
   G Zone deferred items 12, targeted tests 10/10, full Python tests 1130/1130
-> active target: M57-02 sixth-slice human-selected recipe artifact
-> M57-02 tooling/spec/tests are scaffolded; targeted tests 9/9 and full
   Python tests 1139/1139 pass, but the real output artifact is still pending
   an explicit M57-01 review item id such as m57_01_m56_recipe_001_repair_review
-> M57-03 tooling/spec/tests are scaffolded; targeted tests 7/7 and full
   Python tests 1146/1146 pass. It applies manual substitutions first, then
   recomputes grade repair after manual conflict detection, but the real
   output artifact remains gated on M57-02
-> M57-04 tooling/spec/tests are scaffolded; targeted M57-03/M57-04 tests
   16/16 and full Python tests 1155/1155 pass. It records an explicit G Zone /
   Stride boundary decision and keeps G Zone, Stride, runtime fixture, saved
   deck, UI publication, bot/playbook, and GameState mutation disabled
-> M57-05 tooling/spec/tests are scaffolded; targeted M57-04/M57-05 tests
   18/18 and full Python tests 1164/1164 pass. It validates the repaired
   main-deck preview after the M57-04 boundary and checks combo consistency,
   while keeping runtime fixture promotion deferred to M57-06
-> M57-06 tooling/spec/tests are scaffolded; targeted M57-06 tests 9/9 and
   full Python tests 1173/1173 pass. It can create only an offline runtime/test
   fixture after M57-03 accepted rows and M57-05 validation exist; saved deck,
   UI publication, bot/playbook, G Zone/Stride runtime, and GameState mutation
   remain disabled
-> M57-closeout tooling/spec/tests are scaffolded; targeted M57-closeout tests
   6/6 and full Python tests 1179/1179 pass. It routes a completed sixth
   fixture to M58 consumption/scale work, but the real closeout output remains
   gated on M57-06 output
-> M58-01 tooling/spec/tests are scaffolded; targeted M58-01 tests 11/11 and
   full Python tests 1190/1190 pass using an in-memory M57-06 fixture. The real
   M58-01 report remains gated on the real M57-06 runtime fixture file
-> M58-02 tooling/spec/tests are scaffolded; targeted M58-02 tests 7/7 and full
   Python tests 1197/1197 pass using an in-memory M57-06 fixture plus M58-01
   validation report. The real M58-02 deck text/report artifacts remain gated
   on real M57-06 and M58-01 files
-> M58-03 tooling/spec/tests are scaffolded; targeted M58-03 tests 9/9 and full
   Python tests 1206/1206 pass using in-memory M57-06/M58-01/M58-02 artifacts.
   Offline deck-code smoke is ready in scaffold, while real outputs and Unity
   headless evidence remain gated on real upstream files
-> M58-04 tooling/spec/tests are scaffolded; targeted M58-04 tests 8/8 and full
   Python tests 1214/1214 pass using the first five real smoke reports plus
   in-memory sixth fixture smoke. Six-fixture scale decision is ready in
   scaffold, while real report remains gated on real M58-03 Unity evidence
-> M59-01 tooling/spec/tests are scaffolded; targeted M59-01 tests 7/7 and full
   Python tests 1221/1221 pass using an in-memory M58-04 scale decision.
   Seventh target selection is ready in scaffold, while real report remains
   gated on real M58-04 output
-> M59-02 tooling/spec/tests are scaffolded; targeted M59-02 tests 9/9 and full
   Python tests 1230/1230 pass using an in-memory M59-01 selection. The
   selected scaffold target is `เนโอ เนคต้า` / `g_series_first`: source cards
   78, grade profile 17/23/18/12/8, trigger capacity 48, no trigger gaps,
   ready_for_m59_03 true. Real report remains gated on real M59-01 output
-> M59-03 tooling/spec/tests are scaffolded; targeted M59-03 tests 8/8 and full
   Python tests 1238/1238 pass using in-memory M59-01/M59-02 reports. The
   seventh-slice semantic probe reports semantic cards 78, manual-review cards
   10, pair graph edges 2885, candidate edges 107, and ready_for_m59_04 true.
   Real report remains gated on real M59-01 and M59-02 outputs
-> M59-04 tooling/spec/tests are scaffolded; targeted M59-04 tests 9/9 and full
   Python tests 1247/1247 pass using in-memory M59-02/M59-03 reports. The gate
   allows M60 offline recipe pipeline only, with source cards 78, semantic
   cards 78, manual-review cards 10, pair graph edges 2885, candidate edges
   107, blocking issues 0, and ready_for_m60 true. Real report remains gated
   on real M59-02 and M59-03 outputs
-> M60-01 tooling/spec/tests are scaffolded; targeted M60-01 tests 9/9 and full
   Python tests 1256/1256 pass using in-memory M59-02/M59-03/M59-04 reports.
   The fixture scaffold is ready for M60-02 with source cards 78, grade profile
   17/23/18/12/8, trigger profile Critical 5 / Draw 2 / Heal 2 / Stand 3,
   candidate edges 107, manual-review cards 10, and runtime/UI/bot/G Zone/
   Stride/GameState still disabled. Real report remains gated on real upstream
   outputs
-> M60-02 tooling/spec/tests are scaffolded; targeted M60-02 tests 9/9 and full
   Python tests 1265/1265 pass using in-memory M59-01/M59-02/M59-03/M59-04/
   M60-01 reports. The review packet is ready for M60-03 with fixture scaffold
   items 1, manual-review cards 10, candidate edges 107, total review items
   118, and runtime/UI/bot/G Zone/Stride/GameState still disabled. Real packet
   remains gated on real upstream outputs
-> M60-03 tooling/spec/tests are scaffolded; targeted M60-03 tests 9/9 and full
   Python tests 1274/1274 pass using in-memory M59-01/M59-02/M59-03/M59-04/
   M60-01/M60-02 reports. The draft model creates 23 advisory quantity-complete
   recipe drafts, skips 84 trigger/Grade 4/missing candidate edges, carries 23
   manual-overlap recipe blockers, and keeps runtime/UI/bot/G Zone/Stride/
   GameState disabled. Real draft artifacts remain gated on real upstream
   outputs
-> M60-04 tooling/spec/tests are scaffolded; targeted M60-04 tests 7/7 and full
   Python tests 1281/1281 pass using in-memory M59-01/M59-02/M59-03/M59-04/
   M60-01/M60-02/M60-03 reports. The validator checks 23 drafts, reports 0
   runtime-ready recipes, 23 manual-review blockers, 0 missing/copy/slot/
   trigger/Grade 4 main-deck blockers, 21 grade-profile review items, and 23
   G Zone plus Bloom/token deferred review items. Real validation artifacts
   remain gated on real upstream outputs
-> M60-05 tooling/spec/tests are scaffolded; targeted M60-05 tests 6/6 and full
   Python tests 1287/1287 pass using in-memory M59-01/M59-02/M59-03/M59-04/
   M60-01/M60-02/M60-03/M60-04 reports. The consistency check covers 23
   recipes, confirms pair cards present for all 23, reports 0 missing pair-card
   checks, 23 recipe-level manual-review dependencies, 23 G Zone deferred
   checks, 23 Bloom/token deferred checks, and promotion_allowed 0. Real
   consistency artifacts remain gated on real upstream outputs
-> M60-06 tooling/spec/tests are scaffolded; targeted M60-06 tests 8/8 and full
   Python tests 1295/1295 pass using in-memory M59-01/M59-02/M59-03/M59-04/
   M60-01/M60-02/M60-03/M60-04/M60-05 reports. The repair candidate report
   covers 23 recipes, has 23 complete manual repair previews, 21 complete
   grade-profile repair candidates, 23 G Zone deferred packages, 23 Bloom/token
   deferred packages, 0 structural blockers, ready_for_human_repair_review 23,
   ready_for_m60_closeout true, and no runtime/UI/bot/GameState mutation. Real
   repair artifacts remain gated on real upstream outputs
-> M60-closeout tooling/spec/tests are scaffolded; targeted M60-closeout tests
   9/9 and full Python tests 1304/1304 pass using in-memory M59-01/M59-02/
   M59-03/M59-04/M60-01/M60-02/M60-03/M60-04/M60-05/M60-06 reports. The
   closeout reports m60_scaffold_complete true, real_artifacts_available false,
   runtime_ready_recipe_available false, human_selection_review_allowed true,
   next_queue M61, 23 manual-review overlap recipes, 23 G Zone deferred
   recipes, 23 Bloom/token deferred recipes, and no runtime/UI/bot/GameState
   mutation. Real closeout artifacts remain gated on real upstream outputs
-> M61-01 tooling/spec/tests are scaffolded; targeted M61-01 tests 10/10 and
   full Python tests 1314/1314 pass using in-memory M59-01/M59-02/M59-03/
   M59-04/M60-01/M60-02/M60-03/M60-04/M60-05/M60-06/M60-closeout reports.
   The packet exports 23 review items, 23 complete manual repair candidates,
   21 complete grade-profile candidates, 2 grade-profile-not-needed items,
   23 G Zone deferred items, 23 Bloom/token deferred items, ready_for_m61_02
   true, and no selection/acceptance/system decision/runtime/UI/bot/GameState
   mutation. Real M61-01 artifacts remain gated on real upstream outputs
-> M61-02 tooling/spec/tests are scaffolded; targeted M61-02 tests 10/10 and
   full Python tests 1324/1324 pass using an explicit in-memory M61-01
   review-item selection. The artifact
   records exactly one selected review item, preserves pair/manual/grade/G Zone/
   Bloom context, records no acceptance or system decision, keeps runtime/UI/
   bot/GameState mutation disabled, and reports ready_for_m61_03 true for valid
   selected items. Real M61-02 artifacts remain gated on the real M61-01 packet
   plus explicit human `review_item_id` and `selection_text`
-> M61-03 tooling/spec/tests are scaffolded; targeted M61-03 tests 8/8 and
   full Python tests 1332/1332 pass using an explicit in-memory M61-02
   selection and acceptance text. The artifact
   records human acceptance, applies selected manual repair in memory, recomputes
   grade repair after manual substitution, produces a 50-card repaired preview
   with target grade profile for valid selected items, keeps G Zone/Stride and
   Bloom/token decisions deferred to M61-04, records no validation/runtime
   promotion, and does not mutate runtime/UI/bot/GameState. Real M61-03
   artifacts remain gated on real M61-02 output plus explicit `acceptance_text`
-> M61-04 tooling/spec/tests are scaffolded; targeted M61-04 tests 10/10 and
   full Python tests 1342/1342 pass using an explicit in-memory M61-03 accepted
   artifact and explicit G Zone / Bloom-token options. The artifact records
   both boundary decisions, opens M61-05 only when both selected options allow
   main-deck/manual-semantic
   validation, keeps G Zone/Stride/Bloom-token runtime disabled, records no
   validation/runtime promotion, and does not mutate runtime/UI/bot/GameState.
   Real M61-04 artifacts remain gated on real M61-03 output plus explicit
   `g_zone_option` and `bloom_token_option`
-> M61-05 tooling/spec/tests are scaffolded; targeted M61-05 tests 10/10 and
   full Python tests 1352/1352 pass using in-memory M61-03/M61-04 artifacts.
   The rerun validates the repaired main-deck preview, suppresses G Zone and
   Bloom/token deferred review codes only for boundary decisions that
   explicitly allow main-deck validation,
   passes validation/consistency for valid accepted items, reports
   ready_for_m61_06 true, creates no runtime fixture, and does not mutate
   runtime/UI/bot/GameState. Real M61-05 artifacts remain gated on real M61-03
   and M61-04 outputs
-> M61-06 tooling/spec/tests are scaffolded; targeted M61-06 tests 10/10 and
   full Python tests 1362/1362 pass using in-memory M61-03/M61-05 artifacts.
   The promotion gate creates an offline runtime/test fixture artifact only
   when human acceptance, validation, consistency, G Zone boundary, and
   Bloom/token boundary all pass. It blocks deferred G Zone or Bloom/token
   choices, keeps G Zone/Stride/Bloom/token runtime disabled, creates no saved
   deck, publishes no UI deck, enables no bot/playbook, and does not mutate
   GameState. Real M61-06 artifacts remain gated on real M61-03 and M61-05
   outputs
-> M61-closeout tooling/spec/tests are scaffolded; targeted M61-closeout tests
   6/6 and full Python tests 1368/1368 pass. The closeout routes passing
   M61-06 evidence to M62 seventh fixture consumption only, routes failed gate
   evidence to M61-repair, keeps saved deck/UI/bot/GameState mutation disabled,
   and keeps G Zone/Stride/Bloom/token runtime disabled
-> do not promote playbook hints into runtime/bot until a later bot/playbook
   gate explicitly allows it
```

Project direction:

```text
Vanguard Area-style manual freedom
+ VangPro-style deck builder/import/share
+ Dear Days/Cardfight Online-style field, hand, zone, phase, and action layout
+ Thai card database/images
+ our own rules core, bot, replay, and online room foundation
```

Architecture direction:

- Correctness first, performance later.
- Manual play and audited logs first, full auto effect later.
- JSON/canonical text formats first for debugging.
- Packed/flat simulation state and binary codecs later only when needed.
- Advanced bot, ISMCTS, and RL wait until core correctness gates are stable.

## Current Status

Completed at a high level:

- Card browser, deck builder, deck validation, save/load, deck code import/export.
- Manual PlayTable with zones, action log, undo/replay, Gift markers.
- Baseline deterministic bots.
- RulesCore command/query facade, seeded RNG, snapshot/restore, AbilityCore foundation.
- Mobile readiness, custom pack importer.
- Multiplayer foundation, Photon transport, lobby/room, reconnect, online PlayTable action publishing.
- Hidden-state player/spectator/replay views.
- Deck privacy/commitment plan and conservative commitment-only gameplay block.
- Trigger probability, board/resource evaluator, battle search, guard estimator, playbook, combo discovery scaffold.
- M10 ability/trigger automation foundation is closed with trigger scaffolds,
  pending AUTO queues, real timing event integration, manual resolution
  metadata, and closeout regression.
- M11 RulesCore completion is closed with timing/window audit, phase/timing
  matrix, facade coverage, legal-action mask hardening, no-mutation rejects,
  event-sourcing coverage, replay determinism, snapshot/rollback, hidden-state
  hardening, resource ledger, RuleSet profiles, and closeout regression.
- M12 structured card ability data is closed with schema/validator/runtime
  registry, cost/target/effect/modifier templates, fixture DSL, first
  structured smoke pack, manual fallback bridge, and closeout verification.
- M13-01 owner-private room initialization is complete with local true state,
  opponent hidden placeholders from public count metadata, commitment mismatch
  no-mutation rejection, and normal commitment gameplay still blocked.
- RulesCore command facade coverage audit marks current GameActionType commands
  as covered and `UndoLast` as the explicit exception.
- Legal action mask usage report marks UI/bot/session/ability paths as hardened
  or explicit exceptions.
- Reject no-mutation guard snapshots and tests cover RulesCore rejects, pending
  AUTO queue commit rejects, and missing-payload session publish rejects.
- Event-sourcing coverage report marks current `GameActionType` mutations as
  `GameEvent`/reducer-backed and keeps `UndoLast` plus pending AUTO timing
  commit as explicit exceptions.
- Replay determinism verifier replays supported `GameEventReducer` events and
  verifies final state equality without mutating source states/events.
- Snapshot rollback verifier proves branch actions mutate cloned branch state
  only, while live state and restored state remain isolated.
- Hidden state view hardening verifier checks player/spectator masking for
  private zones, face-down public cards, private event ids, and source
  no-mutation.
- Resource ledger validates CounterBlast, SoulBlast, EnergyBlast, and once
  flags as a pure transaction preview before live cost payment exists.
- RuleSet profile catalog separates Standard, V-Premium, and Premium feature
  flags behind a central resolver instead of raw shared-format hard-coding.

Latest verification baseline:

- Python data/unit tests: `44/44` passed after runtime JSON catalog export and
  Android install-smoke `--push-pack` coverage.
- Unity compile log: `unity_compile_m19_09_icon_override_loader.log` has no C#
  compiler errors after semantic icon override loader work.
- Unity EditMode tests: `892/892` passed at
  `unity_editmode_m19_09_icon_override_loader.xml` after user icon pack tests were added.
- Android build log: `android_build_m19_09_icon_override_loader.log` rebuilt
  `build/android/latest/VanguardThaiSim.apk` with `errors=0`, `warnings=1`.
- Android install-smoke report:
  `android_install_smoke_m19_09_icon_override_loader.json` detects package
  `com.DefaultCompany.VanguardThaiSim`, selects LDPlayer ADB, installs the APK,
  pushes `data/packs/vanguard_th` into app external files, force-stops, and
  launches with `status=passed`.
- Android LDPlayer visual smoke: Home loads `Pack: Vanguard TH / 251`, `Cards
  10836` after M19-09 at
  `android_ldplayer_m19_09_icon_override_loader_home.png`; Card images are
  expected fallback on Android until the external 2.16 GiB image dump is
  provisioned.
- Client smoke: `ClientSmokeFlowRunner` passed after `M16-10`; latest rebuilt
  Windows player smoke passed at `player_smoke_m19_09_icon_override_loader.json`.
- Latest M21-02 Windows-first verification: Unity compile
  `unity_compile_m21_02_playtable_board_first_c.log` has no compiler-error
  markers, Unity EditMode
  `unity_editmode_m21_02_playtable_board_first_c.xml` passed `902/902`,
  Windows smoke build `windows_build_m21_02_playtable_board_first_b.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_02_playtable_board_first_b.json` passed with
  `blockers=[]`.
- Latest M21-03 Windows-first verification: Unity compile
  `unity_compile_m21_03_zone_status.log` has no compiler-error markers, Unity
  EditMode `unity_editmode_m21_03_zone_status.xml` passed `904/904`, Windows
  smoke build `windows_build_m21_03_zone_status.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke `player_smoke_m21_03_zone_status.json`
  passed with `blockers=[]`.
- Latest M21-04 Windows-first verification: Unity compile
  `unity_compile_m21_04_hand_preview.log` has no compiler-error markers, Unity
  EditMode `unity_editmode_m21_04_hand_preview.xml` passed `917/917`, Windows
  smoke build `windows_build_m21_04_hand_preview.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke `player_smoke_m21_04_hand_preview.json`
  passed with `blockers=[]`.
- Latest M21-04b Windows-first verification: Unity compile
  `unity_compile_m21_04b_soul_status_ledger.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_04b_soul_status_ledger.xml` passed
  `920/920`, Windows smoke build
  `windows_build_m21_04b_soul_status_ledger.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_04b_soul_status_ledger.json` passed with `blockers=[]`.
- Latest M21-04d Windows-first verification: Unity compile
  `unity_compile_m21_04d_ride_soul_resource.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_04d_ride_soul_resource.xml` passed
  `926/926`, Windows smoke build
  `windows_build_m21_04d_ride_soul_resource.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_04d_ride_soul_resource.json` passed with `blockers=[]`.
- Latest M21-04c Windows-first verification: Unity compile
  `unity_compile_m21_04c_board_thumbnail.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_04c_board_thumbnail.xml` passed `929/929`,
  Windows smoke build `windows_build_m21_04c_board_thumbnail.log` succeeded
  with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_04c_board_thumbnail.json` passed with `blockers=[]`.
- Latest M21-05a Windows-first verification: Unity compile
  `unity_compile_m21_05_action_availability.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05_action_availability.xml`
  passed `934/934`, Windows smoke build
  `windows_build_m21_05_action_availability.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05_action_availability.json` passed with `blockers=[]`.
- Latest M21-05a2 Windows-first verification: Unity compile
  `unity_compile_m21_05b_check_guard_surface.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05b_check_guard_surface.xml`
  passed `937/937`, Windows smoke build
  `windows_build_m21_05b_check_guard_surface.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b_check_guard_surface.json` passed with `blockers=[]`.
- Latest M21-05a3 Windows-first verification: Unity compile
  `unity_compile_m21_05c_trigger_source_split.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05c_trigger_source_split.xml`
  passed `941/941`, Windows smoke build
  `windows_build_m21_05c_trigger_source_split.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05c_trigger_source_split.json` passed with `blockers=[]`.
- Latest M21-05a4 Windows-first verification: Unity compile
  `unity_compile_m21_05d_attack_vanguard_surface.log` has no compiler-error
  markers, Unity EditMode
  `unity_editmode_m21_05d_attack_vanguard_surface.xml` passed `945/945`,
  Windows smoke build `windows_build_m21_05d_attack_vanguard_surface.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_05d_attack_vanguard_surface.json` passed with
  `blockers=[]`.
- Latest M21-05a5 Windows-first verification: Unity compile
  `unity_compile_m21_05e_attack_target_selection.log` has no compiler-error
  markers, Unity EditMode
  `unity_editmode_m21_05e_attack_target_selection.xml` passed `949/949`,
  Windows smoke build `windows_build_m21_05e_attack_target_selection.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_05e_attack_target_selection.json` passed with
  `blockers=[]`.
- Latest M21-05a6 Windows-first verification: Unity compile
  `unity_compile_m21_05f_battle_flow_status.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05f_battle_flow_status.xml`
  passed `955/955`, Windows smoke build
  `windows_build_m21_05f_battle_flow_status.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05f_battle_flow_status.json` passed with `blockers=[]`.
- Latest M21-05a7 Windows-first verification: Unity compile
  `unity_compile_m21_05g_manual_note_surface.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05g_manual_note_surface.xml`
  passed `960/960`, Windows smoke build
  `windows_build_m21_05g_manual_note_surface.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05g_manual_note_surface.json` passed with `blockers=[]`.
- Latest M21-05b1 Windows-first verification: Unity compile
  `unity_compile_m21_05b1_setup_readiness.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_05b1_setup_readiness.xml` passed
  `965/965`, Windows smoke build
  `windows_build_m21_05b1_setup_readiness.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b1_setup_readiness.json` passed with `blockers=[]`.
- Latest M21-05b2 Windows-first verification: Unity compile
  `unity_compile_m21_05b2_first_vanguard_setup.log` has no compiler-error
  markers, Unity EditMode
  `unity_editmode_m21_05b2_first_vanguard_setup.xml` passed `969/969`,
  Windows smoke build `windows_build_m21_05b2_first_vanguard_setup.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b2_first_vanguard_setup.json` passed with `blockers=[]`.
- Latest M21-05b3 Windows-first verification: Unity compile
  `unity_compile_m21_05b3_mulligan_selected_r2.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_05b3_mulligan_selected_r2.xml`
  passed `972/972`, Windows smoke build
  `windows_build_m21_05b3_mulligan_selected.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b3_mulligan_selected.json` passed with `blockers=[]`.
- Latest M21-05b4 Windows-first verification: Unity compile
  `unity_compile_m21_05b4_phase_actions.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_05b4_phase_actions.xml` passed
  `973/973`, Windows smoke build `windows_build_m21_05b4_phase_actions.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b4_phase_actions.json` passed with `blockers=[]`.
- Latest M21-05b5 Windows-first verification: Unity compile
  `unity_compile_m21_05b5_setup_guidance.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_05b5_setup_guidance.xml` passed
  `981/981`, Windows smoke build `windows_build_m21_05b5_setup_guidance.log`
  succeeded with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_05b5_setup_guidance.json` passed with `blockers=[]`.
- Latest M21-06 Windows-first verification: Unity compile
  `unity_compile_m21_06_advanced_debug_cleanup.log` has no compiler-error
  markers, Unity EditMode `unity_editmode_m21_06_advanced_debug_cleanup_r2.xml`
  passed `981/981`, Windows smoke build
  `windows_build_m21_06_advanced_debug_cleanup.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m21_06_advanced_debug_cleanup.json` passed with `blockers=[]`.
- Latest M21-07 Windows-first verification: Unity compile
  `unity_compile_m21_07_player_event_log.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m21_07_player_event_log.xml` passed `982/982`,
  Windows smoke build `windows_build_m21_07_player_event_log.log` succeeded
  with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m21_07_player_event_log.json` passed with `blockers=[]`.
- Latest M22-01 Windows-first verification: Unity compile
  `unity_compile_m22_01_player_settings.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_01_player_settings.xml` passed `988/988`,
  Windows smoke build `windows_build_m22_01_player_settings.log` succeeded
  with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m22_01_player_settings.json` passed with `blockers=[]`.
- Latest M22-02 Windows-first verification: Unity compile
  `unity_compile_m22_02_deck_appearance.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_02_deck_appearance.xml` passed `997/997`,
  Windows smoke build `windows_build_m22_02_deck_appearance.log` succeeded
  with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m22_02_deck_appearance.json` passed with `blockers=[]`.
- Latest M22-03 Windows-first verification: Unity compile
  `unity_compile_m22_03_home_settings.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_03_home_settings.xml` passed `1002/1002`,
  Windows smoke build `windows_build_m22_03_home_settings.log` succeeded
  with `errors=0`, `warnings=0`, and Windows player smoke
  `player_smoke_m22_03_home_settings.json` passed with `blockers=[]`.
- Latest M22-04 Windows-first verification: Unity compile
  `unity_compile_m22_04_deck_accessories.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_04_deck_accessories.xml` passed
  `1006/1006`, Windows smoke build
  `windows_build_m22_04_deck_accessories.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m22_04_deck_accessories.json` passed with `blockers=[]`.
- Latest M22-05 Windows-first verification: Unity compile
  `unity_compile_m22_05_cosmetic_legality.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_05_cosmetic_legality.xml` passed
  `1008/1008`, Windows smoke build
  `windows_build_m22_05_cosmetic_legality.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m22_05_cosmetic_legality.json` passed with `blockers=[]`.
- Latest M22-06 Windows-first verification: Unity compile
  `unity_compile_m22_06_user_deck_assets.log` has no compiler-error markers,
  Unity EditMode `unity_editmode_m22_06_user_deck_assets.xml` passed
  `1014/1014`, Windows smoke build
  `windows_build_m22_06_user_deck_assets.log` succeeded with `errors=0`,
  `warnings=0`, and Windows player smoke
  `player_smoke_m22_06_user_deck_assets.json` passed with `blockers=[]`.
- Headless CLI: `HeadlessSimulationCliRunner` passed after `M17-02`
- Latest live Photon game-session smoke after `M13-11` passed

Current next target:

```text
Next
1. M20 Windows Product Reset is closed. Use
   `docs/WINDOWS_FIRST_PROGRAM_COMPLETION_SPEC.md`,
   `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`, and
   `docs/WINDOWS_ONLY_VERIFICATION_PROFILE.md` as the active product and
   verification gates.
2. `M21-01` PlayTable v2 spec is complete at
   `docs/PLAYTABLE_WINDOWS_UX_V2_SPEC.md`. `M21-02` board/table prominence is
   complete at `docs/history/M21_02_PLAYTABLE_BOARD_FIRST_CLOSEOUT.md`.
   `M21-03` zone status is complete at
   `docs/history/M21_03_PLAYTABLE_ZONE_STATUS_CLOSEOUT.md`. `M21-04` hand strip
   and selected-card preview is complete at
   `docs/history/M21_04_HAND_STRIP_SELECTED_PREVIEW_CLOSEOUT.md`.
   `M21-04b` Soul status and ResourceLedger wiring is complete at
   `docs/history/M21_04B_SOUL_STATUS_LEDGER_CLOSEOUT.md`.
   `M21-04d` ride-to-soul and Soul resource commands are complete at
   `docs/history/M21_04D_RIDE_SOUL_RESOURCE_COMMAND_CLOSEOUT.md`.
   `M21-04c` board thumbnail/card-face summary polish is complete at
   `docs/history/M21_04C_BOARD_THUMBNAIL_CLOSEOUT.md`.
   `M21-05a` action availability and phase buttons are complete at
   `docs/history/M21_05A_ACTION_AVAILABILITY_CLOSEOUT.md`.
   `M21-05a2` Check/Guard surface is complete at
   `docs/history/M21_05A2_CHECK_GUARD_SURFACE_CLOSEOUT.md`.
   `M21-05a3` Drive/Damage trigger source split is complete at
   `docs/history/M21_05A3_TRIGGER_SOURCE_SPLIT_CLOSEOUT.md`.
   `M21-05a4` attack-vanguard shortcut surface is complete at
   `docs/history/M21_05A4_ATTACK_VANGUARD_SURFACE_CLOSEOUT.md`.
   `M21-05a5` attack target selection is complete at
   `docs/history/M21_05A5_ATTACK_TARGET_SELECTION_CLOSEOUT.md`.
   `M21-05a6` Battle Flow status guidance is complete at
   `docs/history/M21_05A6_BATTLE_FLOW_STATUS_CLOSEOUT.md`.
   `M21-05a7` Manual Note surface is complete at
   `docs/history/M21_05A7_MANUAL_NOTE_SURFACE_CLOSEOUT.md`.
   `M21-05b1` setup readiness guard is complete at
   `docs/history/M21_05B1_SETUP_READINESS_GUARD_CLOSEOUT.md`.
   `M21-05b2` first Vanguard setup is complete at
   `docs/history/M21_05B2_FIRST_VANGUARD_SETUP_CLOSEOUT.md`.
   `M21-05b3` selected-card mulligan is complete at
   `docs/history/M21_05B3_MULLIGAN_SELECTED_CLOSEOUT.md`.
   `M21-05b4` phase actions are complete at
   `docs/history/M21_05B4_PHASE_ACTIONS_CLOSEOUT.md`.
   `M21-05b5` setup status guidance is complete at
   `docs/history/M21_05B5_SETUP_STATUS_GUIDANCE_CLOSEOUT.md`.
   `M21-06` Advanced-only debug surface cleanup is complete at
   `docs/history/M21_06_ADVANCED_DEBUG_SURFACE_CLOSEOUT.md`.
   `M21-07` player-readable event log is complete at
   `docs/history/M21_07_PLAYER_READABLE_EVENT_LOG_CLOSEOUT.md`.
   `M21-08` PlayTable tests and Windows smoke roll-up is complete at
   `docs/history/M21_08_TESTS_WINDOWS_SMOKE_CLOSEOUT.md`.
   `M21-09` PlayTable Windows UX closeout is complete at
   `docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md`.
   `M22-01` PlayerSettings is complete at
   `docs/history/M22_01_PLAYER_SETTINGS_CLOSEOUT.md`.
   `M22-02` DeckAppearanceMetadata is complete at
   `docs/history/M22_02_DECK_APPEARANCE_METADATA_CLOSEOUT.md`.
   `M22-03` Home Settings screen is complete at
   `docs/history/M22_03_HOME_SETTINGS_SCREEN_CLOSEOUT.md`.
   `M22-04` Deck Type / Accessories dialog is complete at
   `docs/history/M22_04_DECK_TYPE_ACCESSORIES_DIALOG_CLOSEOUT.md`.
   `M22-05` cosmetic metadata / deck legality separation is complete at
   `docs/history/M22_05_COSMETIC_LEGALITY_SEPARATION_CLOSEOUT.md`.
   `M22-06` user deck asset slots is complete at
   `docs/history/M22_06_USER_DECK_ASSET_SLOT_CLOSEOUT.md`.
   `M22-07` tests roll-up is complete at
   `docs/history/M22_07_SETTINGS_ACCESSORIES_TEST_ROLLUP_CLOSEOUT.md`.
   `M22` Windows Settings / Deck Type / Accessories is closed at
   `docs/history/M22_WINDOWS_SETTINGS_ACCESSORIES_CLOSEOUT.md`.
   `M23-01` Manual content spec is complete at
   `docs/history/M23_01_MANUAL_CONTENT_SPEC_CLOSEOUT.md`.
   `M23-02` Manual screen is complete at
   `docs/history/M23_02_MANUAL_SCREEN_CLOSEOUT.md`.
   `M23-03` Loading tips is complete at
   `docs/history/M23_03_LOADING_TIPS_CLOSEOUT.md`.
   `M23-04` Original content gate is complete at
   `docs/history/M23_04_ORIGINAL_CONTENT_GATE_CLOSEOUT.md`.
   `M23-05` Manual filter is complete at
   `docs/history/M23_05_MANUAL_FILTER_CLOSEOUT.md`.
   `M23-06` Manual tests are complete at
   `docs/history/M23_06_MANUAL_TESTS_CLOSEOUT.md`.
   `M23` Manual/Tutorial is closed at
   `docs/history/M23_MANUAL_TUTORIAL_CLOSEOUT.md`.
   `M24-01` Deck Builder Windows landscape is complete at
   `docs/history/M24_01_DECK_BUILDER_WINDOWS_LANDSCAPE_CLOSEOUT.md`.
   `M24-02` Count-line deck text is complete at
   `docs/history/M24_02_COUNT_LINE_DECK_TEXT_CLOSEOUT.md`.
   `M24-03` Deck import compatibility UI is complete at
   `docs/history/M24_03_DECK_IMPORT_MISMATCH_UI_CLOSEOUT.md`.
   `M24-04` CGS-like custom pack adapter spec is complete at
   `docs/history/M24_04_CGS_LIKE_CUSTOM_PACK_ADAPTER_SPEC_CLOSEOUT.md`.
   `M24-05` VangPro-like custom import spec is complete at
   `docs/history/M24_05_VANGPRO_LIKE_CUSTOM_IMPORT_SPEC_CLOSEOUT.md`.
   `M24-06` local custom import validator is complete at
   `docs/history/M24_06_LOCAL_CUSTOM_IMPORT_VALIDATOR_CLOSEOUT.md`.
   `M24-07` Pack Validation UI is complete at
   `docs/history/M24_07_PACK_VALIDATION_UI_CLOSEOUT.md`.
   `M24-08` Deck image export is complete at
   `docs/history/M24_08_DECK_IMAGE_EXPORT_CLOSEOUT.md`.
   `M24-09` import/custom pack workflow test rollup is complete at
   `docs/history/M24_09_CUSTOM_IMPORT_WORKFLOW_TEST_ROLLUP_CLOSEOUT.md`.
   `M24` Deck Builder / Import / Custom Pack UX is closed at
   `docs/history/M24_DECK_BUILDER_IMPORT_CUSTOM_PACK_UX_CLOSEOUT.md`.
   `M25-01` Photon trusted-client room policy is complete at
   `docs/history/M25_01_PHOTON_TRUSTED_CLIENT_ROOM_POLICY_CLOSEOUT.md`.
   `M25-02` lobby flow is complete at
   `docs/history/M25_02_LOBBY_FLOW_CLOSEOUT.md`.
   `M25-03` room status is complete at
   `docs/history/M25_03_ROOM_STATUS_CLOSEOUT.md`.
   `M25-04` reconnect UX is complete at
   `docs/history/M25_04_RECONNECT_UX_CLOSEOUT.md`.
   `M25-05` online PlayTable default UI is complete at
   `docs/history/M25_05_ONLINE_PLAYTABLE_DEFAULT_UI_CLOSEOUT.md`.
   `M25-06` replay sync/status is complete at
   `docs/history/M25_06_REPLAY_SYNC_STATUS_CLOSEOUT.md`.
   `M25-07` online room test rollup is complete at
   `docs/history/M25_07_ONLINE_ROOM_TEST_ROLLUP_CLOSEOUT.md`.
   `M25` Windows Online Room usability is closed at
   `docs/history/M25_WINDOWS_ONLINE_ROOM_USABILITY_CLOSEOUT.md`.
   `M26-01` bot/automation return audit is complete at
   `docs/history/M26_01_BOT_AUTOMATION_RETURN_AUDIT_CLOSEOUT.md`.
   `M26-02` bot legal-action/masked-state gate is complete at
   `docs/history/M26_02_BOT_LEGAL_ACTION_MASKED_STATE_GATE_CLOSEOUT.md`.
   `M26-03` bot explanation panel is complete at
   `docs/history/M26_03_BOT_EXPLANATION_PANEL_CLOSEOUT.md`.
   `M26-04` structured ability template gate is complete at
   `docs/history/M26_04_STRUCTURED_ABILITY_TEMPLATE_GATE_CLOSEOUT.md`.
   `M26-05` live effect no text parsing gate is complete at
   `docs/history/M26_05_LIVE_EFFECT_NO_TEXT_PARSING_GATE_CLOSEOUT.md`.
   `M27-03` Windows performance baseline is complete at
   `docs/history/M27_03_WINDOWS_PERFORMANCE_BASELINE_CLOSEOUT.md`.
   `M27-04` memory / performance gate is complete at
   `docs/history/M27_04_WINDOWS_PERFORMANCE_GATE_CLOSEOUT.md`.
   `M27-05` graceful error handling is complete at
   `docs/history/M27_05_WINDOWS_GRACEFUL_ERROR_HANDLING_CLOSEOUT.md`.
   `M27-06` integration / PlayMode test is complete at
   `docs/history/M27_06_WINDOWS_PLAYMODE_INTEGRATION_CLOSEOUT.md`.
   `M27-07` known limitations list is complete at
   `docs/history/M27_07_WINDOWS_KNOWN_LIMITATIONS_CLOSEOUT.md`.
   `M27-08` no-public-release gate is complete at
   `docs/history/M27_08_NO_PUBLIC_RELEASE_GATE_CLOSEOUT.md`.
   `M28-01` Windows gameplay completion gate is complete at
   `docs/history/M28_01_WINDOWS_GAMEPLAY_COMPLETION_GATE_CLOSEOUT.md`.
   `M28-02` local PlayTable seat toggle is complete at
   `docs/history/M28_02_LOCAL_PLAYTABLE_SEAT_TOGGLE_CLOSEOUT.md`.
   `M28-03` UI-level two-seat match smoke is complete at
   `docs/history/M28_03_UI_TWO_SEAT_MATCH_SMOKE_CLOSEOUT.md`.
   `M28-04` Windows manual match gap audit is complete at
   `docs/history/M28_04_WINDOWS_MANUAL_MATCH_GAP_AUDIT.md`.
   `M28-05` PlayTable guided next-action panel is complete at
   `docs/history/M28_05_PLAYTABLE_GUIDED_NEXT_ACTION_CLOSEOUT.md`.
   `M28-06` Windows built-player smoke is complete at
   `docs/history/M28_06_WINDOWS_BUILT_PLAYER_SMOKE_CLOSEOUT.md`.
   `M28-07` PlayTable action grouping polish is complete at
   `docs/history/M28_07_PLAYTABLE_ACTION_GROUPING_POLISH_CLOSEOUT.md`.
   `M28-08` PlayTable side-panel density audit is complete at
   `docs/history/M28_08_PLAYTABLE_SIDE_PANEL_DENSITY_AUDIT.md`.
   `M28-09` Bot Plan Advanced drawer cleanup is complete at
   `docs/history/M28_09_BOT_PLAN_ADVANCED_DRAWER_CLOSEOUT.md`.
   `M29-01` Photon lobby navigation lockout is complete at
   `docs/history/M29_01_PHOTON_LOBBY_NAVIGATION_LOCKOUT_CLOSEOUT.md` with
   Unity compile, EditMode `1140/1140`, client smoke `blockers=[]`, Windows
   build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M29-02` Photon lobby reconnect flow polish is complete at
   `docs/history/M29_02_PHOTON_LOBBY_RECONNECT_FLOW_POLISH_CLOSEOUT.md` with
   Unity compile, EditMode `1142/1142`, client smoke `blockers=[]`, Windows
   build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M29-03` Photon lobby Quick Deck Selector is complete at
   `docs/history/M29_03_PHOTON_LOBBY_QUICK_DECK_SELECTOR_CLOSEOUT.md` with
   Unity compile, EditMode `1144/1144`, client smoke `blockers=[]`, Windows
   build `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M29-04` Photon lobby Quick Edit modal is complete at
   `docs/history/M29_04_PHOTON_LOBBY_QUICK_EDIT_MODAL_CLOSEOUT.md` with Unity
   compile, EditMode `1146/1146`, client smoke `blockers=[]`, Windows build
   `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M29-05` Online Room usability closeout audit is complete at
   `docs/history/M29_05_ONLINE_ROOM_USABILITY_CLOSEOUT_AUDIT.md`; it identified
   the next gap as local online deck readiness before room actions.
   `M29-06` Online deck readiness guard is complete at
   `docs/history/M29_06_ONLINE_DECK_READINESS_GUARD_CLOSEOUT.md` with Unity
   compile, EditMode `1149/1149`, client smoke `blockers=[]`, Windows build
   `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M28-10` Match Log / Preview density review is complete at
   `docs/history/M28_10_MATCH_LOG_PREVIEW_DENSITY_CLOSEOUT.md` with Unity
   compile, EditMode `1151/1151`, client smoke `blockers=[]`, Windows build
   `errors=0 warnings=0`, and Windows player smoke `blockers=[]`.
   `M30-01` Windows playable loop final audit is complete at
   `docs/history/M30_01_WINDOWS_PLAYABLE_LOOP_FINAL_AUDIT.md`; it found the
   Replay Home route was still locked. `M30-02` Windows Replay entry/browser is
   complete at `docs/history/M30_02_WINDOWS_REPLAY_ENTRY_BROWSER_CLOSEOUT.md`.
   `M30-03` Windows Replay local file import is complete at
   `docs/history/M30_03_WINDOWS_REPLAY_LOCAL_FILE_IMPORT_CLOSEOUT.md`.
   `M30-04` Windows Replay viewer launch is complete at
   `docs/history/M30_04_WINDOWS_REPLAY_VIEWER_LAUNCH_CLOSEOUT.md`.
   `M30-05` Windows PlayTable replay export is complete at
   `docs/history/M30_05_WINDOWS_PLAYTABLE_REPLAY_EXPORT_CLOSEOUT.md`.
   `M30-06` Windows playable loop closeout audit is complete at
   `docs/history/M30_06_WINDOWS_PLAYABLE_LOOP_CLOSEOUT_AUDIT.md`.
   `M31-01` Windows UI evidence audit is complete at
   `docs/history/M31_01_WINDOWS_UI_EVIDENCE_AUDIT.md`.
   `M31-02` Card Workshop first-screen clarity pass is complete at
   `docs/history/M31_02_CARD_WORKSHOP_FIRST_SCREEN_CLARITY_CLOSEOUT.md`.
   `M31-03` Card Workshop toolbar density pass is complete at
   `docs/history/M31_03_CARD_WORKSHOP_TOOLBAR_DENSITY_CLOSEOUT.md`.
   `M31-04` Windows UI visual evidence pass is complete at
   `docs/history/M31_04_WINDOWS_UI_VISUAL_EVIDENCE_PASS_CLOSEOUT.md`.
   Current implementation target: `M31-05` Card detail preview aspect-ratio
   fix.
3. Product decision 2026-06-27: keep Card Browser/Deck Builder taxonomy on
   `VanguardAreaClanTaxonomy`. Clan/group division follows the Vanguard
   Area-style model for now unless a later explicit UI milestone replaces it.
4. Use `outputs/comparator_study/COMPARATOR_SYSTEM_UX_STUDY_TH.md` as the
   latest comparator-study reference for VangPro, CGS, Vanguard Area,
   Dear Days, Card Game Simulator, and Cardfight Connect.
5. Do not run Android, LDPlayer, APK, mobile QA, app packaging, release
   candidate, or public distribution work unless the user explicitly reopens
   that track.
```

M10-74 completed:

- Add a pure model for manual handling of a pending auto ability request.
- Support `Resolve`, `Skip`, and `Defer` decisions.
- Keep it separate from actual card effect execution.
- Do not mutate `GameState`.

M10-75 completed:

- Encode manual resolution decisions into a network-ready payload.
- Decode payloads safely with protocol validation.
- Preserve hidden-source safety and no source mutation.

M10-76 completed:

- Reserve a Photon event code for manual resolution decision payloads.
- Add encode/decode wrapper methods around the network payload.
- Keep this separate from transport dispatch.

M10-77 completed:

- Add transport send/receive hooks for manual resolution decision payloads.
- Add Photon adapter decode/dispatch path for event code `11`.
- Keep this separate from session storage and ability resolution.

Goal of M10-78:

- Store received manual resolution decision payloads in the game-session controller.
- Keep storage outside `GameState`.
- Do not resolve, skip, or defer abilities yet.

M10-78 completed:

- Store received manual resolution decision payloads in the game-session controller.
- Keep storage outside `GameState` and `GameState.event_log`.
- Preserve normal game-event sync after receipt.

Goal of M10-79:

- Publish the latest stored manual resolution decision payload through the session controller.
- Reject publish when no decision payload exists.
- Preserve stored payloads and avoid `GameState` mutation.

M10-79 completed:

- Publish latest stored manual resolution decision payload through session controller.
- Reject missing-payload publish.
- Preserve stored payload history and `GameState`.

Goal of M10-80:

- Add online PlayTable control for publishing latest stored manual resolution decision payload.
- Keep local mode without this control.
- Preserve no-payload disabled state and no `GameState` mutation.

M10-80 completed:

- Add online PlayTable `DecAuto` publish control.
- Keep local PlayTable without the control.
- Publish latest stored manual resolution decision payload without `GameState` mutation.

Goal of M10-81:

- Add pure formatter for latest manual resolution decision payload summary.
- Preserve hidden-source safety and invalid payload fallback.
- Do not add new PlayTable surface yet.

M10-81 completed:

- Add pure latest manual resolution decision summary formatter.
- Preserve hidden-source safety.
- Keep formatter separate from PlayTable surface wiring.

Goal of M10-82:

- Add read-only PlayTable summary label for latest manual resolution decision.
- Show local zero-summary and online received/published summary.
- Preserve no `GameState` mutation.

M10-82 completed:

- Add read-only PlayTable summary label for latest manual resolution decision.
- Preserve local zero-summary and hidden-source safety.
- Keep rendering non-mutating.

Goal of M10-83:

- Add pure newest-first list formatter for manual resolution decision payloads.
- Bound output length.
- Preserve invalid-payload and hidden-source safety.

M10-83 completed:

- Add pure newest-first bounded list formatter for manual resolution decision payloads.
- Preserve invalid-payload and hidden-source safety.
- Keep formatter separate from PlayTable surface wiring.

Goal of M10-84:

- Add read-only PlayTable list label for manual resolution decision payloads.
- Show local zero list and online newest-first list.
- Preserve hidden-source safety and no `GameState` mutation.

M10-84 completed:

- Add read-only PlayTable list label for manual resolution decision payloads.
- Preserve local zero list, online newest-first list, and hidden-source safety.
- Keep rendering non-mutating.

Goal of M10-85:

- Add pure publish result formatter for manual resolution decision publish action.
- Preserve current success/failure text behavior where practical.
- Wire PlayTable `DecAuto` status message to the formatter.

M10-85 completed:

- Add pure publish result formatter for manual resolution decision publish action.
- Wire PlayTable `DecAuto` status message to the formatter.
- Keep transport/session behavior unchanged.

Goal of M10-86:

- Add pure factory for creating manual resolution decision draft payloads from a selected resolution request and decision type.
- Do not auto-resolve abilities.
- Preserve hidden-source safety and no `GameState` mutation.

M10-86 completed:

- Add pure factory for creating manual resolution decision draft payloads from a selected resolution request and decision type.
- Support `Resolve`, `Skip`, and `Defer`.
- Preserve hidden-source safety and source request immutability.

Goal of M10-87:

- Add online PlayTable control path that creates a draft manual resolution decision payload from the latest selected resolution request.
- Keep it separate from publishing/applying the decision.
- Preserve hidden-source safety and no `GameState` mutation.

M10-87 completed:

- Add online PlayTable `DraftDec` control.
- Create and store a `Resolve` manual resolution decision draft from the latest selected resolution request.
- Keep draft creation separate from network publish and ability application.

Goal of M10-88:

- Add a small selector for manual resolution decision type.
- Cycle between `Resolve`, `Skip`, and `Defer`.
- Use the selected type when creating `DraftDec` payloads.

M10-88 completed:

- Add online PlayTable `DecType` selector.
- Cycle `Resolve` -> `Skip` -> `Defer`.
- Make `DraftDec` use the selected decision type.

Goal of M10-89:

- Add pure formatter for manual resolution decision draft creation results.
- Replace inline `DraftDec` status text with the formatter.
- Keep session, transport, and `GameState` behavior unchanged.

M10-89 completed:

- Add pure formatter for manual resolution decision draft creation results.
- Wire PlayTable `DraftDec` status to the formatter.
- Preserve session, transport, and `GameState` behavior.

Goal of M10-90:

- Add a pure validator for manual resolution decision payloads before any future apply step.
- Check payload/decode validity and supported decision type.
- Keep validation non-mutating and separate from ability resolution.

M10-90 completed:

- Add pure validator for manual resolution decision payloads.
- Reject invalid payloads, unsupported decision types, and missing pending ids.
- Return sanitized hidden-source-safe decisions without mutating payloads.

Goal of M10-91:

- Add pure formatter for manual resolution decision validation results.
- Keep it separate from apply behavior.
- Use it later for PlayTable/session validation previews.

M10-91 completed:

- Add pure formatter for manual resolution decision validation results.
- Format accepted, rejected, null, and hidden-source cases.
- Keep it separate from PlayTable wiring and decision application.

Goal of M10-92:

- Add read-only PlayTable validation preview for the latest manual resolution decision payload.
- Use validator and validation result formatter.
- Keep preview non-mutating and separate from decision application.

M10-92 completed:

- Add read-only PlayTable validation preview for the latest manual resolution decision payload.
- Use validator and validation result formatter.
- Preserve hidden-source safety and no `GameState` mutation.

Goal of M10-93:

- Add pure command/result contract for applying a manual resolution decision.
- Define accepted inputs and rejection reasons before mutating queue state.
- Keep actual queue mutation for the next milestone.

M10-93 completed:

- Add pure command/result contract for applying a manual resolution decision.
- Define stable rejection reasons.
- Keep actual queue mutation and ability resolution out of scope.

Goal of M10-94:

- Add pure validator for the apply command against a pending queue and validated decision.
- Reject missing queue, missing decision, pending id mismatch, and unsupported decision type.
- Keep queue mutation for a later milestone.

M10-94 completed:

- Add pure validator for apply command inputs.
- Reject missing queue, missing decision, pending id mismatch, and unsupported decision type.
- Keep queue mutation and ability resolution out of scope.

Goal of M10-95:

- Add pure formatter for manual resolution apply result.
- Format accepted, rejected, and null results deterministically.
- Keep formatter separate from queue mutation and PlayTable wiring.

M10-95 completed:

- Add pure formatter for manual resolution apply result.
- Format accepted, rejected, and null results deterministically.
- Keep formatter separate from queue mutation and PlayTable wiring.

Goal of M10-96:

- Add pure executor that applies validated `Skip` and `Defer` decisions to a cloned pending queue.
- Leave `Resolve` as accepted/manual-not-executed until structured ability execution is wired.
- Do not mutate source queue, source decision, or `GameState`.

M10-96 completed:

- Add pure executor that returns a cloned pending queue plus apply result.
- Support `Skip`, `Defer`, and accepted/manual `Resolve`.
- Preserve source queue, source decision, and `GameState`.

Goal of M10-97:

- Add session helper that applies the latest stored manual decision to the latest stored pending queue payload.
- Store the returned queue payload in session history only.
- Do not publish, resolve card effects, or mutate `GameState`.

M10-97 completed:

- Add session helper that previews applying latest manual decision to latest pending queue payload.
- Store accepted returned queue payload in session history only.
- Preserve no publish, no card effect execution, and no `GameState` mutation.

Goal of M10-98:

- Add online PlayTable control for session apply preview.
- Use apply result formatter for status text.
- Keep local mode without the control and avoid network publish.

M10-98 completed:

- Add online PlayTable `ApplyDec` control.
- Use apply result formatter for status text.
- Keep local mode without the control and avoid network publish / `GameState` mutation.

Goal of M10-99:

- Add pure log entry model for manual resolution apply preview results.
- Keep it separate from `GameState.event_log` until the next integration gate.
- Preserve hidden-source and no-publish boundaries.

M10-99 completed:

- Add pure apply preview log entry model.
- Cover accepted/rejected JSON round-trip, hidden-source-safe fields, and source
  result no-mutation.
- Keep session, transport, UI, and `GameState` untouched.

Goal of M10-100:

- Add pure formatter for manual resolution apply preview log entries.
- Format accepted, rejected, null, and newest-first bounded list output.
- Keep free-text summary hidden from display to avoid future source-context
  leaks.

M10-100 completed:

- Add pure apply preview log formatter.
- Cover accepted/rejected/null/list formatting, hidden summary safety, and entry
  no-mutation.
- Keep session, transport, PlayTable, and `GameState` untouched.

Goal of M10-101:

- Store manual resolution apply preview log entries in session history.
- Append logs for accepted and rejected preview attempts.
- Keep storage outside `GameState` and avoid network publish.

M10-101 completed:

- Store manual resolution apply preview logs in game-session history.
- Append accepted and rejected apply preview results.
- Keep storage outside `GameState`, `GameState.event_log`, and transport.

Goal of M10-102:

- Add read-only PlayTable surface for manual resolution apply preview logs.
- Show latest and bounded list output with the M10-100 formatter.
- Keep rendering non-mutating and local mode zero-state safe.

M10-102 completed:

- Added read-only latest/list PlayTable labels for manual resolution apply
  preview logs.
- Kept local zero-state, online accepted/rejected display, no network publish,
  no hidden source leak, and no `GameState` mutation covered by EditMode tests.

Goal of M10-103:

- Verify the full apply preview flow from queue payload to decision payload,
  validation, apply preview, and session log.
- Keep this verification pure/test-oriented; do not commit queue state yet.

M10-103 completed:

- Added flow-level verification for queue payload -> manual decision payload ->
  decision validation -> apply preview -> session log.
- Covered accepted and validation-rejected paths with no network publish and no
  `GameState` mutation.

Goal of M10-104:

- Write the commit policy spec that separates preview queue state from
  committed queue state.
- Define which later helpers may mutate committed queue state and which
  surfaces remain preview-only.

M10-104 completed:

- Added `PENDING_AUTO_ABILITY_QUEUE_COMMIT_POLICY_SPEC.md`.
- Defined preview queue vs committed queue, event boundary, and M10-105 helper
  constraints.
- Docs-only; no Unity test run required beyond the M10-103 baseline.

Goal of M10-105:

- Add a pure helper for committing `Skip`, `Defer`, and manual `Resolve` to a
  selected queue state clone.
- Return accepted/rejected result metadata for M10-106 replay events.
- Preserve input no-mutation and avoid direct `GameState`/network changes.

M10-105 completed:

- Added `PendingAutoAbilityQueueCommitHelper` and result metadata model.
- Covered `Skip`, `Defer`, manual `Resolve`, rejects, input no-mutation, and
  null pending-list no-normalization.
- Unity compile and EditMode passed at `519/519`.

Goal of M10-106:

- Add replayable event/log model for committed pending AUTO queue decisions.
- Convert commit helper accepted output into event metadata without hidden
  source leaks.
- Keep event creation pure; do not wire UI/network commit yet.

M10-106 completed:

- Added `PendingAutoAbilityQueueCommitEvent` and pure builder.
- Covered JSON round-trip, manual `Resolve` flag, rejected build paths,
  hidden-source safety, and no queue/result mutation.
- Unity compile and EditMode passed at `523/523`.

Goal of M10-107:

- Create a committed trigger check event from manual trigger draft/log data.
- Record RNG/check outcome metadata for replay without replacing real RNG.
- Keep event creation pure until replay integration is wired.

M10-107 completed:

- Added `TriggerCheckCommitEvent` and pure builder from `TriggerCheckLogEntry`.
- Covered JSON round-trip, masked identity preservation, invalid metadata
  rejection, and no source log entry mutation.
- Unity compile and EditMode passed at `527/527`.

Goal of M10-108:

- Add a pure helper to commit trigger allocation modifiers into a
  `CombatModifierLedger` clone.
- Use trigger check bundle/log metadata as input and keep no direct
  `GameState`/network mutation.

M10-108 completed:

- Added `TriggerAllocationCommitHelper` and result model.
- Covered accepted modifier append, empty accepted ledger, missing/rejected
  inputs, masked metadata, and no ledger/bundle mutation.
- Unity compile and EditMode passed at `531/531`.

Goal of M10-109:

- Integrate trigger modifier cleanup timing around the committed ledger helper.
- Keep cleanup pure against ledger clones; no live `GameState` mutation.

M10-109 completed:

- Added `TriggerModifierCleanupIntegration` and hardened cleanup previewer to
  avoid normalizing null source modifier lists.
- Covered EndOfBattle/EndOfTurn cleanup, rejected paths, source no-mutation,
  and Manual/Permanent retention.
- Unity compile and EditMode passed at `535/535`.

Goal of M10-110:

- Generate pending AUTO queue entries from real timing events produced by the
  current game/action flow, not only manual/scaffold publication.
- Keep collection pure and behind existing queue/adaptor boundaries.

M10-110 completed:

- Added explicit adapter commit path from a real `GameEvent` to
  `GameState.pending_auto_abilities`.
- Covered RulesCore-produced timing event collection, unmatched no-op behavior,
  source queue copy-before-assignment, missing-input rejection, no network
  publish, and no `GameState.event_log` append.
- Unity compile and EditMode passed at `539/539`.

Goal of M10-111:

- Add a committed manual-resolved event/log model for unsupported pending AUTO
  abilities.
- Keep it replayable and separate from structured effect execution until M12.

M10-111 completed:

- Added `PendingAutoAbilityManualResolvedEvent` and builder for unsupported
  manual `Resolve` commits.
- Covered accepted event creation, JSON round-trip, rejected/mismatched inputs,
  hidden-source-safe ids/reasons, and source no-mutation.
- Unity compile and EditMode passed at `545/545`.

Goal of M10-112:

- Close M10 with regression verification and docs/status updates.
- Set next target to `M11-01` TimingWindow audit.

M10-112 completed:

- Added `M10_ABILITY_TRIGGER_AUTOMATION_CLOSEOUT.md`.
- Verified data pack, custom pack validation/import, Python unit tests, Unity
  compile, and Unity EditMode `545/545`.
- Marked next target as `M11-01` TimingWindow audit.

Goal of M11-01:

- Audit timing/window names currently implied by actions, phases, trigger
  queues, cleanup timing, and pending AUTO timing strings.
- Produce the gap list needed before `M11-02` Phase/Timing Matrix.

M11-01 completed:

- Added `TimingWindowAuditCatalog` and `TimingWindowAuditReport`.
- Added `TIMING_WINDOW_AUDIT_SPEC.md` with current timing identifiers and gaps.
- Covered enum inventory, pending AUTO timing mappings, gap entries, JSON
  round-trip, and duplicate detection.
- Unity compile and EditMode passed at `550/550`.

Goal of M11-02:

- Convert the audit into the first Phase / Timing Matrix.
- Test representative allowed/rejected combinations without broad behavior
  rewrites.

M11-02 completed:

- Added `TimingWindow`, `PhaseTimingMatrix`, matrix command ids, and JSON
  matrix definition.
- Added `PHASE_TIMING_MATRIX_SPEC.md`.
- Covered action-to-window mapping, game action entries, representative
  allowed/rejected action/trigger/cleanup combinations, JSON round-trip, and
  duplicate detection.
- Unity compile and EditMode passed at `556/556`.

Goal of M11-03:

- Audit current gameplay mutation paths and document/cover which ones already
  go through `RulesCore` command/query facade.
- Keep remaining manual/scaffold-only exceptions explicit before hardening legal
  action masks.

M11-03 completed:

- Added `RulesCoreCommandFacadeCoverage` report.
- Added `RULES_CORE_COMMAND_FACADE_COVERAGE_SPEC.md`.
- Covered all current `GameActionType` facade paths, explicit `UndoLast`
  exception, lookup behavior, JSON round-trip, and duplicate command ids.
- Unity compile and EditMode passed at `561/561`.

Goal of M11-04:

- Harden legal action mask visibility for UI/bot-facing paths.
- Add a helper/report proving current UI/bot/session paths use legal actions or
  documented read-only/manual exceptions.

M11-04 completed:

- Added `LegalActionMaskUsageReport`.
- Added `LEGAL_ACTION_MASK_USAGE_SPEC.md`.
- Covered UI, bot, session, and ability-core paths, hardened/exception status,
  Undo direct mutation exception, JSON round-trip, and duplicate path ids.
- Unity compile and EditMode passed at `566/566`.

Goal of M11-05:

- Add regression helpers/tests proving invalid command paths reject without
  mutating state, event logs, queues, or payload histories.

M11-05 completed:

- Added `NoMutationSnapshot` guard helper.
- Added `REJECT_NO_MUTATION_GUARANTEE_SPEC.md`.
- Covered null/illegal RulesCore rejects, accepted mutation detection, pending
  AUTO queue reject no-mutation, null pending-list no-normalization, and
  missing-payload session publish history preservation.
- Unity compile and EditMode passed at `572/572`.

Goal of M11-06:

- Identify state mutations still missing replayable event metadata.
- Expand event coverage without wiring unsupported card effects.

M11-06 completed:

- Added `EventSourcingCoverageReport`.
- Added `EVENT_SOURCING_COVERAGE_SPEC.md`.
- Covered current `GameActionType` event-sourcing entries, AbilityCore
  structured-effect routing, explicit `UndoLast` and pending AUTO timing commit
  exceptions, JSON round-trip, and duplicate path detection.
- Unity compile and EditMode passed at `578/578`.

Goal of M11-07:

- Replay event/log data and verify final state equality for supported
  `GameEventReducer` surfaces.

M11-07 completed:

- Added `ReplayDeterminismVerifier`.
- Added `REPLAY_DETERMINISM_VERIFIER_SPEC.md`.
- Covered supported RulesCore command replay, divergent final-state rejection,
  unsupported event rejection, source no-mutation, result JSON round-trip, and
  missing-state rejection paths.
- Unity compile and EditMode passed at `584/584`.

Goal of M11-08:

- Verify clone/restore paths isolate simulated branches from live state.

M11-08 completed:

- Added `SnapshotRollbackVerifier`.
- Added `SNAPSHOT_ROLLBACK_VERIFIER_SPEC.md`.
- Covered branch-only mutation, illegal branch rejection, missing input
  rejection, snapshot restore independence after later live mutation, and
  result JSON round-trip.
- Unity compile and EditMode passed at `589/589`.

Goal of M11-09:

- Strengthen player/spectator/bot views against hidden hand, deck, and source
  leaks.

M11-09 completed:

- Added `HiddenStateViewHardeningVerifier`.
- Added `HIDDEN_STATE_VIEW_HARDENING_SPEC.md`.
- Covered current player/spectator masking policy, source no-mutation, direct
  private-zone/event masking, missing input rejection, and result JSON
  round-trip.
- Unity compile and EditMode passed at `594/594`.

Goal of M11-10:

- Prepare validation coverage for CounterBlast, SoulBlast, Energy, and
  once-per-turn/once-per-fight flags without implementing full card text
  execution.

M11-10 completed:

- Added pure `ResourceLedgerState`, `ResourceCostRequest`, and
  `ResourceLedger.ValidateCost`.
- Added `RESOURCE_LEDGER_SPEC.md`.
- Covered CounterBlast derivation, accepted CB/SB/Energy costs, unavailable
  costs, duplicate once flags, negative costs, player mismatch, missing inputs,
  source no-mutation, and JSON round-trip.
- Unity compile and EditMode passed at `600/600`.

Goal of M11-11:

- Separate Standard, V-Premium, and Premium RuleSet flags from shared
  hard-coded logic.

M11-11 completed:

- Added `RuleSetProfile`, `RuleSetProfileCatalog`, and feature flags for core
  Standard, V-Premium, and Premium profiles.
- Added `RULE_SET_PROFILE_SPEC.md`.
- Covered Standard/V/Premium flag separation, state-based resolution
  no-mutation, duplicate profile/alias rejection, JSON round-trip, and
  missing/unknown format rejection.
- Unity compile and EditMode passed at `607/607`.

Goal of M11-12:

- Run the RulesCore regression suite and update docs before moving to M12
  structured ability data.

M11-12 completed:

- Added `M11_RULES_CORE_COMPLETION_CLOSEOUT.md`.
- Re-ran data pack verification, custom pack validation/import, Python unit
  tests, Unity compile, and Unity EditMode.
- Verified pack `10836/10836`, Python unit `13/13`, and Unity EditMode
  `607/607`.
- Marked next target as `M12-01` Ability schema v1.

Goal of M12-01:

- Define ability JSON schema v1 for trigger, timing, cost, target, effect, and
  duration fields before writing validators or runtime loading.

M12-01 completed:

- Added `data/schemas/ability_schema_v1.json`.
- Added `data/templates/ability_schema_v1/sample_abilities.json`.
- Added `ABILITY_SCHEMA_V1_SPEC.md`.
- Added Python structural tests for schema shape, required sections, template
  enums, and sample shape.
- Python unit tests passed at `17/17`.

Goal of M12-02:

- Add a Python/Pydantic validator for `ability_schema_v1` files before runtime
  pack loading.

M12-02 completed:

- Added `tools/data/validate_ability_schema.py`.
- Added `ABILITY_SCHEMA_VALIDATOR_SPEC.md`.
- Added validator tests for valid sample data, duplicate ability ids, missing
  required sections, invalid enums, and once-per-turn key rejection.
- Verified CLI validator on `sample_abilities.json`.
- Python unit tests passed at `22/22`.

Goal of M12-03:

- Add a Unity runtime ability registry that loads validated ability schema data
  and maps abilities to card ids while preserving manual fallback.

M12-03 completed:

- Added `RuntimeAbilityRegistry` and structured ability runtime DTOs.
- Added `RUNTIME_ABILITY_REGISTRY_SPEC.md`.
- Covered card/ability indexing, manual fallback preservation, cloned returned
  data, duplicate ability id rejection, missing/wrong schema rejection, and load
  result JSON round-trip.
- Unity compile and EditMode passed at `612/612`.

Goal of M12-04:

- Add Cost template v1 to convert structured costs into resource-ledger
  validation requests without mutating live `GameState`.

M12-04 completed:

- Added `StructuredCostTemplate`.
- Added `STRUCTURED_COST_TEMPLATE_SPEC.md`.
- Covered CB/SB/Energy aggregation, once key generation, ledger validation
  without ledger mutation, discard manual placeholder, negative cost rejection,
  duplicate once rejection, and result JSON round-trip.
- Unity compile and EditMode passed at `618/618`.

Goal of M12-05:

- Add Target template v1 for self/unit/circle/hand/drop/soul/damage/deck target
  shapes without applying effects.

M12-05 completed:

- Added `StructuredTargetTemplate`.
- Added `STRUCTURED_TARGET_TEMPLATE_SPEC.md`.
- Covered self/any public target resolution, face-down skipping, optional
  target acceptance, required count rejection, hidden/unsupported zone manual
  placeholders, circle manual placeholder, source no-mutation, and result JSON
  round-trip.
- Unity compile and EditMode passed at `624/624`.

Goal of M12-06:

- Add draw and move-zone effect templates that preview/execute only through
  RulesCore-supported command paths.

M12-06 completed:

- Added `StructuredEffectTemplate` for `draw` and `move_zone`.
- Added `STRUCTURED_EFFECT_DRAW_MOVE_TEMPLATE_SPEC.md`.
- Covered draw preview no-mutation, draw apply through RulesCore/event log,
  move-zone apply through RulesCore, unsupported/manual effect fallback, invalid
  move reject no-mutation, and result JSON round-trip.
- Unity compile and EditMode passed at `630/630`.

Goal of M12-07:

- Add resource operation effect templates for CounterCharge, SoulCharge,
  SoulBlast, and CounterBlast as safe preview/execution helpers.

M12-07 completed:

- Added `GameActionType.ResourceFlip` and `GameResourceOperationType`.
- Added event-sourced CounterBlast/CounterCharge damage face-up flips through
  `RulesCore`, `GameActionService`, `GameEventReducer`, undo, and replay.
- Added `counter_blast` and `counter_charge` to `StructuredEffectTemplate`.
- Kept `soul_charge` and `soul_blast` as manual-resolution placeholders because
  live `GameState` has no authoritative Soul zone yet.
- Added `STRUCTURED_EFFECT_RESOURCE_OPS_TEMPLATE_SPEC.md`.
- Updated event-sourcing, facade, timing audit, pending AUTO timing, event-log
  formatting, and replay determinism coverage.
- Unity compile and EditMode passed at `636/636`.

Goal of M12-08:

- Add PowerPlus/CriticalPlus structured modifier effects through the existing
  combat modifier ledger and cleanup timing boundaries.

M12-08 completed:

- Added `StructuredModifierEffectTemplate` for `power_plus` and
  `critical_plus`.
- Kept modifier execution out of `GameState.event_log`; it writes to a supplied
  `CombatModifierLedger` only.
- Preview clones the source ledger and preserves source `GameState`/ledger.
- Apply mutates only the supplied combat modifier ledger.
- Added explicit duration mapping to `CombatModifierExpiration`.
- Added `STRUCTURED_MODIFIER_EFFECT_TEMPLATE_SPEC.md`.
- Unity compile and EditMode passed at `642/642`.

Goal of M12-09:

- Add a small ability fixture DSL for before/action/after structured ability
  scenario tests.

M12-09 completed:

- Added `StructuredAbilityFixture`, expectation model, result model, and
  `StructuredAbilityFixtureRunner`.
- Runner clones source state, validates cost, resolves target, applies supported
  structured effects/modifiers, and compares opt-in expected counts.
- Added `STRUCTURED_ABILITY_FIXTURE_DSL_SPEC.md`.
- Covered draw, CounterBlast, PowerPlus, mismatch reporting, manual fallback,
  source no-mutation, and fixture/result JSON round-trip.
- Unity compile and EditMode passed at `648/648`.

Goal of M12-10:

- Create the first small structured card pack using only supported templates and
  fixture tests.

M12-10 completed:

- Added `data/packs/vanguard_th/abilities/structured_ability_pack_m12_10.json`
  with 20 template smoke abilities mapped to real local card ids.
- Pack covers `draw`, `counter_blast`, `counter_charge`, `power_plus`, and
  `critical_plus`.
- Added Python pack validation tests.
- Added Unity tests that load the actual pack file through
  `RuntimeAbilityRegistry` and run draw/PowerPlus abilities through the fixture
  DSL.
- Added `FIRST_STRUCTURED_CARD_PACK_SPEC.md`.
- Python tests passed at `24/24`.
- Unity compile and EditMode passed at `651/651`.

Goal of M12-11:

- Connect unsupported structured ability paths to the existing manual resolution
  bridge explicitly.

M12-11 completed:

- Added `StructuredAbilityManualFallbackBridge`.
- Bridge creates a `PendingAutoAbility`, `PendingAutoAbilityResolutionRequest`,
  and `PendingAutoAbilityManualResolutionDecision` with `Resolve` using existing
  factories.
- Bridge preserves hidden source boundaries and does not mutate `GameState`,
  queues, session storage, UI, or network state.
- Added `STRUCTURED_ABILITY_MANUAL_FALLBACK_BRIDGE_SPEC.md`.
- Unity compile and EditMode passed at `657/657`.

Goal of M12-12:

- Run M12 closeout verification, update status docs, and set M13 as the next
  target.

M12-12 completed:

- Added `M12_STRUCTURED_CARD_ABILITY_DATA_CLOSEOUT.md`.
- Verified Vanguard TH pack, custom pack validation/import, M12 structured
  ability pack validation, Python tests, Unity compile, and Unity EditMode.
- M12 is closed.

Goal of M13-01:

- Implement owner-private room initialization with private deck/hand/source
  state boundaries and explicit client-trust assumptions.

M13-01 completed:

- Added owner-private public count metadata to `RoomPlayerInfo`.
- Added `LocalOwnerPrivateSession`, `OwnerPrivateRoomInitializationResult`,
  and `OwnerPrivateRoomInitializer`.
- Local true state now initializes from only the local deck plus both
  commitments.
- Opponent private zones are synthetic hidden placeholders from public count
  metadata only.
- Commitment mismatch rejects without mutating room/deck inputs.
- Normal `deck_commitment` gameplay remains blocked until explicit
  client-trust UX and later public-event state application/reconnect paths.
- Unity compile passed.
- Unity EditMode passed at `661/661`.

Goal of M13-02:

- Add explicit trusted-client warning UX for commitment-only rooms.
- Make clear this is casual/trusted-client mode, not ranked secure server mode.
- Keep normal gameplay start blocked until the warning acknowledgment is wired.

M13-02 completed:

- Added `DeckPrivacyGameplayPolicy.Evaluate(room, acknowledged)`.
- Added lobby client-trust acknowledgment state and `Acknowledge Trust` UI.
- Pre-ack commitment rooms reject with
  `DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED`.
- Post-ack commitment rooms still reject with
  `OWNER_PRIVATE_GAMEPLAY_PATH_INCOMPLETE`.
- Added `CLIENT_TRUST_UX_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `662/662`.

Goal of M13-03:

- Add online command envelope metadata: player id, sequence, room game id, and
  state cursor.
- Keep command envelopes separate from true `GameEvent` envelopes.
- Add validation hooks for stale/out-of-turn checks without changing gameplay
  transport behavior yet.

M13-03 completed:

- Added `NetworkCommandEnvelope`, factory, and basic shape validator.
- Reserved Photon payload event code `12` for command envelopes.
- Added JSON/Photon round-trip tests and wrong event-code rejection.
- Kept dispatch/execution out of scope.
- Unity compile passed.
- Unity EditMode passed at `666/666`.

Goal of M13-04:

- Reject command envelopes with stale state cursor.
- Reject out-of-turn command envelopes for turn-owner-gated actions.
- Reject player id/index owner mismatch.
- Preserve no-mutation on rejects.

M13-04 completed:

- Added state-aware command envelope validation.
- Rejects stale cursor, room game id mismatch, owner mismatch, actor mismatch,
  and out-of-turn commands.
- Verified stale reject does not mutate source `GameState`.
- Unity compile passed.
- Unity EditMode passed at `670/670`.

Goal of M13-05:

- Apply public event delivery to public/opponent views without true opponent
  card ids.
- Preserve true/private event separation for commitment-mode delivery.
- Keep normal commitment gameplay blocked until reconnect/public replay path is
  ready.

M13-05 completed:

- Added `NetworkPublicGameEventApplier`.
- Hidden public events update only counts/placeholders.
- Revealed public events add only public card id/instance id.
- Owner-private session delivery updates `opponent_public_view`,
  `public_event_log`, and `event_cursor`.
- `local_true_state` and `GameState.event_log` stay untouched.
- Added `PUBLIC_EVENT_MASKING_DELIVERY_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `674/674`.

Goal of M13-06:

- Define public-event reconnect cursor behavior.
- Rebuild public/opponent view from initial public view plus public event batch.
- Keep true reconnect batch blocked for commitment-only rooms.

M13-06 completed:

- Added `NetworkPublicEventBatch` and public reconnect recovery helper.
- Reserved Photon payload event code `13` for public event batches.
- Public reconnect applies through masked public event applier.
- Commitment/non-shared privacy rooms no longer emit true reconnect events.
- Added `PUBLIC_RECONNECT_RECOVERY_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `678/678`.

Goal of M13-07:

- Block start when local pack hash does not match room pack hash.
- Block start when deck hash/commitment metadata is missing or inconsistent.
- Cover shared-deck-code and commitment-room start guards.

M13-07 completed:

- Shared deck-code start recomputes canonical deck hash after deck-code import.
- Host/guest missing or mismatched deck hash blocks start.
- Added `START_GUARD_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `679/679`.

Goal of M13-08:

- Add ready/start/end/rematch lifecycle model.
- Keep lifecycle transitions explicit and testable.
- Prevent invalid state transitions from mutating room state.

M13-08 completed:

- Added room lifecycle ready/start/end/rematch helper.
- Added `RoomPlayerInfo.ready`.
- Lifecycle transitions return cloned rooms and reject invalid transitions
  without source mutation.
- Added `ROOM_LIFECYCLE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `683/683`.

Goal of M13-09:

- Build spectator-safe public replay from public event batches.
- Keep spectator state public-only.
- Verify replay sync from public event cursor without hidden data leaks.

M13-09 completed:

- `NetworkPublicGameReplayPlayer` now applies public events into
  `CurrentStateView`.
- Added cursor-based `ApplyBatch(...)` for public spectator sync.
- Batch sync rejects cursor mismatch and invalid public events without mutating
  replay state.
- Visible event logs and replay input events are cloned to prevent UI mutation
  of replay history.
- Added `PUBLIC_SPECTATOR_REPLAY_SYNC_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `688/688`.

Goal of M13-10:

- Export room metadata, commitments, public events, and result as an audit log.
- Keep audit export public/spectator safe.
- Do not unblock ranked/server-authoritative gameplay in this milestone.

M13-10 completed:

- Added `TournamentAuditLog`, player records, result metadata, and factory.
- Audit export includes room metadata, pack hashes, player deck
  hashes/commitments, sanitized public events, and result metadata.
- Audit export omits deck codes, reveal nonces, and hidden card payloads.
- Added `TOURNAMENT_AUDIT_LOG_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `692/692`.

Goal of M13-11:

- Run or extend live Photon gameplay smoke for the current multiplayer path.
- Verify start/play/reconnect/end behavior where local AppId config allows it.
- Keep commitment-only ranked/secure assumptions out of scope.

M13-11 completed:

- Extended `PhotonGameSessionSmokeTestRunner` to publish `playing` and `ended`
  room lifecycle states.
- Verified live Photon start/play/reconnect/end with local config.
- Unity compile passed.
- Unity EditMode passed at `692/692`.
- Live smoke passed in `work/photon_game_session_smoke_m13_11_a.log`.

Goal of M14-01:

- Upgrade the heuristic bot to use the current evaluator and legal-action
  surface.
- Keep bot decisions hidden-state safe.
- Do not introduce advanced ISMCTS/RL in this milestone.

M14-01 completed:

- Added `HeuristicBotV2` legal-action ranker.
- Candidate actions execute only on cloned branch states.
- Scoring uses player-view evaluation and masks simulated draw cards before
  `BoardResourceEvaluator` can inspect top-deck identity.
- Decision reasons omit card ids and instance ids.
- Added `HEURISTIC_BOT_V2_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `697/697`.

Goal of M14-02:

- Add an advisory guard decision bot.
- Use visible shield, unknown hand count, damage risk, and trigger odds.
- Keep the recommendation hidden-state safe and no-`GameState` mutation.

M14-02 completed:

- Added advisory `GuardDecisionBot`.
- Guard result covers `NoGuard`, `Guard`, `PerfectGuardPreferred`, and
  `CannotGuard`.
- Shield needed is rounded to shield steps and considers lethal damage,
  high-damage, and exact trigger-risk inputs.
- Recommendations use visible/masked shield estimates without mutating
  `GameState`.
- Added `GUARD_DECISION_BOT_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `704/704`.

Goal of M14-03:

- Use exact trigger probability as a planning input for attack choice.
- Do not use probability as the actual trigger/RNG outcome.
- Keep output advisory and deterministic.

M14-03 completed:

- Added `TriggerRiskAttackChoice` advisory helper.
- Valid exact trigger probability feeds battle candidate scoring only.
- Invalid/missing probability falls back to zero trigger risk.
- Result explicitly marks `AppliesTriggerOutcome = false`.
- Source `GameState`, event log, and power modifiers remain unchanged.
- Added `TRIGGER_RISK_ATTACK_CHOICE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `709/709`.

Goal of M14-04:

- Expand battle sequence search scoring with guard estimate and trigger-risk
  inputs.
- Keep output advisory and visible-attacker only.
- Preserve no `GameState` mutation.

M14-04 completed:

- Added `BattleSequenceSearchV2` wrapper over the M9 battle search prototype.
- V2 scoring adds guard-pressure contribution from `OpponentGuardEstimate`.
- Valid trigger probability is carried through as trigger risk/pressure input.
- Hidden attackers remain skipped and returned attackers are cloned summaries.
- Added `BATTLE_SEQUENCE_SEARCH_V2_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `713/713`.

Goal of M14-05:

- Add a pure snapshot simulation helper for bot/search branch actions.
- Apply legal actions only to cloned branch state.
- Return accepted/rejected branch results without mutating live state.

M14-05 completed:

- Added `SnapshotSimulationPath` helper for branch-only action sequences.
- Branch actions run through `RulesCore.TryExecute` on `GameStateSnapshot`
  clones.
- Result returns per-action status and `branch_state_json`.
- Live state hash is checked before/after simulation.
- Added `SNAPSHOT_SIMULATION_PATH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `718/718`.

Goal of M14-06:

- Connect archetype playbook matching to bot/search helpers as deterministic
  bias.
- Preserve balanced defaults when no playbook matches.
- Keep no `GameState` mutation.

M14-06 completed:

- Added `PlaybookIntegratedBot` wrapper over `HeuristicBotV2`.
- Playbook matching runs through player-view public board signals.
- Priority call ids add deterministic rear-guard call bias.
- Unmatched decks fall back to `default_balanced`.
- Summaries omit card ids and instance ids.
- Added `PLAYBOOK_INTEGRATION_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `723/723`.

Goal of M14-07:

- Export combo line summaries from current playbook, board, guard, trigger, and
  battle search helpers.
- Include replay/log references where available without executing live actions.
- Keep deterministic JSON output and no `GameState` mutation.

M14-07 completed:

- Expanded `ComboDiscoveryReport` with `combo_lines`.
- Combo lines include rank, candidate id, total score, guard pressure, trigger
  pressure, trigger risk, replay reference, and explanation.
- Report includes `source_event_count`.
- Replay reference points at source event-log count and candidate id without
  executing actions.
- Added `OFFLINE_COMBO_DISCOVERY_OUTPUT_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `723/723`.

Goal of M14-08:

- Add compact bot debug trace model.
- Explain bot choice inputs and scores.
- Keep trace sanitized and deterministic.

M14-08 completed:

- Added `BotDebugTrace` and `BotDebugTraceLine`.
- Trace is generated separately from `GameState`, replay, and network payloads.
- Trace ranks playbook-integrated bot evaluations by score.
- Trace summaries are sanitized and omit card ids/instance ids.
- Added `BOT_DEBUG_TRACE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `727/727`.

Goal of M14-09:

- Add an ISMCTS readiness gate/checklist artifact.
- Verify legal action, hidden-state, snapshot, probability, debug trace, and
  no-mutation prerequisites.
- Keep advanced search blocked unless every required gate is ready.

M14-09 completed:

- Added `IsmctsReadinessGate` checklist artifact.
- Default checklist covers legal actions, hidden-state boundaries, snapshot
  simulation, probability planning-only, guard/battle advisors, debug trace,
  and no-mutation tests.
- Blocked checklist items prevent advanced search.
- Added `ISMCTS_READINESS_GATE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `731/731`.

Goal of M14-10:

- Add a small readiness-gated advanced search prototype.
- Keep prototype deterministic, bounded, and one-ply only.
- Do not add RL/self-play or long rollout loops.

M14-10 completed:

- Added `AdvancedSearchPrototype`.
- Search is gated by `IsmctsReadinessGate`.
- Prototype ranks legal actions with `HeuristicBotV2` and verifies bounded
  candidates through `SnapshotSimulationPath`.
- Output is deterministic, JSON round-trippable, no-mutation, and no-leak for
  opponent private/top-deck ids.
- Added `ADVANCED_SEARCH_PROTOTYPE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `736/736`.

Goal of M15-01:

- Add a backward-compatible custom pack schema v2 envelope.
- Keep v1 custom packs valid.
- Preserve source schema metadata in imported runtime manifests.

M15-01 completed:

- Added schema v2 validation support to `validate_custom_pack_schema.py`.
- Added v2 capabilities for cards, images, abilities, and custom formats.
- Added v2 dependency metadata validation.
- Added source metadata fields to custom pack runtime manifests.
- Added `data/templates/custom_pack_v2`.
- Added `CUSTOM_PACK_V2_SCHEMA_SPEC.md`.
- Python tests passed at `29/29`.

Goal of M15-02:

- Allow schema v2 custom packs to include structured ability JSON files.
- Validate custom ability files with the existing ability schema validator.
- Preserve ability file hash metadata for multiplayer pack matching.
- Do not execute custom abilities in this milestone.

M15-02 completed:

- Added `abilities_file` validation for schema v2 custom packs.
- Validates ability files through `ability_schema_v1`.
- Rejects ability `card_id` values not present in the same custom pack.
- Preserves `source_ability_count` and `source_ability_data_hash` in runtime
  manifests.
- Added `CUSTOM_PACK_ABILITY_DATA_SPEC.md`.
- Python tests passed at `31/31`.

Goal of M15-03:

- Expose pack validation status for runtime/UI surfaces.
- Report schema version, capabilities, ability count/hash, and validation
  warnings/errors.
- Keep validation status read-only and separate from enabling/disabling packs.

M15-03 completed:

- Added `CardPackValidationStatus` and compact formatter.
- Added schema v2 source metadata fields to `CardPackManifest`.
- Card Browser appends read-only validation status to its existing status text.
- Added `PACK_VALIDATION_STATUS_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `740/740`.

Goal of M15-04:

- Define a local pack registry state for installed packs.
- Support enable/disable selection without deleting pack data.
- Keep card database source packs immutable.
- Do not change the active pack automatically.

M15-04 completed:

- Added `CardPackRegistryState`, `CardPackRegistryEntry`, and mutation result.
- Added clone-only `CardPackRegistryManager.SetEnabled`.
- Added stable reject reasons.
- Added `PACK_MANAGER_ENABLE_DISABLE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `744/744`.

Goal of M15-05:

- Define a custom-format-facing profile model that references existing
  RuleSet profile ids.
- Keep it data/validation only.
- Prepare M15-06 through M15-08 format flag tasks.

M15-05 completed:

- Added `CustomFormatProfile` and catalog definition.
- Validation resolves base RuleSet profiles through `RuleSetProfileCatalog`.
- Rejects duplicate format ids, aliases, and allowed pack ids.
- Added `CUSTOM_FORMAT_PROFILE_MODEL_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `748/748`.

Goal of M15-06:

- Add a Standard custom format preset using the custom format model.
- Keep Standard flags aligned with the existing core `standard`
  `RuleSetProfile`.
- Keep the preset data-only.

M15-06 completed:

- Added `CustomFormatProfileCatalog.CreateStandardPreset`.
- Standard preset delegates flags to the core `standard` `RuleSetProfile`.
- Added `STANDARD_FORMAT_FLAGS_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `749/749`.

Goal of M15-07:

- Add a V-Premium custom format preset using the custom format model.
- Keep V-Premium flags aligned with the existing core `v_premium`
  `RuleSetProfile`.
- Keep the preset data-only.

M15-07 completed:

- Added `CustomFormatProfileCatalog.CreateVPremiumPreset`.
- V-Premium preset delegates flags to the core `v_premium` `RuleSetProfile`.
- Added `V_PREMIUM_FORMAT_FLAGS_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `750/750`.

Goal of M15-08:

- Add a Premium custom format preset using the custom format model.
- Keep Premium flags aligned with the existing core `premium` `RuleSetProfile`.
- Keep the preset data-only.

M15-08 completed:

- Added `CustomFormatProfileCatalog.CreatePremiumPreset`.
- Premium preset delegates flags to the core `premium` `RuleSetProfile`.
- Added `PREMIUM_FORMAT_FLAGS_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `751/751`.

Goal of M15-09:

- Add a read-only custom format sandbox preview.
- Resolve format ids and aliases to cloned base `RuleSetProfile` summaries.
- Reject invalid catalogs, unknown formats, and disallowed packs without source mutation.
- Keep this data/validation only; do not enforce gameplay format behavior.

M15-09 completed:

- Added `CustomFormatProfileCatalog.CreateCorePresetCatalog`.
- Added `CustomFormatSandbox` and JSON-safe `CustomFormatSandboxResult`.
- Added `CUSTOM_FORMAT_SANDBOX_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `757/757`.

Goal of M15-10:

- Add closeout validation around custom format profile + sandbox behavior.
- Verify Standard/V-Premium/Premium presets remain distinct and data-only.
- Verify custom pack/format validation boundaries stay separate from gameplay
  enforcement.

M15-10 completed:

- Added custom format validation closeout tests.
- Added `M15_CUSTOM_PACKS_FORMATS_CLOSEOUT.md`.
- Custom pack v1/v2 validation and v2 import passed.
- Python tests passed at `31/31`.
- Unity compile passed.
- Unity EditMode passed at `762/762`.

Goal of M16-01:

- Polish the PlayTable pending AUTO panel without changing queue semantics.
- Keep item list and selection diagnostics available.
- Preserve hidden-source redaction and no `GameState` mutation.

M16-01 completed:

- Added `PendingAutoAbilityPanelFormatter`.
- PlayTable pending AUTO summary uses compact panel text.
- Added `PLAYTABLE_PENDING_AUTO_PANEL_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `767/767`.

Goal of M16-02:

- Polish the PlayTable trigger panel without changing trigger RNG/log semantics.
- Keep checked-card ids out of the compact latest line.
- Preserve existing trigger log and draft diagnostic formatters.

M16-02 completed:

- Added `TriggerCheckPanelFormatter`.
- PlayTable trigger summary uses compact panel text.
- Added `PLAYTABLE_TRIGGER_PANEL_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `772/772`.

Goal of M16-03:

- Polish the PlayTable manual resolution panel without changing preview semantics.
- Keep decision list, validation, and apply preview diagnostics available.
- Preserve hidden-source redaction and no `GameState` mutation.

M16-03 completed:

- Added `PendingAutoAbilityManualResolutionPanelFormatter`.
- PlayTable manual resolution summary uses compact panel text.
- Added `PLAYTABLE_MANUAL_RESOLUTION_PANEL_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `778/778`.

Goal of M16-04:

- Polish the PlayTable event/replay panel without changing replay semantics.
- Keep event order newest-first.
- Avoid card instance ids in compact resource flip lines.

M16-04 completed:

- Added `PlayTableEventReplayPanelFormatter`.
- PlayTable event log surface uses compact event/replay panel text.
- Added `PLAYTABLE_EVENT_REPLAY_PANEL_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `783/783`.

Goal of M16-05:

- Polish the PlayTable online status/cursor panel without changing network
  sync semantics.
- Keep reconnect/event cursor behavior unchanged.
- Preserve no `GameState` mutation.

M16-05 completed:

- Polished `PlayTableModeSummaryFormatter` online text into explicit status,
  transport, cursor, and trigger-log segments.
- Reconnect sync now shows as an explicit `Reconnect` segment.
- Added `PLAYTABLE_ONLINE_STATUS_CURSOR_PANEL_POLISH_SPEC.md`.
- Updated `PLAYTABLE_MODE_SUMMARY_FORMATTER_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `783/783`.

Goal of M16-06:

- Polish Deck Builder filter/status text without changing deck contents,
  repository queries, or validation semantics.
- Add pure formatter/helper tests for empty filters, active filters, and
  result-count/status edge cases.

M16-06 completed:

- Added `DeckBuilderFilterPanelFormatter`.
- Card pool status shows total/showing counts, active filters, pack version,
  and pack validation through a pure formatter.
- Deck panel status shows counts, issue totals, and playable state through the
  same formatter.
- Added `DECK_BUILDER_FILTER_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `789/789`.

Goal of M16-07:

- Polish Card Browser Thai search feedback without changing pack data.
- Preserve existing repository query behavior unless a covered normalization
  bug is found.
- Add tests for Thai/English query labels and empty-result messaging.

M16-07 completed:

- Added `CardBrowserSearchPanelFormatter`.
- Card Browser no-result detail text now shows active Thai/English query,
  series, and clan filters.
- Search whitespace is normalized for display only.
- Added `CARD_BROWSER_THAI_SEARCH_POLISH_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `795/795`.

Goal of M16-08:

- Review card image loading fallback paths in browser/deck/detail surfaces.
- Add deterministic missing-image fallback text or visual status without
  moving downloaded images.
- Add tests for missing path/null texture formatting where practical.

M16-08 completed:

- Added `CardImageFallbackStatusFormatter`.
- Added `CardImageCache.IsFallbackTexture()` read-only detection.
- Card Browser tile labels and detail text now report image fallback only when
  fallback is used.
- Added `BROKEN_IMAGE_FALLBACK_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `800/800`.

Goal of M16-09:

- Review Android-oriented responsive profiles and touch target sizing.
- Add deterministic layout QA helpers or tests for compact/tablet profiles.
- Do not redesign screens or change gameplay behavior.

M16-09 completed:

- Hardened phone landscape and tablet touch target heights to `48`.
- Added `ResponsiveLayoutQaVerifier` for Android reference viewport checks.
- Verified toolbar width, tile cell, panel, side-panel, and play-height budgets.
- Added `ANDROID_TOUCH_LAYOUT_QA_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `805/805`.

Goal of M16-10:

- Run or document Windows editor smoke for card browser/deck/playtable flow.
- Run or document Android build/smoke feasibility from current Unity setup.
- Record exact commands/log paths and blockers for M18 build artifact work.

M16-10 completed:

- Added `ClientSmokeFlowVerifier` for card browser, deck builder, Play Table,
  and Android layout smoke checks.
- Added `ClientSmokeFlowRunner` editor command for enabled scene,
  Windows/Android build target support, and Photon define checks.
- Smoke runner passed with no blockers at
  `client/unity/VanguardThaiSim/work/client_smoke_m16_10_a.log`.
- Added `WINDOWS_ANDROID_SMOKE_FLOW_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `806/806`.

Goal of M17-01:

- Add a headless CLI runner entry point for deterministic local simulation.
- Keep it separate from Unity UI and online transport.
- Accept minimal hard-coded/default smoke inputs first; full deck/ruleset/seed
  CLI arguments are M17-02.

M17-01 completed:

- Added `HeadlessSimulationResult`.
- Added `HeadlessSimulationRunner.RunDefault()` with fixed seed `1701`.
- Added `HeadlessSimulationCliRunner` editor command with optional
  `-headlessResultPath`.
- Default run executes Draw, MoveCard, SetPhase, and AddGiftMarker through
  RulesCore without UI or Photon.
- Added `HEADLESS_CLI_RUNNER_SPEC.md`.
- Headless CLI wrote
  `client/unity/VanguardThaiSim/work/headless_m17_01_result.json`.
- Unity compile passed.
- Unity EditMode passed at `807/807`.

Goal of M17-02:

- Add bounded CLI arguments for deck source, ruleset, and seed.
- Preserve M17-01 default behavior when no arguments are supplied.
- Reject invalid CLI inputs cleanly and keep result output minimal until
  M17-03.

M17-02 completed:

- Added `HeadlessSimulationRequest`.
- Added `HeadlessSimulationCliArguments` parser for seed, ruleset, deck code,
  and result path.
- `-headlessSeed 42 -headlessRuleset Premium` writes accepted JSON.
- Invalid seed input writes rejected JSON without starting simulation.
- Valid deck-code input is covered by EditMode test using runtime pack data.
- Added `HEADLESS_CLI_INPUT_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `812/812`.

Goal of M17-03:

- Expand headless output into replay/result artifact paths.
- Keep event identity and hidden-state policy explicit.
- Add deterministic output tests.

M17-03 completed:

- Added `HeadlessReplayArtifact` and redacted replay event records.
- Added `HeadlessSimulationRunner.RunWithReplay()`.
- CLI accepts `-headlessReplayPath`.
- CLI writes result JSON and replay JSON.
- Replay output uses hidden-state policy
  `local_headless_trace_card_instance_ids_redacted`.
- Added `HEADLESS_CLI_REPLAY_OUTPUT_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `813/813`.

Goal of M17-04:

- Add bounded batch runner over deterministic headless simulations.
- Summarize accepted/blocked runs.
- Keep advanced RL/self-play training gated until observation/action/reward API
  work.

M17-04 completed:

- Added `HeadlessBatchSimulationRequest`.
- Added `HeadlessBatchSimulationResult`.
- Added `HeadlessBatchSimulationRunner`.
- Added `HeadlessBatchSimulationCliRunner`.
- Batch count is guarded to `1..50`.
- Batch CLI wrote
  `client/unity/VanguardThaiSim/work/headless_batch_m17_04_result.json`.
- Added `HEADLESS_BATCH_SELF_PLAY_RUNNER_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `818/818`.

M17-05 completed:

- Added `HeadlessDatasetExport`.
- Added `HeadlessDatasetRunRecord`.
- Added `HeadlessDatasetExporter.FromBatch()`.
- Dataset schema v1 exports summary metrics only from headless batch results.
- Dataset export intentionally omits card ids, card instance ids, replay records,
  and hidden state.
- Added `HEADLESS_DATASET_EXPORT_SCHEMA_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `820/820`.

M17-06 completed:

- Added `HeadlessObservation`.
- Added `HeadlessActionMaskEntry`.
- Added `HeadlessRewardSignal`.
- Added `HeadlessObservationActionRewardSample`.
- Added `HeadlessObservationActionRewardApi`.
- Observation uses player-view masking plus `RulesCore.GetLegalActions`.
- Action masks omit card ids, card instance ids, and legal-action labels.
- Reward model is `m17_06_smoke_acceptance_v1`, not win/loss reward shaping.
- Added `HEADLESS_OBSERVATION_ACTION_REWARD_API_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `824/824`.

M17-07 completed:

- Added `HeadlessPerformanceProfileRequest`.
- Added `HeadlessPerformanceProfileResult`.
- Added `HeadlessPerformanceRunRecord`.
- Added `HeadlessPerformanceProfiler.Run()`.
- Profiler records timing summaries only and keeps simulation behavior
  unchanged.
- Run count is bounded to `1..50`.
- Added `HEADLESS_PERFORMANCE_PROFILING_HARNESS_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `827/827`.

M17-08 completed:

- Added `PackedStateDecisionInput`.
- Added `PackedStateDecisionReport`.
- Added `PackedStateDecisionGate.Evaluate()`.
- Gate defaults to `defer` and keeps readable `GameState` as source of truth.
- `allow_prototype` is possible only after safety gates pass and profiler is
  slower than target.
- Added `PACKED_STATE_DECISION_GATE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `831/831`.

M17-09 completed:

- Added `DistributedWorkerPrototypeSpecDocument`.
- Added `DistributedWorkerPrototypeSpecValidationResult`.
- Added `DistributedWorkerPrototypeSpec.CreateDefault()` and `Validate()`.
- Worker spec is explicitly `spec_only`.
- Validation enforces no hidden state, no card instance ids, no live network,
  no packed-state migration, no RL training, and no cluster scope.
- Added `DISTRIBUTED_WORKER_PROTOTYPE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `835/835`.

M17-10 completed:

- Added `DistributedWorkerPrototypeRequest`.
- Added `DistributedWorkerPrototypeResult`.
- Added `DistributedWorkerPrototype.Run()`.
- Worker validates the M17-09 spec before running.
- Worker runs bounded headless batches and optional dataset/profile artifacts.
- Worker remains local in-memory only: no Photon, no cluster, no RL training.
- Added `DISTRIBUTED_WORKER_LOCAL_PROTOTYPE_SPEC.md`.
- Unity compile passed.
- Unity EditMode passed at `839/839`.

M18-01 completed:

- Added `.github/workflows/python-tests.yml`.
- Workflow runs on push and pull request.
- Workflow uses Python 3.11.
- Workflow command matches local baseline:
  `python -m unittest discover -s tests -p "test_*.py"`.
- Added `CI_PYTHON_TESTS_SPEC.md`.
- Local Python unit tests passed at `31/31`.

M18-02 completed:

- Added `.github/workflows/data-validation.yml`.
- Workflow runs on push and pull request.
- Workflow uses Python 3.11.
- Workflow runs pack verification, custom pack schema validation, custom pack
  import smoke, and structured ability validation.
- Added `CI_DATA_VALIDATION_SPEC.md`.
- Local data validation commands passed.

M18-03 completed:

- Added `.github/workflows/unity-compile.yml`.
- Workflow runs on push and pull request.
- Workflow uses self-hosted Windows runner labels: `self-hosted`, `Windows`,
  `Unity`.
- Workflow supports `UNITY_EXE` override or Unity Hub default path for
  `6000.5.0f1`.
- Added `CI_UNITY_COMPILE_SPEC.md`.
- Local Unity batchmode compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_03_a.log`.

M18-04 completed:

- Added `.github/workflows/unity-editmode.yml`.
- Workflow runs on push and pull request.
- Workflow uses self-hosted Windows runner labels: `self-hosted`, `Windows`,
  `Unity`.
- Workflow supports `UNITY_EXE` override or Unity Hub default path for
  `6000.5.0f1`.
- Workflow uploads EditMode log and XML result artifacts.
- Added `CI_UNITY_EDITMODE_SPEC.md`.
- Local Unity EditMode tests passed at `839/839`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_04_a.xml`.

M18-05 completed:

- Added `CoreRegressionSuiteReportBuilder`.
- Added required-category validation for RulesCore facade, legal action mask,
  event sourcing/replay, snapshot/rollback, hidden-state masking, resource
  ledger, and RuleSet profile gates.
- Added `CORE_REGRESSION_SUITE_SPEC.md`.
- Local Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_05_a.log`.
- Unity EditMode passed at `843/843`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_05_a.xml`.

M18-06 completed:

- Added `AbilityRegressionSuiteReportBuilder`.
- Added required-category validation for schema contract, Python validator,
  runtime registry, cost/target templates, effect/resource templates, modifier
  templates, fixture DSL/pack smoke, manual fallback bridge, and custom pack
  ability metadata.
- Added `ABILITY_REGRESSION_SUITE_SPEC.md`.
- Python unit tests passed `31/31`.
- Ability schema validator passed for the M12 structured pack.
- Local Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_06_a.log`.
- Unity EditMode passed at `847/847`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_06_a.xml`.

M18-07 completed:

- Added `MultiplayerPayloadNoLeakSuiteReportBuilder`.
- Added required-category validation for command envelopes, owner-private room
  state, public event masking, public reconnect recovery, spectator replay
  sync, trigger-check payloads, pending AUTO payloads, manual resolution
  decision payloads, and session-storage no-mutation.
- Added `MULTIPLAYER_PAYLOAD_NO_LEAK_SUITE_SPEC.md`.
- Local Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_07_a.log`.
- Unity EditMode passed at `851/851`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_07_a.xml`.

M18-08 completed:

- Added `WindowsBuildArtifactRunner`.
- Added `WINDOWS_BUILD_ARTIFACT_SPEC.md`.
- Updated `RELEASE_PLAN.md`.
- Built Windows artifact:
  `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`.
- Build log:
  `client/unity/VanguardThaiSim/work/windows_build_m18_08_a.log`.
- Local Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_08_a.log`.
- Unity EditMode passed at `851/851`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_08_a.xml`.

M18-09 completed:

- Added `AndroidBuildArtifactRunner`.
- Added `ANDROID_BUILD_ARTIFACT_SPEC.md`.
- Updated `RELEASE_PLAN.md`.
- Built Android artifact:
  `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk`.
- Build log:
  `client/unity/VanguardThaiSim/work/android_build_m18_09_b.log`.
- Local Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m18_09_b.log`.
- Unity EditMode passed at `851/851`:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m18_09_a.xml`.

M18-10 completed:

- Added `RELEASE_CANDIDATE_CHECKLIST.md`.
- Verified Windows/Android artifacts, build logs, Unity compile log, and
  EditMode XML paths.
- Checklist records known constraints and no-publish boundaries.
- No GitHub Release or artifact upload was performed.

## Work Rules

Always:

- Work in small milestones.
- Add or update a spec before implementing a new system.
- Add tests or verification for logic changes.
- Update docs after feature changes.
- Preserve existing user/generated data.
- Keep online transport separate from core game logic.
- Keep hidden information masked in player/bot/spectator views.
- Use concise/caveman-style communication only for chat, status, review, and
  handoff summaries; keep source-of-truth docs explicit.

Never:

- Do not rewrite large systems unless the current milestone explicitly requires it.
- Do not change stack without ADR.
- Do not replace JSON with MessagePack/Protobuf yet.
- Do not move to flat array/packed state in live `GameState` yet.
- Do not implement advanced bot/RL before the Correctness-First gates are stable.
- Do not auto-resolve arbitrary card text at runtime.
- Do not mutate `GameState` directly from UI or bot.
- Do not leak opponent hand/top-deck/private choices.
- Do not use probability as actual random outcome.
- Do not unblock commitment-only normal gameplay until public-event state application/reconnect paths exist.
- Do not compress specs, architecture decisions, acceptance criteria, or test
  plans so much that another AI must guess missing context.

## Correctness-First Gates

Advanced AI, ISMCTS, RL, packed simulation state, and heavy optimization wait until these are stable:

- Legal Action API and validation.
- Hidden-state views.
- Seeded live/replay/simulation RNG separation.
- Snapshot/rollback.
- TimingWindow enum and Phase/Timing Matrix.
- FormatProfile model.
- Pending resolution queue and manual decision model.
- Structured ability schema.
- Cost/target/effect resolver.
- Rule fixture DSL and scenario tests.

## Current Implementation Style

Preferred pattern:

1. Add or update a focused spec in `docs/`.
2. Add pure model/helper/formatter first.
3. Add EditMode tests.
4. Wire into PlayTable/session/transport only after the pure layer is stable.
5. Run Unity compile and EditMode tests for C# changes.
6. Update `docs/AI_CONTEXT_BRIEF.md`, `docs/IMPLEMENTATION_PLAN.md`, `docs/ROADMAP.md`, `docs/TESTING_STRATEGY.md`, and `docs/INDEX.md` if needed.

For docs-only changes:

- Unity tests are not required unless runtime code changed.

## Key Files To Open Only When Needed

Read these when you need deeper context:

- Current state: `docs/AI_CONTEXT_BRIEF.md`
- Current milestone queue: `docs/IMPLEMENTATION_PLAN.md`
- Product phases: `docs/ROADMAP.md`
- Hard boundaries: `docs/DO_NOT_DO.md`
- Short core checklist: `docs/CORE_DEVELOPMENT_GUARDRAILS.md`
- Full core/rule/format/timing reference: `docs/VANGUARD_CORE_RULE_ARCHITECTURE_REFERENCE.md`
- Architecture summary: `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`
- Testing expectations: `docs/TESTING_STRATEGY.md`
- Done criteria: `docs/DEFINITION_OF_DONE.md`
- Team communication token policy: `docs/AI_COMMUNICATION_TOKEN_POLICY.md`

Open subsystem specs only for the area you touch, for example:

- Decks: `docs/DECK_SYSTEM_SPEC.md`
- Core game: `docs/GAME_ENGINE_SPEC.md`
- Multiplayer: `docs/MULTIPLAYER_ROOM_SPEC.md`, `docs/PHOTON_REALTIME_TRANSPORT_SPEC.md`
- Pending auto ability: `docs/PENDING_AUTO_ABILITY_*.md`
- Trigger/check systems: `docs/TRIGGER_*.md`

## Verification Commands

Python/data changes:

```powershell
python tools\verification\verify_vanguard_th_pack.py
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite
python -m unittest discover -s tests -p "test_*.py"
```

Unity C# compile:

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
if (-not (Get-Process Unity -ErrorAction SilentlyContinue)) {
  Remove-Item -LiteralPath (Join-Path $projectPath "Library\ArtifactDB-lock") -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath (Join-Path $projectPath "Library\SourceAssetDB-lock") -Force -ErrorAction SilentlyContinue
}
$logPath = Join-Path $projectPath "work\unity_compile.log"
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile $logPath
Select-String -Path $logPath -Pattern "fatal error|error CS|Compilation failed|Scripts have compiler errors" -CaseSensitive:$false
```

Unity EditMode tests:

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
if (-not (Get-Process Unity -ErrorAction SilentlyContinue)) {
  Remove-Item -LiteralPath (Join-Path $projectPath "Library\ArtifactDB-lock") -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath (Join-Path $projectPath "Library\SourceAssetDB-lock") -Force -ErrorAction SilentlyContinue
}
$logPath = Join-Path $projectPath "work\unity_editmode_tests.log"
$resultsPath = Join-Path $projectPath "work\unity_editmode_results.xml"
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults $resultsPath -logFile $logPath
if (Test-Path -LiteralPath $resultsPath) {
  [xml]$xml = Get-Content -LiteralPath $resultsPath
  $run = $xml.SelectSingleNode("//test-run")
  "total=$($run.total) passed=$($run.passed) failed=$($run.failed) inconclusive=$($run.inconclusive) skipped=$($run.skipped) result=$($run.result)"
}
```

Unity sometimes leaves project-local lock files after batchmode. If no Unity process is running, remove only:

- `client/unity/VanguardThaiSim/Library/ArtifactDB-lock`
- `client/unity/VanguardThaiSim/Library/SourceAssetDB-lock`

## Immediate Handoff

If continuing from here, do this next:

1. Open `docs/IMPLEMENTATION_PLAN.md` section `M11-09`.
2. Read `GameStateViewFactory.cs`, `GameStateViewTests.cs`,
   `SNAPSHOT_ROLLBACK_VERIFIER_SPEC.md`, and hidden-state specs.
3. Add hidden-state hardening coverage for player, spectator, and bot-safe
   views.
4. Verify hidden card ids/source metadata do not leak.
5. Set next target to `M11-10`.

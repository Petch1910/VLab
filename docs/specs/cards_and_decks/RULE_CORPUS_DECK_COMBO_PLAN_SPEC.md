# Rule Corpus Driven Deck / Combo Plan Spec

## Status

`M34-02` imports and reviews the 2026-06-29 rule/development corpus.
`M34-03` adds Phase A1 archetype priority ranking.
`M35-A2` selects the first target slice: Classic Core / โนว่า เกรปเปอร์.
`M35-A3` adds minimal deck legality fixtures for that selected slice.
`M35-A4` reports Phase A ready for Phase B.
`M35-B1` adds selected-slice semantic vocabulary.
`M35-B2` extracts selected-slice advisory semantic tags.
`M35-B3` builds the selected-slice requirement/provider model.
`M35-B4` exports the selected-slice manual review queue.
`M35-E4` passes the bot integration gate while keeping runtime bot integration
disabled.
`M35-closeout` closes the Hybrid Vertical-Slice Strategy and selects `M36`.

The Hybrid Vertical-Slice Strategy is now closed. The active plan is `M36`
Human-review-assisted Deck Recipe Validation, which turns M35 advisory outputs
into reviewable deck recipe drafts before any runtime or bot work resumes.

This spec is planning-only. It does not change live runtime rules, card data,
Unity UI, bot decisions, or `GameState`.

## New Local Corpus

Copied source snapshot:

```text
outputs/research_2026_06_29_new_chat/
```

Important files:

- `vanguard_development/00_DEVELOPMENT_INDEX.md`
- `vanguard_development/rule_engine_spec.md`
- `vanguard_development/rule_taxonomy.json`
- `vanguard_development/implementation_checklist.md`
- `vanguard_development/field_layout_by_era.md`
- `vanguard_development/field_blueprints.json`
- `vanguard_development/source_references.md`
- `vanguard_rules_markdown/manifest.json`
- `vanguard_rules_markdown/mechanic_presence_matrix.md`
- `vanguard_effect_conditions_reference.md`
- `vanguard_rules_source_map.md`
- `vanguard_rules_study_notes.md`

The corpus references official Comprehensive Rules through `4.55`
(`SRC-RULES-455`, May 22, 2026) plus historical rule versions from `1.19`
through `4.55`.

## Key Finding

The existing M33/M34 tools can say:

- which clan/nation groups have enough card capacity to build a theoretical
  deck
- which card pairs look related by text/tag heuristics

They cannot yet prove:

- the deck is legal for a specific format
- the combo can execute in a real timing window
- costs and resources do not collide
- target/zone/board requirements are compatible
- continuous/replacement effects do not override each other
- the pair belongs in a runtime bot playbook

Therefore, `M33-04` playbook seed promotion must stay deferred until the
project has a rule-aware compatibility layer.

## Plan Change

Old next step:

```text
M34-02 -> review feasible groups -> promote high-confidence pair candidates.
```

Current vertical plan:

```text
Phase A -> foundation slice: choose one target slice and prove minimal legality
Phase B -> semantic slice: tag only that selected slice
Phase C -> compatibility slice: detect resource/timing/zone conflicts
Phase D -> deck skeleton + safe playbook seed for reviewed packages
Phase E -> scale out to more formats/eras only after the first slice works
```

## Milestones

### M34: Offline Deck Construction Possibility

- `M34-01`: Deck possibility analysis by clan/nation group. Done.
- `M34-02`: Rule corpus intake and plan refresh. Done.
- `M34-03`: Deck-feasible archetype priority v2 / Phase A1. Done.
  - Combine `outputs/deck_possibility/deck_possibility_summary.csv` with
    combo matrix outputs and new rule-complexity metadata.
  - Prioritize groups that are feasible, have enough local card data, and use
    lower-risk mechanics first.
  - Defer groups with missing set data, insufficient trigger/non-trigger
    capacity, or unsupported mechanic modules.

### Phase A: Foundation Slice

- `M35-A1`: Archetype priority ranking. Done as `M34-03`.
- `M35-A2`: First target slice selection + format policy + taxonomy gap report.
  Done.
  - Output: `outputs/target_slice/m35_a2_first_target_slice_report.json`
  - Output: `outputs/target_slice/m35_a2_first_target_slice_report.md`
- `M35-A3`: Minimal deck legality fixtures for selected slice. Next.
  Done.
  - Output:
    `outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.json`
  - Output:
    `outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.md`
- `M35-A4`: First-slice feasibility report refresh. Next.
  Done.
  - Output: `outputs/target_slice/m35_a4_first_slice_feasibility_refresh.json`
  - Output: `outputs/target_slice/m35_a4_first_slice_feasibility_refresh.md`

### Phase B: Semantic Slice

- `M35-B1`: Semantic vocabulary for selected slice. Done.
  - Output: `outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.json`
  - Output: `outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.md`
- `M35-B2`: Offline semantic extractor for selected slice. Next.
  Done.
  - Output: `outputs/target_slice/m35_b2_first_slice_semantic_tags.json`
  - Output: `outputs/target_slice/m35_b2_first_slice_semantic_tags.md`
- `M35-B3`: Requirement/provider model for selected slice. Done.
  - Output:
    `outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json`
  - Output:
    `outputs/target_slice/m35_b3_first_slice_requirement_provider_model.md`
- `M35-B4`: Manual review queue for unknown/low-confidence cards. Done.
  - Output: `outputs/target_slice/m35_b4_first_slice_manual_review_queue.json`
  - Output: `outputs/target_slice/m35_b4_first_slice_manual_review_queue.csv`
  - Output: `outputs/target_slice/m35_b4_first_slice_manual_review_queue.md`

### Phase C: Compatibility Slice

- `M35-C1`: Pair compatibility graph for selected slice. Done.
  - Output:
    `outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json`
  - Output:
    `outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.md`
- `M35-C2`: Resource conflict detector. Done.
  - Output:
    `outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json`
  - Output:
    `outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.md`
- `M35-C3`: Timing compatibility detector. Done.
  - Output:
    `outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.json`
  - Output:
    `outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.md`
- `M35-C4`: Zone/target compatibility detector. Done.
  - Output:
    `outputs/target_slice/m35_c4_first_slice_zone_target_detector.json`
  - Output:
    `outputs/target_slice/m35_c4_first_slice_zone_target_detector.md`
- `M35-C5`: Selected-slice compatibility output. Done.
  - Output:
    `outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json`
  - Output:
    `outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.md`

### Phase D: Deck Skeleton + Safe Playbook Seed

- `M35-D1`: Candidate package selection. Done.
  - Output:
    `outputs/target_slice/m35_d1_first_slice_candidate_packages.json`
  - Output:
    `outputs/target_slice/m35_d1_first_slice_candidate_packages.md`
- `M35-D2`: Deck skeleton ratio planner. Done.
  - Output:
    `outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json`
  - Output:
    `outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.md`
- `M35-D3`: Combo line explainer. Done.
  - Output:
    `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
  - Output:
    `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.md`
- `M35-D4`: Reviewed playbook seed export. Done.
  - Output:
    `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json`
  - Output:
    `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.md`

### Phase E: Scale Out

- `M35-E1`: Second slice selection. Done.
  - Output:
    `outputs/target_slice/m35_e1_second_target_slice_report.json`
  - Output:
    `outputs/target_slice/m35_e1_second_target_slice_report.md`
- `M35-E2`: Second-slice fixture/format readiness check. Done.
  - Output:
    `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
  - Output:
    `outputs/target_slice/m35_e2_second_slice_fixture_readiness.md`
- `M35-E3`: Generalize semantic/compatibility tools. Done.
  - Output:
    `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
  - Output:
    `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.md`
- `M35-E4`: Bot integration gate. Done.
  - Output: `outputs/target_slice/m35_e4_bot_integration_gate.json`
  - Output: `outputs/target_slice/m35_e4_bot_integration_gate.md`
  - Result: `1` reviewed future hint candidate, `1` blocked source, and
    runtime bot integration remains disabled.
- `M35-closeout`: Hybrid Vertical-Slice closeout / next queue selection. Done.
  - Output: `outputs/target_slice/m35_closeout_hybrid_vertical_slice.json`
  - Output: `outputs/target_slice/m35_closeout_hybrid_vertical_slice.md`
  - Result: M35 complete, runtime bot integration disabled, next queue is
    `M36`.

### M36: Human-review-assisted Deck Recipe Validation

- `M36-01`: First-slice review packet. Done.
  - Output: `outputs/target_slice/m36_01_first_slice_review_packet.json`
  - Output: `outputs/target_slice/m36_01_first_slice_review_packet.md`
  - Output: `outputs/target_slice/m36_01_first_slice_review_packet.csv`
  - Result: `31` review items, ready for M36-02.
- `M36-02`: Deck recipe draft model. Done.
  - Output: `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
  - Output: `outputs/target_slice/m36_02_deck_recipe_draft_model.md`
  - Result: `25` recipe drafts, `1` accepted-seed draft, `24`
    rejected-line drafts, `16` slot-gap drafts.
- `M36-03`: Deck recipe validator. Done.
  - Output: `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
  - Output: `outputs/target_slice/m36_03_deck_recipe_validation_report.md`
  - Result: `25` drafts validated, `0` runtime-ready recipes, `0`
    missing-card recipes, `0` copy-limit violations.
- `M36-04`: Combo-line to recipe consistency check. Done.
  - Output: `outputs/target_slice/m36_04_combo_recipe_consistency_report.json`
  - Output: `outputs/target_slice/m36_04_combo_recipe_consistency_report.md`
  - Result: `25` combo lines present in recipes, `0` missing combo-card
    checks, `0` promotable lines.
- `M36-05`: Second-slice readiness comparison. Done.
  - Output: `outputs/target_slice/m36_05_second_slice_readiness_comparison.json`
  - Output: `outputs/target_slice/m36_05_second_slice_readiness_comparison.md`
  - Result: second slice ready for future offline recipe work; broader
    runtime scale-out remains disabled.
- `M36-closeout`: Deck recipe validation closeout. Done.
  - Output: `outputs/target_slice/m36_closeout_deck_recipe_validation.json`
  - Output: `outputs/target_slice/m36_closeout_deck_recipe_validation.md`
  - Result: M36 closed with `0` runtime-ready recipes and `0` promotable
    combo lines; next queue is first-slice blocker repair.

### M37: First-slice Blocker Resolution and Recipe Repair

- `M37-01`: Accepted seed slot-gap completion candidates. Done.
  - Goal: suggest source-backed candidates for missing accepted-seed recipe
    slots without automatic runtime promotion.
  - Output: `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.json`
  - Output: `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.md`
  - Result: `18` trigger candidate cards and `5` complete advisory packages
    for accepted seed `recipe_003`.
- `M37-02`: Trigger package repair proposal. Done.
  - Goal: repair trigger-count mismatch candidates for the accepted seed
    recipe.
  - Output: `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
  - Output: `outputs/target_slice/m37_02_trigger_package_repair_proposal.md`
  - Result: recommended advisory package `m37_01_pkg_001` /
    `balanced_classic`; runtime promotion remains disabled.
- `M37-03`: Rejected-line support-gap triage. Done.
  - Goal: group blocked combo lines by unsupported semantic or review reason.
  - Output: `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json`
  - Output: `outputs/target_slice/m37_03_rejected_line_support_gap_triage.md`
  - Result: `24` rejected lines classified into `5` support-gap groups.
- `M37-04`: Manual semantic mapping candidates. Done.
  - Goal: create reviewable mappings for unsupported effects before recipe
    re-draft.
  - Output: `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json`
  - Output: `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.md`
  - Result: `5` non-executable mapping candidates from `49` triage line links.
- `M37-05`: Revised recipe validation rerun. Done.
  - Goal: re-run recipe validator and combo consistency after accepted repairs.
  - Output: `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`
  - Output: `outputs/target_slice/m37_05_revised_recipe_validation_rerun.md`
  - Result: accepted seed trigger blockers clear in memory, but human
    acceptance is still pending.
- `M37-closeout`: First runtime-ready recipe decision. Done.
  - Goal: decide whether a recipe can become a runtime/test fixture or remains
    advisory only.
  - Output:
    `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.json`
  - Output:
    `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.md`
  - Result: `recipe_003` remains advisory; next queue is `M38`.

### M38: Human Acceptance and Grade-profile Repair Gate

- `M38-01`: Accepted seed human review packet. Done.
  - Goal: export a concise review packet for `recipe_003` and the recommended
    trigger repair.
  - Output: `outputs/target_slice/m38_01_accepted_seed_human_review_packet.json`
  - Output: `outputs/target_slice/m38_01_accepted_seed_human_review_packet.md`
  - Output: `outputs/target_slice/m38_01_accepted_seed_human_review_packet.csv`
  - Result: review packet exported but human acceptance is not recorded.
- `M38-02`: Grade profile repair candidates. Done.
  - Goal: propose reviewable grade-profile adjustments without mutating runtime
    decks.
  - Output: `outputs/target_slice/m38_02_grade_profile_repair_candidates.json`
  - Output: `outputs/target_slice/m38_02_grade_profile_repair_candidates.md`
  - Result: `2` complete substitution-preview candidates reach
    `G0=17/G1=14/G2=11/G3=8`.
- `M38-03`: Human-accepted recipe artifact. Done.
  - Goal: record explicit acceptance or rejection of the repaired recipe.
  - Output: `outputs/target_slice/m38_03_human_accepted_recipe_artifact.json`
  - Output: `outputs/target_slice/m38_03_human_accepted_recipe_artifact.md`
  - Result: accepted `m38_02_grade_pkg_001` and `m37_01_pkg_001`; accepted
    artifact reaches `50` cards, `16` triggers, `G0=17/G1=14/G2=11/G3=8`,
    and `0` blockers while keeping runtime promotion disabled.
- `M38-04`: Runtime fixture promotion gate. Done.
  - Goal: promote only if validation, consistency, grade review, and human
    acceptance all pass.
  - Output: `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.json`
  - Output: `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.md`
  - Fixture:
    `outputs/target_slice/runtime_fixtures/recipe_003_classic_core_nova_grappler_m38_04.json`
  - Result: all `5` gate checks passed and fixture scope is offline
    runtime/test only.
- `M38-closeout`: First runtime fixture closeout. Done.
  - Goal: decide whether the first recipe enters runtime/test-fixture scope or
    remains advisory.
  - Output: `outputs/target_slice/m38_closeout_first_runtime_fixture.json`
  - Output: `outputs/target_slice/m38_closeout_first_runtime_fixture.md`
  - Result: `recipe_003` enters offline fixture scope only and next queue is
    `M39`.

### M39: Fixture Consumption and Second-Slice Scale Gate

- `M39-01`: Offline fixture schema validator. Done.
  - Goal: validate runtime fixture artifacts independently from the M38
    generator.
  - Output: `outputs/target_slice/m39_01_offline_fixture_schema_validation.json`
  - Output: `outputs/target_slice/m39_01_offline_fixture_schema_validation.md`
  - Result: fixture schema valid with `0` blockers and counts recomputed from
    SQLite.
- `M39-02`: Fixture-to-deck text exporter. Done.
  - Goal: export the fixture as reviewable count-line deck text without adding
    it to saved decks.
  - Output: `outputs/target_slice/m39_02_fixture_deck_text_export.txt`
  - Output: `outputs/target_slice/m39_02_fixture_deck_text_export.json`
  - Output: `outputs/target_slice/m39_02_fixture_deck_text_export.md`
  - Result: reviewable count-line deck text generated with `17` importable
    card lines, `50` total cards, and no saved deck/UI/bot/GameState mutation.
- `M39-03`: Headless fixture load smoke. Done.
  - Goal: load the fixture through offline tooling/headless paths without UI or
    bot mutation.
  - Output: `outputs/target_slice/m39_03_headless_fixture_deck_code.txt`
  - Output: `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
  - Output: `outputs/target_slice/m39_03_headless_fixture_load_smoke.md`
  - Output: `outputs/target_slice/m39_03_headless_fixture_unity_result.json`
  - Output: `outputs/target_slice/m39_03_headless_fixture_unity_replay.json`
  - Result: generated `VGTH1.` deck code was accepted by Unity headless with
    `deck_source=deck_code`, `actions_executed=4`, and `event_count=4`.
- `M39-04`: Second-slice recipe scale decision. Done.
  - Goal: decide whether Oracle Think Tank moves into the same recipe repair
    pipeline.
  - Output: `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.json`
  - Output: `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.md`
  - Result: offline recipe pipeline is allowed for Oracle Think Tank only;
    saved deck/UI publication, runtime promotion, and bot/playbook promotion
    remain blocked.

### M40: Second-slice Offline Recipe Pipeline

- `M40-01`: Second-slice review packet. Done.
  - Goal: export Oracle Think Tank candidate edges, manual-review cards, and
    fixture notes for review.
  - Output: `outputs/target_slice/m40_01_second_slice_review_packet.json`
  - Output: `outputs/target_slice/m40_01_second_slice_review_packet.md`
  - Output: `outputs/target_slice/m40_01_second_slice_review_packet.csv`
  - Result: `6` fixture notes, `7` manual-review cards, `259` candidate
    edges, `272` total review items, and `ready_for_m40_02=true`.
- `M40-02`: Second-slice recipe draft model. Done.
  - Output: `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
  - Output: `outputs/target_slice/m40_02_second_slice_recipe_draft_model.md`
  - Result: `25` pair-anchored, fixture-scaffolded advisory recipe drafts,
    all quantity-complete at `50` cards with `16` triggers.
- `M40-03`: Second-slice recipe validator. Done.
  - Output: `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`
  - Output: `outputs/target_slice/m40_03_second_slice_recipe_validation_report.md`
  - Result: `25` drafts validated, `0` runtime-ready recipes, `0`
    missing-card/copy-limit/slot-gap/trigger-count blockers, and `25`
    manual-review overlap blockers.
- `M40-04`: Second-slice combo-to-recipe consistency. Done.
  - Output:
    `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`
  - Output:
    `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.md`
  - Result: `25` consistency checks, candidate pair cards present in all
    drafts, `0` missing pair-card checks, `25` recipe-level manual-review
    dependencies, and `0` promotion-allowed checks.
- `M40-05`: Second-slice blocker repair candidates. Done.
  - Output:
    `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.json`
  - Output:
    `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.md`
  - Result: `25` repair items, `25` grade-profile repair candidates, `25`
    grade packages that clear manual overlap, and `25` items ready for human
    repair review. Runtime promotion remains blocked.
- `M40-closeout`: Second-slice runtime readiness decision. Done.
  - Output:
    `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.json`
  - Output:
    `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.md`
  - Result: M40 is complete, but Oracle Think Tank has `0` runtime-ready
    recipes and `0` promotion-allowed checks. `25` repair candidates are ready
    for human review. Saved deck/UI/runtime/bot promotion remains blocked.

### M41: Second-slice Human Repair Review Gate

- `M41-01`: Second-slice human repair review packet. Done.
  - Goal: export a concise packet for human/team review of M40-05 repair
    packages.
  - Output:
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.json`
  - Output:
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.md`
  - Output:
    `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.csv`
  - Result: `25` repair candidates exported for review. Packet is not
    acceptance and does not enable runtime promotion.
- `M41-02`: Second-slice human-accepted repair artifact. Done.
  - Goal: record explicit acceptance or rejection of one repaired Oracle Think
    Tank recipe.
  - Output:
    `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
  - Output:
    `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.md`
  - Result: accepted `m40_recipe_001` /
    `m40_recipe_001_grade_profile_pkg_001` for validation rerun. This does not
    declare the recipe valid or enable runtime promotion.
- `M41-03`: Second-slice repaired recipe validation rerun. Done.
  - Goal: apply accepted repair in memory and rerun count, trigger, grade,
    copy-limit, clan, and manual-overlap validation.
  - Output:
    `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.json`
  - Output:
    `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.md`
  - Result: validation failed with `trigger_count_mismatch`; trigger count is
    `2/16`. Grade profile and manual-review overlap are cleared, but M41-04 is
    blocked.
- `M41-repair`: Second-slice trigger/profile repair loop. Done.
  - Goal: repair the accepted recipe's trigger profile before promotion gate
    work.
  - Output:
    `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.json`
  - Output:
    `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.md`
  - Result: `3` complete trigger/profile candidates. Balanced package restores
    trigger count to `16/16` and keeps target grade counts.
- `M41-repair-accept`: Second-slice trigger repair acceptance artifact. Done.
  - Goal: record acceptance of one trigger/profile repair package before
    rerunning validation.
  - Result: accepted `m41_repair_pkg_001` /
    `balanced_classic_trigger_restore`; still no runtime promotion.
- `M41-repair-validate`: Second-slice repaired recipe validation after trigger
  repair. Done.
  - Goal: rerun validation after applying the accepted trigger repair.
  - Result: `validator_passed`, blockers `0`, main deck `50`, trigger count
    `16`, grade profile `G0=17/G1=14/G2=11/G3=8`, and ready for `M41-04`.
- `M41-04`: Second-slice runtime fixture promotion gate. Done.
  - Goal: promote only if repaired validation, consistency, and human
    acceptance all pass.
- `M41-closeout`: Second-slice fixture closeout. Done.
  - Goal: decide whether Oracle Think Tank enters offline fixture scope or
    remains advisory.
  - Result: Oracle Think Tank enters offline runtime/test fixture scope only and
    next queue is `M42`.
- `M42-01`: Second fixture schema validator. Done.
  - Goal: validate the Oracle Think Tank runtime fixture independently from the
    M41 generator.
- `M42-02`: Second fixture deck text exporter. Done.
- `M42-03`: Second fixture headless load smoke. Done.
- `M42-04`: Multi-fixture scale decision. Done.
  - Result: two fixtures passed offline/Unity smoke and third-slice offline
    pipeline is allowed.
- `M43-01`: Third target slice selection. Done.
- `M43-02`: Third-slice fixture/format readiness. Done.
  - Result: `127` source-backed cards, grade 0-3 coverage, trigger capacity
    `84`, and `requires_third_slice_fixture_scaffold`.
- `M43-03`: Third-slice semantic/compatibility probe. Done.
  - Result: `127` semantic cards, `61` manual-review cards, `4835` pair graph
    edges, and `109` candidate edges.
- `M43-04`: Third-slice recipe pipeline entry gate. Done.
  - Result: blockers `0`, offline M44 pipeline allowed, and fixture scaffold
    required before recipe validation.
- `M44-01`: Third-slice fixture scaffold. Done.
  - Result: source-backed fixture scaffold, blockers `0`, ready for `M44-02`.
- `M44-02`: Third-slice review packet. Done.
  - Result: `1` fixture scaffold item, `61` manual-review cards, `109`
    candidate edges, and `171` total review items.
- `M44-03`: Third-slice recipe draft model. Done.
  - Result: `25` quantity-complete advisory drafts, `50` cards per draft,
    `16` triggers per draft, and `25` manual-review overlap blockers.
- `M44-04`: Third-slice recipe validator. Done.
  - Result: `25` drafts validated, `0` missing-card/copy-limit/slot-gap/
    trigger-count blockers, `25` manual-review overlap blockers, and `0`
    runtime-ready recipes.
- `M44-05`: Third-slice combo-to-recipe consistency. Done.
  - Result: `25` consistency checks, candidate pair cards present in all
    drafts, `0` missing pair-card checks, `25` recipe-level manual-review
    dependencies, and `0` promotion-allowed checks.
- `M44-06`: Third-slice blocker repair candidates. Done.
  - Result: `25` repair items, `25` complete manual repair packages, `25`
    complete grade-profile repair packages, and runtime promotion disabled.
- `M44-closeout`: Third-slice runtime readiness decision. Done.
  - Result: M44 complete, runtime-ready recipe unavailable, human repair review
    allowed, and next queue `M45`.
- `M45-01`: Third-slice human repair review packet. Done.
  - Result: `25` review items, `25` complete manual repair packages, `25`
    complete grade-profile candidates, runtime promotion disabled, and
    `ready_for_m45_02=True`.
- `M45-02`: Third-slice human-accepted repair artifact. Done.
  - Result: accepted `m45_01_m44_recipe_001_repair_review`, detected `2`
    source grade-package conflicts after manual substitution, recomputed the
    combined grade repair, produced a `50`-card preview, and
    `ready_for_m45_03=True`.
- `M45-03`: Third-slice repaired recipe validation rerun. Done.
  - Result: `validator_passed=1`, `runtime_ready=1`, no missing-card/
    copy-limit/slot-gap/trigger-count/manual-overlap/grade-profile issues, and
    `ready_for_m45_04=True`.
- `M45-04`: Third-slice runtime fixture promotion gate. Done.
  - Result: `promotion_allowed=True`, `7/7` gate checks passed, offline
    runtime/test fixture artifact created, and no saved-deck/UI/bot/GameState
    mutation.
- `M45-closeout`: Third-slice fixture closeout. Done.
  - Result: `m45_complete=True`, third runtime fixture available, and next
    queue `M46`.
- `M46-01`: Third fixture schema validator. Done.
  - Result: `schema_valid=True`, blockers `0`, main deck count `50`, trigger
    profile `Critical=4 / Draw=4 / Heal=4 / Stand=4`, grade profile
    `G0=17 / G1=14 / G2=11 / G3=8`, and no runtime/UI/saved-deck/bot/GameState
    mutation.
- `M46-02`: Third fixture deck text exporter. Done.
  - Result: `export_ready=True`, blockers `0`, `15` exported card lines,
    review-only count-line deck text generated, and no runtime/UI/saved-deck/
    bot/GameState mutation.
- `M46-03`: Third fixture headless load smoke. Done.
  - Result: `offline_load_ready=True`, `unity_headless_smoke_passed=True`,
    `deck_source=deck_code`, `actions_executed=4`, `event_count=4`, blockers
    `0`, and no runtime/UI/saved-deck/bot/GameState mutation.
- `M46-04`: Three-fixture scale decision. Done.
  - Result: `ready_for_m47=True`, `passed_fixture_count=3`,
    `failed_fixture_count=0`, `candidate_count=5`, fourth-slice offline
    pipeline allowed, and no runtime/UI/saved-deck/bot/GameState mutation.
- `M47-01`: Fourth target slice selection. Done.
  - Result: selected `รอยัล พาลาดิน`, era preset `g_series_first`, offline
    analysis only, and no recipe/runtime/UI/saved-deck/bot/GameState mutation.
- `M47-02`: Fourth-slice fixture/format readiness. Done.
  - Result: source-backed card count `71`, trigger gap `Heal`,
    `all_fixture_expectations_met=False`, `repair_required=True`, and no
    recipe/runtime/UI/saved-deck/bot/GameState mutation.
- `M47-repair`: Fourth-slice readiness blocker repair. Done.
  - Result: same-group Heal triggers exist outside selected scope, recommended
    action `review_same_group_source_expansion`, and no card data/runtime/UI/
    saved-deck/bot/GameState mutation.
- `M47-repair-expand-scope`: Same-group source expansion review. Done.
  - Result: recommended `g_era_heal_expansion`, expanded source count `190`,
    trigger gaps cleared, expansion not applied yet, and no card data/runtime/
    UI/saved-deck/bot/GameState mutation.
- `M47-repair-apply-scope`: Apply reviewed source scope expansion. Done.
  - Result: applied `g_era_heal_expansion` to an offline fixture pipeline
    scope artifact, source card count `190`, blockers `0`, and no card data/
    runtime/UI/saved-deck/bot/GameState mutation.
- `M47-03`: Fourth-slice semantic/compatibility probe. Done.
  - Result: semantic cards `190`, pair graph edges `14150`, candidate edges
    `785`, all stage readiness flags passed, and no card data/runtime/UI/
    saved-deck/bot/GameState mutation.
- `M47-04`: Fourth-slice recipe pipeline entry gate. Done.
  - Result: offline M48 recipe pipeline allowed, blockers `0`, fixture scaffold
    required, and no card data/recipe/runtime/UI/saved-deck/bot/GameState
    mutation.
- `M48-01`: Fourth-slice fixture scaffold. Done.
  - Result: scaffold ready, blockers `0`, Grade 4/G Zone deferred as advisory/
    manual-review only, and no card data/recipe/runtime/UI/saved-deck/bot/
    GameState mutation.
- `M48-02`: Fourth-slice review packet. Done.
  - Result: fixture scaffold items `1`, manual-review cards `15`, candidate
    edges `785`, total review items `801`, and no card data/recipe/runtime/UI/
    saved-deck/bot/GameState mutation.
- `M48-03`: Fourth-slice recipe draft model. Done.
  - Result: recipe drafts `25`, quantity-complete drafts `25`, skipped trigger/
    G4/missing candidate edges `35`, and no card data/runtime/UI/saved-deck/
    bot/GameState mutation.
- `M48-04`: Fourth-slice recipe validator. Done.
  - Result: validated drafts `25`, runtime-ready recipes `0`, manual-review
    blocked recipes `25`, Grade 4 main-deck violations `0`, and no card data/
    runtime/UI/saved-deck/bot/GameState mutation.
- `M48-05`: Fourth-slice combo-to-recipe consistency. Done.
  - Result: consistency checks `25`, pair cards present `25`, promotion allowed
    `0`, G Zone deferred checks `25`, and no card data/runtime/UI/saved-deck/
    bot/GameState mutation.
- `M48-06`: Fourth-slice blocker repair candidates. Done.
  - Result: recipes reviewed `25`, complete manual repair candidates `25`,
    complete grade-profile candidates `24`, G Zone deferred recipes `25`,
    unexpected structural blockers `0`, and no card data/runtime/UI/saved-deck/
    bot/GameState mutation.
- `M48-closeout`: Fourth-slice runtime readiness decision. Done.
  - Result: M48 complete `true`, runtime-ready recipe available `false`,
    human/G-Zone review allowed `true`, G Zone deferred recipes `25`, next queue
    `M49`, and no card data/runtime/UI/saved-deck/bot/GameState mutation.
- `M49-01`: Fourth-slice human repair review packet. Done.
  - Result: review items `25`, complete manual repair packages `25`, complete
    grade-profile candidates `24`, G Zone decision items `25`,
    ready_for_m49_02 `true`, and no card data/runtime/UI/saved-deck/bot/
    GameState mutation.
- `M49-02`: Fourth-slice G Zone support decision. Done.
  - Result: selected `main_deck_only_for_current_windows_fixture`, decision
    items `25`, main-deck-only validation allowed `true`, G Zone runtime
    `false`, Stride runtime `false`, runtime promotion `false`,
    ready_for_m49_03 `true`, and no card data/runtime/UI/saved-deck/bot/
    GameState mutation.
- `M49-03`: Fourth-slice human-accepted repair artifact. Done.
  - Result: accepted `m48_recipe_001`, selected G Zone option
    `main_deck_only_for_current_windows_fixture`, repaired main deck `50`,
    grade profile `17/14/11/8`, repair issues `0`, declares recipe valid
    `false`, runtime promotion `false`, ready_for_m49_04 `true`, and no card
    data/runtime/UI/saved-deck/bot/GameState mutation.
- `M49-04`: Fourth-slice repaired recipe validation rerun. Done.
  - Result: accepted `m48_recipe_001`, validator passed `1`, runtime-ready
    recipes `1`, issue_counts `{}`, G Zone runtime `false`, Stride runtime
    `false`, runtime fixture created `false`, runtime promotion `false`,
    ready_for_m49_05 `true`, and no card data/runtime/UI/saved-deck/bot/
    GameState mutation.
- `M49-05`: Fourth-slice runtime fixture gate. Done.
  - Result: promotion allowed `true`, passed checks `8`, fixture created
    `true`, G Zone runtime `false`, Stride runtime `false`, saved-deck/UI/
    bot/GameState mutation disabled, and ready_for_m49_closeout `true`.
- `M49-closeout`: Fourth-slice fixture closeout. Done.
  - Result: M49 complete `true`, fourth runtime fixture available `true`, next
    queue `M50`, G Zone runtime `false`, Stride runtime `false`, saved-deck/UI/
    bot/GameState mutation disabled, and ready_for_next_queue `true`.
- `M50-01`: Fourth fixture schema validator. Done.
  - Result: schema valid `true`, blockers `0`, main deck `50`, unique cards
    `14`, trigger profile `4/4/4/4`, grade profile `17/14/11/8`, G Zone
    runtime `false`, Stride runtime `false`, and ready_for_m50_02 `true`.
- `M50-02`: Fourth fixture deck text exporter. Done.
  - Result: export ready `true`, blockers `0`, main deck `50`, exported card
    lines `14`, G section comment-only, and ready_for_m50_03 `true`.
- `M50-03`: Fourth fixture headless load smoke. Done.
  - Result: offline load ready `true`, deck code created `true`, Unity
    headless accepted `deck_source=deck_code`, actions/events `4/4`, G Zone
    count `0`, and ready_for_m50_04 `true`.
- `M50-04`: Four-fixture scale decision. Done.
  - Result: fixture evidence `4`, passed fixtures `4`, failed fixtures `0`,
    candidates `5`, fifth-slice offline pipeline allowed `true`, G Zone runtime
    `false`, Stride runtime `false`, and ready_for_m51 `true`.
- `M51-01`: Fifth target slice selection. Done.
  - Result: selected `โกลด์ พาลาดิน`, era preset
    `link_joker_legion_mate`, candidate count `5`, ready_for_m51_02 `true`,
    G Zone runtime `false`, Stride runtime `false`, and runtime/UI/saved-deck/
    bot/GameState mutation disabled.
- `M51-02`: Fifth-slice fixture/format readiness. Done.
  - Result: source_card_count `106`, trigger capacity `36`,
    non-trigger capacity `388`, trigger gaps `[]`, all fixture expectations
    met `true`, semantic probe ready `true`, ready_for_m51_03 `true`, and
    runtime/UI/saved-deck/bot/GameState mutation disabled.
- `M51-03`: Fifth-slice semantic/compatibility probe. Done.
  - Result: semantic cards `106`, manual-review cards `4`, pair graph edges
    `3075`, candidate edges `142`, all stage readiness passed `true`,
    ready_for_m51_04 `true`, and runtime/UI/saved-deck/bot/GameState mutation
    disabled.
- `M51-04`: Fifth-slice recipe pipeline entry gate. Done.
  - Result: offline recipe pipeline allowed `true`, blockers `0`, fixture
    scaffold required `true`, ready_for_m52 `true`, and runtime/UI/saved-deck/
    bot/GameState mutation disabled.
- `M52-01`: Fifth-slice fixture scaffold. Done.
  - Result: source-backed cards `106`, trigger capacity `36`, candidate edges
    `142`, scaffold_ready `true`, blockers `0`, ready_for_m52_02 `true`, and
    runtime/UI/saved-deck/bot/GameState mutation disabled.
- `M52-02`: Fifth-slice review packet. Done.
  - Result: fixture scaffold items `1`, manual-review cards `4`, candidate
    edges `142`, total review items `147`, ready_for_m52_03 `true`, and
    runtime/UI/saved-deck/bot/GameState mutation disabled.
- `M52-03`: Fifth-slice recipe draft model. Done.
  - Result: recipe drafts `25`, quantity-complete recipes `25`,
    trigger/missing skipped edges `0`, manual-overlap recipes `0`,
    ready_for_m52_04 `true`, and runtime/UI/saved-deck/bot/GameState mutation
    disabled.
- `M52-04`: Fifth-slice recipe validator. Done.
  - Result: recipes validated `25`, runtime-ready recipes `0`,
    validator-passed pending human selection `25`, invalid drafts `0`,
    missing/copy/slot/trigger/manual-overlap blockers `0`,
    grade-profile review recipes `25`, ready_for_m52_05 `true`, and runtime/
    UI/saved-deck/bot/GameState mutation disabled.
- `M52-05`: Fifth-slice combo-to-recipe consistency. Done.
  - Result: consistency checks `25`, pair cards present `25`, missing
    pair-card checks `0`, promotion allowed `0`, status
    `consistent_pending_human_selection=25`, ready_for_m52_06 `true`, and
    runtime/UI/saved-deck/bot/GameState mutation disabled.
- `M52-06`: Fifth-slice blocker repair candidates. Done.
  - Result: repair candidates `25`, complete grade-profile candidates `25`,
    human selection required `25`, unexpected structural blockers `0`,
    ready_for_m52_closeout `true`, and runtime/UI/saved-deck/bot/GameState
    mutation disabled.
- `M52-closeout`: Fifth-slice runtime readiness decision. Done.
  - Result: M52 complete `true`, runtime-ready recipe available `false`,
    human selection review allowed `true`, next queue `M53`, and runtime/UI/
    saved-deck/bot/GameState mutation disabled.
- `M53-01`: Fifth-slice human repair review packet. Done.
  - Result: review items `25`, ready for human repair review `25`, complete
    grade-profile candidates `25`, human selection required `25`,
    ready_for_m53_02 `true`, and runtime/UI/saved-deck/bot/GameState mutation
    disabled.
- `M53-02`: Fifth-slice human-selected recipe artifact. Done.
  - Result: selected review item `m53_01_m52_recipe_001_repair_review`,
    selected recipe `m52_recipe_001`, selected grade package
    `m52_recipe_001_grade_profile_pkg_001`, ready_for_m53_03 `true`, and
    runtime promotion disabled.
- `M53-03`: Fifth-slice human-accepted repair artifact. Done.
  - Result: human selection and acceptance recorded, repaired main deck count
    `50`, repair application issues `0`, ready_for_m53_04 `true`, and recipe
    validity/runtime promotion still not declared.
- `M53-04`: Fifth-slice repaired recipe validation rerun. Done.
  - Result: validation `validator_passed`, consistency
    `consistent_validator_passed`, runtime-ready recipes `1`, blockers `0`,
    ready_for_m53_05 `true`, and validation/consistency rerun is in-memory only.
- `M53-05`: Fifth-slice runtime fixture promotion gate. Done.
  - Result: promotion_allowed `true`, passed checks `5`, failed checks `0`,
    fixture created at
    `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`,
    and saved-deck/UI/bot/GameState mutation disabled.
- `M53-closeout`: Fifth-slice fixture closeout. Done.
  - Result: M53 complete `true`, fifth runtime fixture available `true`, next
    queue `M54`, saved-deck/UI/bot/GameState mutation disabled, targeted M53
    tests `35/35`, and full Python unittest discovery `1000/1000`.
- `M54-01`: Fifth fixture schema validator. Done.
  - Result: schema valid `true`, blockers `0`, main deck `50`, unique cards
    `16`, trigger profile `4/4/4/4`, grade profile `17/14/11/8`,
    ready_for_m54_02 `true`, targeted tests `8/8`, full Python unittest
    discovery `1008/1008`, and saved-deck/UI/bot/GameState mutation disabled.
- `M54-02`: Fifth fixture deck text exporter. Done.
  - Result: export_ready `true`, blockers `0`, main deck `50`, exported card
    lines `16`, review-only deck text generated at
    `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt`,
    ready_for_m54_03 `true`, targeted tests `7/7`, full Python unittest
    discovery `1015/1015`, and saved-deck/UI/bot/GameState mutation disabled.
- `M54-03`: Fifth fixture headless load smoke. Done.
  - Result: offline load ready `true`, deck code created `true`, Unity
    headless accepted `true`, deck source `deck_code`, actions/events `4/4`,
    blockers `0`, main deck `50`, unique cards `16`, G Zone `0`,
    ready_for_m54_04 `true`, targeted tests `9/9`, full Python unittest
    discovery `1024/1024`, and saved-deck/UI/bot/GameState mutation disabled.
- `M54-04`: Multi-fixture scale decision. Done.
  - Result: five fixture evidence records reviewed, passed fixtures `5`,
    failed fixtures `0`, candidate count `5`, sixth-slice offline pipeline
    allowed `true`, targeted tests `8/8`, full Python unittest discovery
    `1032/1032`, and runtime/UI/bot/GameState mutation disabled.
- `M55-01`: Sixth target slice selection. Done.
  - Result: selected group `ชาโดว์ พาลาดิน`, era `g_next_z`, rank `4`,
    candidate count `5`, ready_for_m55_02 `true`, targeted tests `7/7`, full
    Python unittest discovery `1039/1039`, and runtime/UI/bot/GameState
    mutation disabled.
- `M55-02`: Sixth-slice fixture/format readiness. Done.
  - Result: source cards `77`, grade profile `19/20/16/11/11`, trigger profile
    `Critical=4/Draw=4/Heal=2/Stand=2`, trigger capacity `48`, non-trigger
    capacity `260`, semantic probe ready `true`, targeted tests `8/8`, full
    Python unittest discovery `1047/1047`, and runtime/UI/bot/GameState
    mutation disabled.
- `M55-03`: Sixth-slice semantic/compatibility probe. Done.
  - Result: semantic cards `77`, manual-review cards `11`, pair graph edges
    `2069`, candidate edges `70`, all stage readiness `true`, targeted tests
    `8/8`, full Python unittest discovery `1055/1055`, and runtime/UI/bot/
    GameState mutation disabled.
- `M55-04`: Sixth-slice recipe pipeline entry gate. Done.
  - Result: offline recipe pipeline allowed `true`, blockers `0`,
    ready_for_m56 `true`, source cards `77`, semantic cards `77`, candidate
    edges `70`, targeted tests `9/9`, full Python unittest discovery
    `1064/1064`, and runtime/UI/bot/GameState mutation disabled.
- `M56-01`: Sixth-slice fixture scaffold. Done.
  - Result: scaffold_ready `true`, blockers `0`, source cards `77`, Grade 4
    cards advisory only until G Zone support, targeted tests `9/9`, full
    Python unittest discovery `1073/1073`, and runtime/UI/bot/GameState
    mutation disabled.
- `M56-02`: Sixth-slice review packet. Done.
  - Result: review items `82`, manual-review cards `11`, candidate edges
    `70`, ready_for_m56_03 `true`, targeted tests `8/8`, full Python unittest
    discovery `1081/1081`, and runtime/UI/bot/GameState mutation disabled.
- `M56-03`: Sixth-slice recipe draft model. Done.
  - Result: recipe drafts `12`, quantity-complete recipes `12`, skipped
    trigger/Grade 4/missing edges `58`, manual-overlap recipes `12`, targeted
    tests `9/9`, full Python unittest discovery `1090/1090`, and runtime/UI/
    bot/GameState mutation disabled.
- `M56-04`: Sixth-slice recipe validator. Done.
  - Validate count, trigger, grade, identity, copy limits, missing cards,
    manual-review blockers, and fixture scaffold constraints.
  - Result: validated drafts `12`, runtime-ready recipes `0`,
    manual-review blocked recipes `12`, missing/copy/slot/trigger/Grade 4
    blockers `0`, targeted tests `7/7`, full Python unittest discovery
    `1097/1097`, and runtime/UI/bot/GameState mutation disabled.
- `M56-05`: Sixth-slice combo-to-recipe consistency. Done.
  - Verify M56-04 validated drafts still match M56-02 candidate edges, M56-03
    pair anchors, review blockers, and no-runtime-promotion boundaries.
  - Result: consistency checks `12`, pair cards present `12`, missing
    pair-card checks `0`, promotion allowed `0`, recipe manual dependencies
    `12`, G Zone deferred `12`, targeted tests `6/6`, full Python unittest
    discovery `1103/1103`, and runtime/UI/bot/GameState mutation disabled.
- `M56-06`: Sixth-slice blocker repair candidates. Done.
  - Convert M56-04/M56-05 blockers into human-review repair options while
    keeping all outputs advisory and offline-only.
  - Result: repair items `12`, manual repair complete `12`, grade repair
    complete `12`, G Zone deferred `12`, ready for human repair review `12`,
    targeted tests `8/8`, full Python unittest discovery `1111/1111`, and
    runtime/UI/bot/GameState mutation disabled.
- `M56-closeout`: Sixth-slice runtime readiness decision. Done.
  - Summarize M55/M56 evidence and keep runtime promotion blocked unless human
    review and G Zone/Stride gates are explicitly cleared.
  - Result: M56 complete `true`, runtime-ready recipe available `false`, next
    queue `M57`, manual review blocked `12`, G Zone deferred `12`, targeted
    tests `9/9`, full Python unittest discovery `1120/1120`, and runtime/UI/
    bot/GameState mutation disabled.
- `M57-01`: Sixth-slice human repair review packet. Done.
  - Export a bounded human/team review packet for M56-06 repair packages and
    M56-closeout decision blockers.
  - Result: review items `12`, complete manual repairs `12`, complete grade
    repairs `12`, G Zone deferred items `12`, targeted tests `10/10`, full
    Python unittest discovery `1130/1130`, and runtime/UI/bot/GameState
    mutation disabled.
- `M57-02`: Sixth-slice human-selected recipe artifact. Next.
  - Record exactly one selected sixth-slice recipe id from M57-01 without
    mutating M56 drafts.

## Non-Goals

- No UI redesign in this plan.
- No Android/mobile/release work.
- No copying external assets/code/data.
- No live card text parser.
- No automatic bot playbook promotion from heuristic pairs.
- No full official legality claim until source-backed fixtures exist.

## Verification

For planning/docs-only work:

- Verify copied corpus files exist.
- Update `AI_QUICK_START`, `IMPLEMENTATION_PLAN`, `ROADMAP`, `INDEX`, and
  relevant history closeout.
- Python/Unity tests are not required unless code or data tooling changes.

For later code/data milestones:

- Python unit tests for offline tools.
- Unity compile/EditMode for C#/Unity changes.
- Source metadata attached to rule/deck fixtures.

# Eighth-Slice Human Repair Review Packet Spec

Milestone: `M65-01`

## Purpose

`M65-01` converts `M64-06` repair candidates and the `M64-closeout`
decision into a compact human/team review packet. The packet lets a reviewer
choose one eighth-slice recipe for later selection, grade-repair acceptance,
and Lock plus Legion decision work.

This milestone does not record recipe selection, does not record grade repair
acceptance, does not record a Lock runtime decision, does not record a Legion
runtime decision, and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m64_closeout_eighth_slice_runtime_readiness.json`
- `outputs/target_slice/m64_06_eighth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`

Tests may pass in-memory M64 reports until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m65_01_eighth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m65_01_eighth_slice_human_repair_review_packet.md`
- `outputs/target_slice/m65_01_eighth_slice_human_repair_review_packet.csv`

## Review Rules

- Include all repair candidates that are ready for human repair review.
- Preserve pair context from the recipe draft.
- Preserve human-selection package context from M64-06.
- Preserve grade-profile additions/removals from M64-06.
- Preserve Lock deferred system-work context from M64-06.
- Preserve Legion deferred system-work context from M64-06.
- Provide decision options only; do not choose an option automatically.
- Mark `ready_for_m65_02=true` only if `M64-closeout` allows human selection
  review and every review item is ready.

## Runtime Boundary

This milestone must not:

- modify M64 recipe drafts
- record human selection
- record human acceptance
- record Lock decision
- record Legion decision
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable Lock runtime
- enable Legion runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report exports `25` review items. All `25` are ready for
human repair review, all `25` preserve human-selection candidates, and all `25`
have complete grade-profile repair previews. No item has manual-overlap review
cards. All `25` preserve Lock deferred and Legion deferred decision context.
Runtime promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_eighth_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-closeout, M64-06, and M64-03 outputs exist:

```powershell
python tools\deck\build_eighth_slice_human_repair_review_packet.py
```

## Done Rule

`M65-01` is done when:

- all `25` M64 repair candidates are exported as review items
- human-selection, grade repair, Lock, and Legion contexts are preserved
- no human selection, acceptance, or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m65_02=true`

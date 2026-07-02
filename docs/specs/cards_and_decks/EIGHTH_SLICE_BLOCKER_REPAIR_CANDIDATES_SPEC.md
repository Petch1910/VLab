# Eighth-Slice Blocker Repair Candidates Spec

Milestone: `M64-06`

## Purpose

`M64-06` creates advisory repair candidates for the eighth-slice Kagero /
`link_joker_legion_mate` recipe review gates found in `M64-04` and carried
through `M64-05`.

The current eighth slice has no missing-card, copy-limit, trigger-count,
Grade 4 main-deck, clan-mismatch, manual-overlap, or pair-card blockers. The
remaining gates are:

- human recipe selection
- grade-profile review
- deferred Lock/Unlock runtime support
- deferred Legion/Mate runtime support

The report is an offline planning artifact only. It does not change `M64-03`
recipe drafts, does not record human selection, and does not promote a runtime
deck.

## Inputs

- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`
- `outputs/target_slice/m64_04_eighth_slice_recipe_validation_report.json`
- `outputs/target_slice/m64_05_eighth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m64_06_eighth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m64_06_eighth_slice_blocker_repair_candidates.md`

## Repair Rules

- Human recipe selection must be surfaced per recipe, but not recorded here.
- Manual-review card overlap must remain empty for the current evidence.
- Grade-profile substitution previews must use source-backed cards from
  runtime SQLite.
- Grade-profile previews target `G0=17/G1=14/G2=11/G3=8`.
- Candidate pair cards are protected from grade-profile removal.
- Lock/Unlock support remains deferred system work and cannot be repaired
  inside `M64-06`.
- Legion/Mate support remains deferred system work and cannot be repaired
  inside `M64-06`.
- Structural blockers such as missing cards, copy-limit errors, trigger-count
  mismatches, Grade 4 main-deck violations, and clan mismatch must be counted
  separately.

## Runtime Boundary

This milestone must not:

- modify `M64-03` recipe draft files
- record human selection
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report creates `25` repair items. All `25` have human
selection candidates, source-backed grade-profile substitution previews that
can reach `G0=17/G1=14/G2=11/G3=8`, Lock deferred packages, and Legion deferred
packages. Runtime promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_eighth_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M64-03, M64-04, and M64-05 outputs exist:

```powershell
python tools\deck\build_eighth_slice_blocker_repair_candidates.py
```

## Done Rule

`M64-06` is done when:

- all `25` recipe drafts have repair items
- human recipe selection is reported
- manual-review overlap remains absent
- grade-profile repair candidates are generated
- Lock deferred work is explicitly preserved
- Legion deferred work is explicitly preserved
- repair candidates remain advisory and non-runtime
- `ready_for_m64_closeout=true`

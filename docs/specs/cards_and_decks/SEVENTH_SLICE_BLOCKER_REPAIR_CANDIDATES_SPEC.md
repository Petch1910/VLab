# Seventh-Slice Blocker Repair Candidates Spec

Milestone: `M60-06`

## Purpose

`M60-06` creates advisory repair candidates for the seventh-slice Neo Nectar
G-era recipe blockers found in `M60-04` and carried through `M60-05`.

The report is an offline planning artifact only. It does not change the
`M60-03` recipe drafts, does not accept any repair, and does not promote a
runtime deck.

## Inputs

- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`
- `outputs/target_slice/m60_04_seventh_slice_recipe_validation_report.json`
- `outputs/target_slice/m60_05_seventh_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m60_01_seventh_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m60_06_seventh_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m60_06_seventh_slice_blocker_repair_candidates.md`

## Repair Rules

- Manual-review overlap must be surfaced per recipe.
- Manual-review cards are not resolved by this milestone.
- Manual substitution previews must use source-backed cards from runtime
  SQLite.
- Manual substitution previews must not select any card that is already in the
  global M60 manual-review set.
- Grade-profile substitution previews target `G0=17/G1=14/G2=11/G3=8`.
- Grade-profile previews may clear manual overlap only when their removals
  remove all manual-overlap cards from that recipe.
- G Zone / Stride support remains deferred system work and cannot be repaired
  inside `M60-06`.
- Bloom/token-like support remains deferred system work and cannot be repaired
  inside `M60-06`.
- Structural blockers such as missing cards, copy-limit errors, trigger-count
  mismatches, Grade 4 main-deck violations, and clan mismatch must be counted
  separately.

## Runtime Boundary

This milestone must not:

- modify `M60-03` recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable G Zone or Stride runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report creates `23` repair items. All `23` have complete
manual-overlap substitution previews and are ready for human repair review.
`21` recipes have complete grade-profile repair candidates. G Zone support and
Bloom/token-like support remain deferred for all `23` recipes. Runtime
promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_seventh_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M60-03, M60-04, and M60-05 outputs exist:

```powershell
python tools\deck\build_seventh_slice_blocker_repair_candidates.py
```

## Done Rule

`M60-06` is done when:

- all `23` blocked recipe drafts have repair items
- manual-review overlap is reported
- source-backed manual substitution previews are generated
- grade-profile repair candidates are generated where grade review exists
- G Zone deferred work is explicitly preserved
- Bloom/token deferred work is explicitly preserved
- repair candidates remain advisory and non-runtime
- `ready_for_m60_closeout=true`

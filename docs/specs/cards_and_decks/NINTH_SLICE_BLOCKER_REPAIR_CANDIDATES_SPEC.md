# Ninth-Slice Blocker Repair Candidates Spec

Milestone: `M68-06`

## Purpose

`M68-06` creates advisory repair candidates for the ninth-slice Aqua Force
G-era recipe blockers found in `M68-04` and carried through `M68-05`.

The report is an offline planning artifact only. It does not change the
`M68-03` recipe drafts, does not accept any repair, and does not promote a
runtime deck.

## Inputs

- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`
- `outputs/target_slice/m68_04_ninth_slice_recipe_validation_report.json`
- `outputs/target_slice/m68_05_ninth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory reports until the real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m68_06_ninth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m68_06_ninth_slice_blocker_repair_candidates.md`

## Repair Rules

- Manual-review overlap must be surfaced per recipe.
- Manual-review cards are not resolved by this milestone.
- Manual substitution previews must use source-backed cards from runtime
  SQLite.
- Manual substitution previews must not select any card that is already in the
  global M68 manual-review set.
- Grade-profile substitution previews target `G0=17/G1=14/G2=11/G3=8`.
- Grade-profile previews may clear manual overlap only when their removals
  remove all manual-overlap cards from that recipe.
- G Zone support remains deferred system work and cannot be repaired inside
  `M68-06`.
- Stride/G-unit support remains deferred system work and cannot be repaired
  inside `M68-06`.
- Aqua Force battle-count / attack-order support remains deferred system work
  and cannot be repaired inside `M68-06`.
- Structural blockers such as missing cards, copy-limit errors, trigger-count
  mismatches, trigger-profile mismatches, Grade 4 main-deck violations,
  set-scope mismatches, and clan mismatch must be counted separately.

## Runtime Boundary

This milestone must not:

- modify `M68-03` recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report creates `25` repair items. All `25` have complete
manual-overlap substitution previews and are ready for human repair review.
`23` recipes have complete grade-profile repair candidates. G Zone support,
Stride support, and Aqua Force battle-order support remain deferred for all
`25` recipes. Runtime promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-03, M68-04, and M68-05 outputs exist:

```powershell
python tools\deck\build_ninth_slice_blocker_repair_candidates.py
```

## Done Rule

`M68-06` is done when:

- all `25` blocked recipe drafts have repair items
- manual-review overlap is reported
- source-backed manual substitution previews are generated
- grade-profile repair candidates are generated where grade review exists
- G Zone deferred work is explicitly preserved
- Stride deferred work is explicitly preserved
- Aqua Force battle-order deferred work is explicitly preserved
- repair candidates remain advisory and non-runtime
- `ready_for_m68_closeout=true`

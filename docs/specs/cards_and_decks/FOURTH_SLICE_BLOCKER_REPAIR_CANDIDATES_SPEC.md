# Fourth-Slice Blocker Repair Candidates Spec

Milestone: `M48-06`

## Purpose

`M48-06` creates advisory repair candidates for the fourth-slice G-era expanded
Royal Paladin recipe blockers found in `M48-04` and carried through `M48-05`.

The report is an offline planning artifact only. It does not change the
`M48-03` recipe drafts, does not accept any repair, and does not promote a
runtime deck.

## Inputs

- `outputs/target_slice/m48_03_fourth_slice_recipe_draft_model.json`
- `outputs/target_slice/m48_04_fourth_slice_recipe_validation_report.json`
- `outputs/target_slice/m48_05_fourth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m48_01_fourth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m48_06_fourth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m48_06_fourth_slice_blocker_repair_candidates.md`

## Repair Rules

- Manual-review overlap must be surfaced per recipe.
- Manual-review cards are not resolved by this milestone.
- Manual substitution previews must use source-backed cards from runtime
  SQLite.
- Manual substitution previews must not select any card that is already in the
  global M48 manual-review set.
- Grade-profile substitution previews target `G0=17/G1=14/G2=11/G3=8`.
- Grade-profile previews may clear manual overlap only when their removals
  remove all manual-overlap cards from that recipe.
- G Zone / Stride support remains deferred system work and cannot be repaired
  inside `M48-06`.
- Structural blockers such as missing cards, copy-limit errors, trigger-count
  mismatches, Grade 4 main-deck violations, and clan mismatch must be counted
  separately.

## Runtime Boundary

This milestone must not:

- modify `M48-03` recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_blocker_repair_candidates.py
python -m unittest tests.test_fourth_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M48-06` is done when:

- all `25` blocked recipe drafts have repair items
- manual-review overlap is reported
- source-backed manual substitution previews are generated
- grade-profile repair candidates are generated where grade review exists
- G Zone deferred work is explicitly preserved
- repair candidates remain advisory and non-runtime
- `ready_for_m48_closeout=true`

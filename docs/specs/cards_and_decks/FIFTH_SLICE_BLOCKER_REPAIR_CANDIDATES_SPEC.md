# Fifth-Slice Blocker Repair Candidates Spec

Milestone: `M52-06`

## Purpose

`M52-06` creates advisory repair candidates for the fifth-slice recipe review
items found in `M52-04` and carried through `M52-05`.

The report is an offline planning artifact only. It does not change the
`M52-03` recipe drafts, does not accept any repair, and does not promote a
runtime deck.

## Inputs

- `outputs/target_slice/m52_03_fifth_slice_recipe_draft_model.json`
- `outputs/target_slice/m52_04_fifth_slice_recipe_validation_report.json`
- `outputs/target_slice/m52_05_fifth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m52_01_fifth_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m52_06_fifth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m52_06_fifth_slice_blocker_repair_candidates.md`

## Repair Rules

- Grade-profile substitution previews target `G0=17/G1=14/G2=11/G3=8`.
- Grade-profile previews must use source-backed cards from runtime SQLite.
- Human selection remains a review gate; this milestone does not record
  acceptance.
- Structural blockers such as missing cards, copy-limit errors, trigger-count
  mismatches, and clan mismatch must be counted separately.

## Runtime Boundary

This milestone must not:

- modify `M52-03` recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fifth_slice_blocker_repair_candidates.py
python -m unittest tests.test_fifth_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

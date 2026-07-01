# Third-Slice Blocker Repair Candidates Spec

Milestone: `M44-06`

## Purpose

`M44-06` creates source-backed repair candidates for blockers found in the
third-slice recipe pipeline.

The repair candidates are advisory substitution previews only. They do not
modify the `M44-03` draft files and do not record human acceptance.

## Inputs

- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`
- `outputs/target_slice/m44_04_third_slice_recipe_validation_report.json`
- `outputs/target_slice/m44_05_third_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m44_01_third_slice_fixture_scaffold.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m44_06_third_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m44_06_third_slice_blocker_repair_candidates.md`

## Repair Rules

- Manual-review card overlap must be surfaced.
- Manual-only same-grade replacement candidates may be incomplete.
- Grade-profile substitution candidates may clear manual overlap by removing
  manual-review cards.
- Grade-profile candidates target `G0=17/G1=14/G2=11/G3=8`.
- Additions must be source-backed by runtime SQLite.
- Candidate additions are limited to the `M44-01` third-slice set scope.
- Removals must come from the `M44-03` draft recipe.
- All repair candidates are advisory and require human/team review.

## Runtime Boundary

This milestone must not:

- modify M44-03 recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_third_slice_blocker_repair_candidates.py
python -m unittest tests.test_third_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M44-06` is done when:

- all `25` blocked recipe drafts have repair items
- manual-review overlap is reported
- grade-profile repair candidates are generated
- repair candidates remain advisory and non-runtime
- `ready_for_m44_closeout=true`

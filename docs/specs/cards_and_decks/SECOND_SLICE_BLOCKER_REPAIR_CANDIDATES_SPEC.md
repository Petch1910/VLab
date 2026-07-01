# Second-Slice Blocker Repair Candidates Spec

Milestone: `M40-05`

## Purpose

`M40-05` creates source-backed repair candidates for blockers found in the
second-slice recipe pipeline.

The repair candidates are advisory substitution previews only. They do not
modify the `M40-02` draft files and do not record human acceptance.

## Inputs

- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`
- `outputs/target_slice/m40_03_second_slice_recipe_validation_report.json`
- `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.md`

## Repair Rules

- manual-review card overlap must be surfaced
- manual-only same-grade replacement candidates may be incomplete
- grade-profile substitution candidates may clear manual overlap by removing
  manual-review cards
- grade-profile candidates target `G0=17/G1=14/G2=11/G3=8`
- additions must be source-backed by runtime SQLite
- removals must come from the M40-02 draft recipe
- all repair candidates are advisory and require human/team review

## Runtime Boundary

This milestone must not:

- modify M40-02 recipe draft files
- record human acceptance
- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_blocker_repair_candidates.py
python -m unittest tests.test_second_slice_blocker_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-05` is done when:

- all `25` blocked recipe drafts have repair items
- manual-review overlap is reported
- grade-profile repair candidates are generated
- repair candidates remain advisory and non-runtime
- `ready_for_m40_closeout=true`


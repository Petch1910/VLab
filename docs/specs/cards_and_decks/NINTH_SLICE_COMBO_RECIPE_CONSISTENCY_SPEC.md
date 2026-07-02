# Ninth-Slice Combo-To-Recipe Consistency Spec

Milestone: `M68-05`

## Purpose

`M68-05` checks whether each `M68-03` recipe draft actually contains the
candidate edge pair that caused the draft to exist, while carrying forward
`M68-04` validation status.

This prevents a recipe from moving forward merely because it has 50 cards and
16 triggers. For the ninth slice, pair consistency is not enough for promotion:
manual-review overlap, G Zone, Stride/G-unit, and Aqua Force battle-order gates
must remain visible until later review/repair milestones clear them.

## Inputs

- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`
- `outputs/target_slice/m68_04_ninth_slice_recipe_validation_report.json`

Tests may pass in-memory draft and validation reports until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m68_05_ninth_slice_combo_recipe_consistency_report.json`
- `outputs/target_slice/m68_05_ninth_slice_combo_recipe_consistency_report.md`

## Checks

- Every recipe has the candidate source card.
- Every recipe has the candidate target card.
- Missing pair-card ids are reported.
- Pair-level manual-review dependency is reported separately from recipe-level
  manual-review dependency.
- G Zone deferred status remains visible as review evidence.
- Stride/G-unit deferred status remains visible as review evidence.
- Aqua Force battle-count / attack-order deferred status remains visible as
  review evidence.
- Grade-profile review status remains visible as review evidence.
- Recipe validation status is carried forward.
- Promotion remains blocked unless validation passes and runtime readiness is
  true.

## Runtime Boundary

This milestone must not:

- create or inject saved decks
- publish drafts to UI deck lists
- promote runtime deck fixtures
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report checks `25` recipe drafts. Pair cards are present
for all `25`, with no missing pair-card checks and no pair-level manual-review
dependency overlap. Promotion remains blocked because all `25` drafts remain
blocked by recipe-level manual-review overlap; all `25` carry G Zone, Stride,
Aqua Force battle-order, and human-selection review evidence, and `23` also
carry grade-profile review evidence.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-03 and M68-04 outputs exist:

```powershell
python tools\deck\check_ninth_slice_combo_recipe_consistency.py
```

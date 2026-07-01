# Sixth-Slice Human-Selected Recipe Artifact Spec

Milestone: `M57-02`

## Purpose

`M57-02` records exactly one explicitly selected sixth-slice review item from
the `M57-01` human repair review packet.

This artifact records selection only. It does not record acceptance, does not
record a G Zone / Stride decision, and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.json`
- CLI/user input: `review_item_id`
- CLI/user input: `selection_text`

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selected_recipe_artifact.md`

## Selection Rules

- The selected `review_item_id` must be non-empty and must exist in `M57-01`.
- The selected id must match exactly one review item.
- `selection_text` must be non-empty.
- The artifact must carry pair context, manual-repair context, grade-profile
  repair context, and deferred G Zone context from the selected review item.
- `ready_for_m57_03=true` only when the selected review item is ready for
  human repair review, has complete manual and grade repairs, and has no
  structural blockers.
- Deferred G Zone work does not block `M57-03`; it must be recorded later by
  `M57-04` before fixture promotion can be considered.

## Runtime Boundary

This milestone must not:

- choose a recipe automatically
- record human acceptance
- record a G Zone / Stride decision
- modify M56/M57 recipe artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_human_selected_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only with explicit selection input:

```powershell
python tools\deck\build_sixth_slice_human_selected_recipe_artifact.py `
  --review-item-id <M57-01 review_item_id> `
  --selection-text "<explicit user selection>"
```

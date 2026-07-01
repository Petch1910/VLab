# Fifth-Slice Human-Selected Recipe Artifact Spec

Milestone: `M53-02`

## Purpose

`M53-02` records exactly one explicitly selected fifth-slice review item from
the `M53-01` human review packet.

This artifact records selection only. It does not record acceptance and does
not promote runtime decks.

## Inputs

- `outputs/target_slice/m53_01_fifth_slice_human_repair_review_packet.json`
- CLI/user input: `review_item_id`
- CLI/user input: `selection_text`

## Outputs

- `outputs/target_slice/m53_02_fifth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m53_02_fifth_slice_human_selected_recipe_artifact.md`

## Selection Rules

- The selected `review_item_id` must be non-empty and must exist in `M53-01`.
- The selected id must match exactly one review item.
- `selection_text` must be non-empty.
- The artifact must carry pair context and grade-profile repair context from
  the selected review item.
- `ready_for_m53_03=true` only when the selected review item is ready for
  human repair review, has complete grade repair, and has no structural
  blockers.

## Runtime Boundary

This milestone must not:

- choose a recipe automatically
- record human acceptance
- modify M52/M53 recipe artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_fifth_slice_human_selected_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only with explicit selection input:

```powershell
python tools\deck\build_fifth_slice_human_selected_recipe_artifact.py `
  --review-item-id <M53-01 review_item_id> `
  --selection-text "<explicit user selection>"
```

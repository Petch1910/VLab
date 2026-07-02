# Ninth-Slice Human-Selected Recipe Artifact Spec

Milestone: `M69-02`

## Purpose

`M69-02` records exactly one explicitly selected ninth-slice review item from
the `M69-01` human repair review packet.

This artifact records selection only. It does not record acceptance, does not
record a G Zone decision, does not record a Stride decision, does not record an
Aqua Force battle-order decision, and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m69_01_ninth_slice_human_repair_review_packet.json`
- CLI/user input: `review_item_id`
- CLI/user input: `selection_text`

Tests may pass an in-memory `M69-01` review packet until real upstream artifacts
exist.

## Outputs

- `outputs/target_slice/m69_02_ninth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m69_02_ninth_slice_human_selected_recipe_artifact.md`

## Selection Rules

- The selected `review_item_id` must be non-empty and must exist in `M69-01`.
- The selected id must match exactly one review item.
- `selection_text` must be non-empty.
- The artifact must carry pair context, manual-repair context, grade-profile
  repair context, deferred G Zone context, deferred Stride context, and
  deferred Aqua Force battle-order context from the selected review item.
- `ready_for_m69_03=true` only when the selected review item is ready for
  human repair review, has complete manual repair, has complete grade repair or
  explicitly needs no grade repair, and has no structural blockers.
- Deferred G Zone, Stride, and Aqua Force battle-order work do not block
  `M69-03`; they must be recorded later by `M69-04` before fixture promotion
  can be considered.

## Runtime Boundary

This milestone must not:

- choose a recipe automatically
- record human acceptance
- record a G Zone decision
- record a Stride decision
- record an Aqua Force battle-order decision
- modify M68/M69 recipe artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_human_selected_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only with explicit selection input after the real
`M69-01` packet exists:

```powershell
python tools\deck\build_ninth_slice_human_selected_recipe_artifact.py `
  --review-item-id <M69-01 review_item_id> `
  --selection-text "<explicit user selection>"
```

## Done Rule

`M69-02` is done when:

- exactly one review item is recorded as selected
- the artifact carries the selected pair/manual/grade/G Zone/Stride/Aqua Force
  context
- selection is traceable to explicit `review_item_id` and `selection_text`
- no acceptance or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m69_03=true` for valid selected items

# Sixth-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M57-05`

## Purpose

`M57-05` validates the repaired quantity preview from `M57-03` after the
explicit G Zone / Stride boundary from `M57-04`.

This is still an offline validation rerun. It may report that the repaired
main-deck recipe is validator-passed and consistency-passed, but it must not
create a runtime fixture, saved deck, UI entry, or bot playbook. `M57-06` is
the runtime fixture promotion gate.

## Inputs

- `outputs/target_slice/m57_03_sixth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m57_04_sixth_slice_g_zone_stride_decision_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m57_05_sixth_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m57_05_sixth_slice_repaired_recipe_validation_report.md`

## Validation Rules

Reuse the sixth-slice recipe validator and combo consistency rules against the
`M57-03` repaired quantity preview:

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 / G units must not appear in the main deck.
- All cards must match the selected clan.
- Grade profile should equal the classic advisory target.
- Manual-review card overlap must be absent after the accepted repair.
- Human acceptance must already be recorded by `M57-03`.
- The `M57-04` boundary must be `main_deck_only_review_no_runtime_promotion`
  to open `M57-06`.

## G Zone Boundary

`M57-05` validates only the main deck. The `M57-04` boundary may suppress
`g_zone_support_deferred` for this validation pass, but it must not enable G
Zone, Stride, Generation Break runtime, Grade 4 main-deck usage, or G-unit
runtime usage.

## Runtime Boundary

This milestone must not:

- record human acceptance
- record a new G Zone / Stride decision
- mutate M57-03 or M57-04 artifact files
- mutate M56 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M57-03 and M57-04 outputs exist:

```powershell
python tools\deck\validate_sixth_slice_repaired_recipe.py
```

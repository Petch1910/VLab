# Sixth-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M57-06`

## Purpose

`M57-06` checks whether the human-selected, accepted, G Zone-bounded, and
revalidated sixth-slice recipe can be promoted into an offline runtime/test
fixture.

The gate may create a fixture artifact only. It must not inject saved decks,
publish UI deck lists, enable bot playbooks, enable G Zone/Stride runtime, or
mutate `GameState`.

## Inputs

- `outputs/target_slice/m57_03_sixth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m57_05_sixth_slice_repaired_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m57_06_sixth_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m57_06_sixth_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/<recipe_id>_shadow_paladin_m57_06.json`
  only if all gate checks pass

## Gate Checks

- Human selection and acceptance are recorded.
- The M57-04/M57-05 boundary is
  `main_deck_only_review_no_runtime_promotion`.
- G Zone runtime, Stride runtime, Grade 4 main-deck use, and G-unit runtime
  remain disabled.
- Validation passes with runtime readiness true.
- Main deck is 50 cards.
- Trigger count is 16 with `Critical=4/Draw=4/Heal=4/Stand=4`.
- Grade profile is `G0=17/G1=14/G2=11/G3=8`.
- Grade 4 main-deck count is 0.
- Combo-to-recipe consistency passes and allows promotion.
- M57-05 reports `ready_for_m57_06=true`.
- M57-03 repaired rows still match the M57-05 accepted recipe id and sum to
  50 cards.
- M57-05 did not create runtime fixtures, UI entries, bot playbooks, or
  `GameState` mutation.

## Runtime Boundary

This milestone must not:

- modify M56/M57 source artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone/Stride runtime
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M57-03 and M57-05 outputs exist:

```powershell
python tools\deck\build_sixth_slice_runtime_fixture_promotion_gate.py
```

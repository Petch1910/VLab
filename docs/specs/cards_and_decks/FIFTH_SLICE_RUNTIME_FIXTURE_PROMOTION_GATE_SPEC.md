# Fifth-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M53-05`

## Purpose

`M53-05` checks whether the M53 accepted and validated fifth-slice recipe can
be promoted into an offline runtime/test fixture.

The gate may create a fixture artifact only. It must not inject saved decks,
publish UI deck lists, enable bot playbooks, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m53_04_fifth_slice_repaired_recipe_validation_rerun.json`

## Outputs

- `outputs/target_slice/m53_05_fifth_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m53_05_fifth_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/<recipe_id>_gold_paladin_m53_05.json`
  only if all gate checks pass

## Gate Checks

- Human selection and acceptance are both recorded.
- Validation passes with runtime readiness true.
- Main deck is 50 cards.
- Trigger count is 16 with `Critical=4/Draw=4/Heal=4/Stand=4`.
- Grade profile is `G0=17/G1=14/G2=11/G3=8`.
- Combo-to-recipe consistency passes and allows promotion.
- M53-04 reports `ready_for_m53_05=true`.
- M53-04 did not create runtime fixtures, UI entries, bot playbooks, or
  `GameState` mutation.

## Runtime Boundary

This milestone must not:

- modify M52/M53 source artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_fifth_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M53-04 output exists:

```powershell
python tools\deck\build_fifth_slice_runtime_fixture_promotion_gate.py
```

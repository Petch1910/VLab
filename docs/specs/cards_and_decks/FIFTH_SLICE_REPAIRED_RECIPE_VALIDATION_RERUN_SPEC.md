# Fifth-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M53-04`

## Purpose

`M53-04` reruns validation and combo-to-recipe consistency for the recipe
accepted in `M53-03`.

The rerun is in-memory only. It may prove that the repaired recipe satisfies
offline validation and consistency, but runtime fixture promotion remains
disabled until `M53-05`.

## Inputs

- `outputs/target_slice/m53_03_fifth_slice_human_accepted_repair_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m53_04_fifth_slice_repaired_recipe_validation_rerun.json`
- `outputs/target_slice/m53_04_fifth_slice_repaired_recipe_validation_rerun.md`

## Validation Rules

- Build one in-memory recipe from `M53-03.accepted_repair.repaired_quantities`.
- Reuse the M52 fifth-slice validator rules:
  - main deck count `50`
  - trigger count `16`
  - heal trigger max `4`
  - copy limit from SQLite
  - clan consistency
  - grade target `G0=17/G1=14/G2=11/G3=8`
- Reuse the M52 combo-to-recipe consistency check.
- `ready_for_m53_05=true` only when M53-03 is ready for validation, validation
  passes, runtime readiness is true, and consistency promotion is allowed.

## Runtime Boundary

This milestone must not:

- modify M52/M53 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_fifth_slice_repaired_recipe_validation_rerun
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M53-03 output exists:

```powershell
python tools\deck\build_fifth_slice_repaired_recipe_validation_rerun.py
```

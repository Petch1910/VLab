# Second-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M41-03`

## Purpose

`M41-03` validates the repaired Oracle Think Tank recipe accepted in `M41-02`.

This validation rerun is the gate before any fixture-promotion work. If the
accepted repair has blockers, the pipeline must route to a repair loop instead
of `M41-04`.

## Inputs

- `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.json`
- `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.md`

## Validation Rules

The rerun checks:

- human acceptance is recorded
- main deck count is `50`
- trigger count is `16`
- heal trigger count does not exceed `4`
- card copy limits are respected
- all card ids exist in SQLite
- clan identity matches the selected target
- grade counts match `G0=17/G1=14/G2=11/G3=8`
- manual-review cards removed by the accepted package are no longer present

## Current Expected Result

The accepted `m40_recipe_001` repair:

- keeps main deck count at `50`
- clears manual-review card overlap
- matches the grade profile
- fails trigger count with `2/16`

Therefore it must not enter `M41-04`.

## Runtime Boundary

This milestone must not:

- mutate the accepted artifact
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_second_slice_repaired_recipe.py
python -m unittest tests.test_second_slice_repaired_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-03` is done when:

- the validation report is written
- blockers are explicit
- `ready_for_m41_04=false` when blockers exist
- next target is a repair loop when blockers exist
- project status docs no longer point directly to `M41-04`

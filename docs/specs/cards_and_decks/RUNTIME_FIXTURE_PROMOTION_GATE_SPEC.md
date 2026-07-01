# Runtime Fixture Promotion Gate Spec

Milestone: `M38-04`

## Purpose

`M38-04` is the gate that decides whether the first human-accepted recipe can
become an offline runtime/test fixture.

It consumes the M38-03 accepted artifact and checks that all review and
validation gates have passed. It may create a fixture artifact under
`outputs/target_slice/runtime_fixtures/`, but it must not inject anything into
the live deck library, UI, bot, or `GameState`.

## Inputs

- `outputs/target_slice/m38_03_human_accepted_recipe_artifact.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`

## Outputs

- `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/recipe_003_classic_core_nova_grappler_m38_04.json`

## Gate Checks

The gate must pass all of these checks:

- `human_acceptance`: M38-03 records an accepted human decision.
- `grade_profile_review`: grade profile is `G0=17`, `G1=14`, `G2=11`, `G3=8`.
- `validation`: blocker count is `0`, main deck count is `50`, trigger count
  is `16`, and M38-03 validation status is ready for the runtime gate.
- `combo_consistency`: the revised consistency source is
  `consistent_pending_human_acceptance` or a later runtime-candidate status.
- `runtime_boundary`: the previous artifact did not already mutate runtime
  decks, enable bot integration, or bypass the gate.

## Fixture Boundary

The generated fixture is allowed to be an offline test/runtime fixture artifact
only. It must not:

- mutate saved player decks
- appear automatically in the UI deck list
- enable bot playbook behavior
- change `GameState`
- parse live card text

## Verification

```powershell
python tools\deck\build_runtime_fixture_promotion_gate.py
python -m unittest tests.test_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M38-04` is done when every gate check passes, the offline fixture artifact is
created, tests cover both pass and fail behavior, and the next target is
`M38-closeout`.

# Second-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M41-04`

## Purpose

`M41-04` decides whether the accepted and repaired Oracle Think Tank recipe may
become an offline runtime/test fixture.

The milestone may create a fixture artifact under
`outputs/target_slice/runtime_fixtures/`, but it must not inject saved decks,
publish UI deck-list entries, enable bot/playbook behavior, or mutate
`GameState`.

## Inputs

- `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.json`
- `outputs/target_slice/m41_repair_validate_second_slice_repaired_recipe.json`
- `outputs/target_slice/m40_04_second_slice_combo_recipe_consistency_report.json`

## Outputs

- `outputs/target_slice/m41_04_second_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m41_04_second_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json`

## Gate Checks

The gate passes only when all checks pass:

- `human_acceptance`: trigger repair acceptance artifact records an accepted
  human decision.
- `validation`: repaired recipe validation status is `validator_passed`, has
  `0` blockers, `50` cards, and `16` triggers.
- `grade_profile_review`: grade profile remains `G0=17/G1=14/G2=11/G3=8`.
- `combo_pair_consistency`: M40-04 still confirms the source candidate pair is
  present with no missing pair-card dependencies.
- `manual_review_cleared_after_repair`: repaired validation has no
  manual-review overlap.
- `runtime_boundary`: previous artifacts did not already mutate runtime decks,
  saved decks, UI, bot/playbook, or `GameState`.

## Boundary

The generated fixture is allowed to be an offline runtime/test fixture artifact
only. It must not:

- mutate saved player decks
- appear automatically in the UI deck list
- enable bot playbook behavior
- change `GameState`
- parse live card text

## Verification

```powershell
python tools\deck\build_second_slice_runtime_fixture_promotion_gate.py
python -m unittest tests.test_second_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-04` is done when every gate check passes, the offline fixture artifact is
created, tests cover pass and fail behavior, docs point to `M41-closeout`, and
no saved deck/UI/bot/runtime mutation beyond the fixture artifact occurs.

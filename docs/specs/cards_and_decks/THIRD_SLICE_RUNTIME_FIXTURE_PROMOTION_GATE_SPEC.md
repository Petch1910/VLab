# Third-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M45-04`

## Purpose

`M45-04` decides whether the accepted, repaired, and validated third-slice
recipe may become an offline runtime/test fixture.

The milestone may create a fixture artifact under
`outputs/target_slice/runtime_fixtures/`, but it must not inject saved decks,
publish UI deck-list entries, enable bot/playbook behavior, or mutate
`GameState`.

## Inputs

- `outputs/target_slice/m45_02_third_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m45_03_third_slice_repaired_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m45_04_third_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m45_04_third_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json`

## Gate Checks

The gate passes only when all checks pass:

- `human_acceptance`: M45-02 records an accepted human decision.
- `validation`: M45-03 validation status is `validator_passed`, has `0`
  blockers, `50` cards, `16` triggers, and is ready for M45-04.
- `grade_profile_review`: grade profile is `G0=17/G1=14/G2=11/G3=8`.
- `combo_pair_consistency`: the accepted source/target pair cards are present
  in the repaired quantity preview.
- `manual_review_cleared_after_repair`: repaired validation has no
  manual-review overlap.
- `combined_repair_integrity`: M45-02 recomputed the combined grade repair
  after manual substitution and has no repair application issues.
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
python tools\deck\build_third_slice_runtime_fixture_promotion_gate.py
python -m unittest tests.test_third_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M45-04` is done when every gate check passes, the offline fixture artifact is
created, tests cover pass and fail behavior, docs point to `M45-closeout`, and
no saved deck/UI/bot/runtime mutation beyond the fixture artifact occurs.

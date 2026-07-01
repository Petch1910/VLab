# Fourth-Slice Runtime Fixture Gate Spec

Milestone: `M49-05`

## Purpose

`M49-05` decides whether the accepted, repaired, validated fourth-slice
main-deck recipe may become an offline runtime/test fixture.

The milestone may create a fixture artifact under
`outputs/target_slice/runtime_fixtures/`, but it must not inject saved decks,
publish UI deck-list entries, enable bot/playbook behavior, enable G Zone /
Stride runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m49_03_fourth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m49_04_fourth_slice_repaired_recipe_validation_report.json`

## Outputs

- `outputs/target_slice/m49_05_fourth_slice_runtime_fixture_gate.json`
- `outputs/target_slice/m49_05_fourth_slice_runtime_fixture_gate.md`
- `outputs/target_slice/runtime_fixtures/m48_recipe_001_g_series_first_royal_paladin_m49_05.json`

## Gate Checks

The gate passes only when all checks pass:

- `human_acceptance`: M49-03 records an accepted human decision.
- `g_zone_boundary`: M49-02/M49-03 boundary is
  `main_deck_only_for_current_windows_fixture`; G Zone and Stride runtime remain
  disabled.
- `validation`: M49-04 validation status is `validator_passed`, has `0`
  blockers, `50` cards, `16` triggers, and is ready for M49-05.
- `grade_profile_review`: grade profile is `G0=17/G1=14/G2=11/G3=8` and Grade
  4 main-deck count is `0`.
- `combo_pair_consistency`: the accepted source/target pair cards are present
  in the repaired quantity preview.
- `manual_review_cleared_after_repair`: repaired validation has no
  manual-review overlap.
- `combined_repair_integrity`: M49-03 recomputed the combined grade repair
  after manual substitution and has no repair application issues.
- `runtime_boundary`: previous artifacts did not already mutate runtime decks,
  saved decks, UI, bot/playbook, G Zone runtime, Stride runtime, or `GameState`.

## Boundary

The generated fixture is allowed to be an offline runtime/test fixture artifact
only. It must not:

- mutate saved player decks
- appear automatically in the UI deck list
- enable bot playbook behavior
- enable G Zone runtime
- enable Stride runtime
- change `GameState`
- parse live card text

## Verification

```powershell
python tools\deck\build_fourth_slice_runtime_fixture_gate.py
python -m unittest tests.test_fourth_slice_runtime_fixture_gate
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M49-05` is done when every gate check passes, the offline fixture artifact is
created, tests cover pass and fail behavior, docs point to `M49-closeout`, and
no saved deck/UI/bot/runtime mutation beyond the fixture artifact occurs.

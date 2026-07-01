# Second-Slice Readiness Comparison Spec

Milestone: `M36-05`

## Purpose

`M36-05` compares the second selected slice (`Classic Core / Oracle Think
Tank`) against the completed first-slice deck recipe validation pipeline before
broader scale-out.

The comparison is a planning gate. It does not start second-slice recipe
drafting and does not permit runtime/bot promotion.

## Inputs

- `outputs/target_slice/m35_closeout_hybrid_vertical_slice.json`
- `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
- `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
- `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- `outputs/target_slice/m36_04_combo_recipe_consistency_report.json`

## Outputs

- `outputs/target_slice/m36_05_second_slice_readiness_comparison.json`
- `outputs/target_slice/m36_05_second_slice_readiness_comparison.md`

## Required Findings

- first-slice runtime-ready recipe count
- first-slice promotable combo-line count
- first-slice review/slot blockers
- second-slice fixture readiness
- second-slice semantic probe readiness
- second-slice probe candidate edge count
- recommendation for whether to start broader recipe scale-out

## Runtime Boundary

This milestone must not:

- start second-slice recipe drafting
- create runtime decks
- publish bot playbooks
- enable bot integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_readiness_comparison.py
python -m unittest tests.test_second_slice_readiness_comparison
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-05` is done when the comparison reports second-slice future recipe
readiness, keeps runtime scale-out disabled, and points to `M36-closeout`.


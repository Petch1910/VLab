# First Slice Combo Line Explainer Spec

Milestone: `M35-D3`

## Purpose

Explain why each M35-D2 skeleton/package is included and what it needs to work.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_d1_first_slice_candidate_packages.json
outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json
```

## Explainer Contract

Each combo line must include:

- source skeleton id
- source package id
- anchor card
- involved cards
- top clean compatibility steps
- why the package is included
- what the line still needs to work
- known limits

## Hard Boundaries

- no per-card quantities
- no final play sequence legality claim
- no reviewed playbook seed yet
- no bot runtime data
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json
outputs/target_slice/m35_d3_first_slice_combo_line_explainer.md
```

## Verification

```powershell
python tools\deck\build_first_slice_combo_line_explainer.py
python -m unittest tests.test_first_slice_combo_line_explainer
python -m unittest discover -s tests -p "test_*.py"
```

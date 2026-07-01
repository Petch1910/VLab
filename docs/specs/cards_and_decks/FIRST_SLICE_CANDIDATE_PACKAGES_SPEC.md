# First Slice Candidate Packages Spec

Milestone: `M35-D1`

## Purpose

Select human-reviewable candidate card packages from the clean M35-C5 synergy
edges.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json
```

## Selection Policy

M35-D1 packages may use only M35-C5 edges where:

- status is `synergy`
- `candidate_for_m35_d1` is true
- no `manual_review_required`
- no `missing_data`
- no `conflict`

Packages are selected as ego packages around anchor cards using top clean
incident edges. They are inputs for D2 only.

## Hard Boundaries

- no deck quantities
- no deck skeleton
- no final deck choice
- no playbook promotion
- no bot runtime data
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_d1_first_slice_candidate_packages.json
outputs/target_slice/m35_d1_first_slice_candidate_packages.md
```

## Verification

```powershell
python tools\deck\build_first_slice_candidate_packages.py
python -m unittest tests.test_first_slice_candidate_packages
python -m unittest discover -s tests -p "test_*.py"
```

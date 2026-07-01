# First Slice Selected Compatibility Output Spec

Milestone: `M35-C5`

## Purpose

Combine the selected-slice compatibility detectors into one advisory
edge-level output for M35-D1 candidate package selection.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json
outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.json
outputs/target_slice/m35_c4_first_slice_zone_target_detector.json
```

## Output Labels

Each edge must expose reasons using these labels:

- `synergy`
- `conflict`
- `missing_data`
- `manual_review_required`

An edge can have more than one label. The status priority is:

```text
manual_review_required > missing_data > mixed > conflict > synergy > neutral
```

## D1 Candidate Policy

`m35_d1_candidate_edges` may include only clean `synergy` edges:

- no `manual_review_required`
- no `missing_data`
- no `conflict`
- positive net score

These candidates are inputs for D1 package selection only. They are not final
deck choices.

## Hard Boundaries

- no deck skeleton
- no deck slot counts
- no automatic playbook promotion
- no bot runtime data
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.md
```

## Verification

```powershell
python tools\deck\build_first_slice_selected_compatibility_output.py
python -m unittest tests.test_first_slice_selected_compatibility_output
python -m unittest discover -s tests -p "test_*.py"
```

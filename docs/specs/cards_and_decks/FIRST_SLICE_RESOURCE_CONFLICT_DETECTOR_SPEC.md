# First Slice Resource Conflict Detector Spec

Milestone: `M35-C2`

## Purpose

Analyze the M35-C1 selected-slice pair graph for coarse resource pressure and
resource support.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Input

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
```

## Resource Model

M35-C2 may detect only resource categories that exist in the M35-B3
requirement/provider tags:

- `counter_blast`
- `soul`
- `hand_cards`
- `self_rest`
- `self_retire`

The detector must not claim exact resource amounts unless structured ability
cost amounts exist. Current output is pressure/support only.

## Edge Verdicts

Each resource-relevant edge may receive one advisory verdict:

- `resource_support`
- `mixed_support_and_shared_pressure`
- `shared_resource_pressure`
- `target_resource_need_not_supported_by_source`
- `source_resource_profile_only`
- `missing_slice_recovery`

These verdicts are inputs for later compatibility filters. They are not final
deck-building decisions.

## Manual Review Policy

If the source M35-C1 edge is manual-review gated, the resource finding remains:

```text
confidence = review_required
```

Manual-review findings cannot become high-confidence compatibility until the
manual-review queue is resolved.

## Hard Boundaries

- no exact cost amount claims
- no timing compatibility verdict yet
- no zone/target compatibility verdict yet
- no deck skeleton
- no bot playbook
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.md
```

## Verification

```powershell
python tools\deck\build_first_slice_resource_conflict_detector.py
python -m unittest tests.test_first_slice_resource_conflict_detector
python -m unittest discover -s tests -p "test_*.py"
```

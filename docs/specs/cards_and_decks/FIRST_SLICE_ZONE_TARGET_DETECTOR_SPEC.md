# First Slice Zone / Target Detector Spec

Milestone: `M35-C4`

## Purpose

Analyze the M35-C1 selected-slice pair graph for advisory zone and target
compatibility.

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
```

## Zone Model

M35-C4 may detect only zone categories available from the current
requirement/provider tags:

- `vanguard_circle`
- `rear_guard_circle`
- `guardian_circle`
- `damage_zone`
- `drop_zone`
- `soul`
- `deck`
- `hand`

The detector treats `vanguard_circle` as an exclusive role signal, not as a
zone that needs a provider.

## Edge Verdicts

Each zone-relevant edge may receive one advisory verdict:

- `zone_support`
- `mixed_zone_support_and_slot_pressure`
- `vanguard_role_conflict`
- `rear_guard_slot_pressure`
- `target_zone_need_not_supported_by_source`
- `source_zone_profile_only`
- `missing_zone_support_in_slice`

These verdicts are inputs for M35-C5 selected-slice compatibility output. They
are not final deck-building decisions.

## Manual Review Policy

If the source M35-C1 edge is manual-review gated, the zone finding remains:

```text
confidence = review_required
```

Manual-review findings cannot become high-confidence compatibility until the
manual-review queue is resolved.

## Hard Boundaries

- no exact board-capacity claim
- no final archetype rejection from Vanguard-circle conflicts
- no deck skeleton
- no bot playbook
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_c4_first_slice_zone_target_detector.json
outputs/target_slice/m35_c4_first_slice_zone_target_detector.md
```

## Verification

```powershell
python tools\deck\build_first_slice_zone_target_detector.py
python -m unittest tests.test_first_slice_zone_target_detector
python -m unittest discover -s tests -p "test_*.py"
```

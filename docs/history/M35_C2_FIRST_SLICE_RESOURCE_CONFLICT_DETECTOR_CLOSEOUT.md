# M35-C2 First Slice Resource Conflict Detector Closeout

## Summary

Implemented the resource conflict detector for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The detector consumes the M35-C1 pair compatibility graph and emits advisory
resource-support/resource-pressure findings while preserving manual-review
gating.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_RESOURCE_CONFLICT_DETECTOR_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_resource_conflict_detector.py
```

Tests:

```text
tests/test_first_slice_resource_conflict_detector.py
```

Outputs:

```text
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.md
```

## Result

- source graph edges: `3919`
- resource-relevant edges: `2826`
- manual-review resource edges: `224`
- missing recovery resource types: `0`
- ready for M35-C3: `true`

Verdict counts:

```text
mixed_support_and_shared_pressure: 12
resource_support: 357
shared_resource_pressure: 286
source_resource_profile_only: 1293
target_resource_need_not_supported_by_source: 878
```

Detected resource demand:

```text
counter_blast: 27
soul: 5
```

Detected resource providers:

```text
counter_blast: 12
hand_cards: 15
soul: 13
```

## Guardrails

- Advisory resource detector only.
- No exact resource amount claim until structured cost amounts exist.
- No timing compatibility verdict yet.
- No zone/target compatibility verdict yet.
- Manual-review findings stay review-required.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_resource_conflict_detector.py
python -m unittest tests.test_first_slice_resource_conflict_detector
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 143 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-C3`: Timing compatibility detector for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

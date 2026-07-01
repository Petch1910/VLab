# M35-C4 First Slice Zone / Target Detector Closeout

## Summary

Implemented the zone/target compatibility detector for the selected first
slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The detector consumes the M35-C1 pair graph and carries M35-C2 resource plus
M35-C3 timing verdicts forward as context.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_ZONE_TARGET_DETECTOR_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_zone_target_detector.py
```

Tests:

```text
tests/test_first_slice_zone_target_detector.py
```

Outputs:

```text
outputs/target_slice/m35_c4_first_slice_zone_target_detector.json
outputs/target_slice/m35_c4_first_slice_zone_target_detector.md
```

## Result

- source graph edges: `3919`
- zone-relevant edges: `3704`
- manual-review zone edges: `331`
- unsupported required zone types: `1`
- ready for M35-C5: `true`

Verdict counts:

```text
missing_zone_support_in_slice: 52
mixed_zone_support_and_slot_pressure: 339
rear_guard_slot_pressure: 436
source_zone_profile_only: 766
target_zone_need_not_supported_by_source: 1641
vanguard_role_conflict: 402
zone_support: 68
```

Zone requirements:

```text
damage_zone: 16
drop_zone: 1
rear_guard_circle: 43
vanguard_circle: 28
```

Zone providers:

```text
damage_zone: 12
deck: 13
hand: 15
rear_guard_circle: 8
soul: 13
```

Unsupported required zones:

```text
drop_zone
```

## Guardrails

- Advisory zone/target detector only.
- Vanguard-circle conflicts are archetype-review signals, not final rejection.
- Rear-guard slot pressure is coarse, not exact board-capacity calculation.
- Manual-review findings stay review-required.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_zone_target_detector.py
python -m unittest tests.test_first_slice_zone_target_detector
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 161 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-C5`: Selected-slice compatibility output for the selected Classic Core /
โนว่า เกรปเปอร์ slice.

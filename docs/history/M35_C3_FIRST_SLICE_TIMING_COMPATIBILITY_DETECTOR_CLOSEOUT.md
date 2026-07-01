# M35-C3 First Slice Timing Compatibility Detector Closeout

## Summary

Implemented the timing compatibility detector for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The detector consumes the M35-C1 pair graph and carries M35-C2 resource
verdicts forward as context. It uses card-level timing tags as an explicit
proxy until structured effect-level timing exists.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_TIMING_COMPATIBILITY_DETECTOR_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_timing_compatibility_detector.py
```

Tests:

```text
tests/test_first_slice_timing_compatibility_detector.py
```

Outputs:

```text
outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.json
outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.md
```

## Result

- source graph edges: `3919`
- timing-relevant edges: `3876`
- manual-review timing edges: `363`
- timing window types: `7`
- ready for M35-C4: `true`

Verdict counts:

```text
provider_after_consumer_window: 990
same_window_requires_ordering: 804
source_timing_unknown_or_static: 943
target_timing_not_constrained: 317
timing_can_precede: 822
```

Timing window counts:

```text
attack_declaration: 40
attack_hit: 26
boost_step: 18
end_phase: 6
on_call: 11
on_ride: 12
trigger_check: 19
```

## Guardrails

- Advisory timing detector only.
- Uses card-level timing tags as proxy.
- No final timing legality claim until structured effect timing exists.
- No zone/target compatibility verdict yet.
- Manual-review findings stay review-required.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_timing_compatibility_detector.py
python -m unittest tests.test_first_slice_timing_compatibility_detector
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 152 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-C4`: Zone/target compatibility detector for the selected Classic Core /
โนว่า เกรปเปอร์ slice.

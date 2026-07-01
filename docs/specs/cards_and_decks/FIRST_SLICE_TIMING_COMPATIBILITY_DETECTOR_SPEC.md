# First Slice Timing Compatibility Detector Spec

Milestone: `M35-C3`

## Purpose

Analyze the M35-C1 selected-slice pair graph for advisory timing compatibility.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
outputs/target_slice/m35_c2_first_slice_resource_conflict_detector.json
```

## Timing Proxy Model

The current selected slice has card-level timing tags, not structured
per-effect provider timing. M35-C3 therefore uses card-level timing requirements
as an explicit proxy.

The detector must not claim final timing legality until structured effect-level
timing exists.

## Timing Order

M35-C3 uses this coarse order:

```text
on_ride < on_call < boost_step < attack_declaration < trigger_check < attack_hit < end_phase
```

## Edge Verdicts

Each timing-relevant edge may receive one advisory verdict:

- `timing_can_precede`
- `same_window_requires_ordering`
- `provider_after_consumer_window`
- `source_timing_unknown_or_static`
- `target_timing_not_constrained`

These verdicts are inputs for later compatibility filters. They are not final
deck-building decisions.

## Manual Review Policy

If the source M35-C1 edge is manual-review gated, the timing finding remains:

```text
confidence = review_required
```

Manual-review findings cannot become high-confidence compatibility until the
manual-review queue is resolved.

## Hard Boundaries

- no final timing legality claim
- no structured effect-level timing claim
- no zone/target compatibility verdict yet
- no deck skeleton
- no bot playbook
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.json
outputs/target_slice/m35_c3_first_slice_timing_compatibility_detector.md
```

## Verification

```powershell
python tools\deck\build_first_slice_timing_compatibility_detector.py
python -m unittest tests.test_first_slice_timing_compatibility_detector
python -m unittest discover -s tests -p "test_*.py"
```

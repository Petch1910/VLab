# M35-B4 First Slice Manual Review Queue Closeout

## Summary

Implemented the manual review queue for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

This closes Phase B of the Hybrid Vertical-Slice Strategy for the first target
slice and prepares Phase C compatibility work.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_MANUAL_REVIEW_QUEUE_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_manual_review_queue.py
```

Tests:

```text
tests/test_first_slice_manual_review_queue.py
```

Outputs:

```text
outputs/target_slice/m35_b4_first_slice_manual_review_queue.json
outputs/target_slice/m35_b4_first_slice_manual_review_queue.csv
outputs/target_slice/m35_b4_first_slice_manual_review_queue.md
```

## Result

- selected-slice cards: `112`
- manual review queue: `6`
- unmapped feature requiring review: `bounce_to_hand`
- ready for Phase C: `true`

Queued cards:

```text
BT04-039TH
BT04-075TH
BT06-098TH
BT06-100TH
BT06-101TH
BT09-074TH
```

## Guardrails

- Queue export only.
- No automatic tag correction.
- No compatibility graph yet.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_manual_review_queue.py
python -m unittest tests.test_first_slice_manual_review_queue
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 126 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-C1`: Pair compatibility graph for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

# M35-C5 First Slice Selected Compatibility Output Closeout

## Summary

Implemented the selected-slice compatibility output for:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

This closes Phase C of the Hybrid Vertical-Slice Strategy for the first target
slice. The output combines C1 pair graph, C2 resource findings, C3 timing
findings, and C4 zone/target findings.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_SELECTED_COMPATIBILITY_OUTPUT_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_selected_compatibility_output.py
```

Tests:

```text
tests/test_first_slice_selected_compatibility_output.py
```

Outputs:

```text
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.json
outputs/target_slice/m35_c5_first_slice_selected_compatibility_output.md
```

## Result

- edges: `3919`
- M35-D1 candidate edges: `604`
- ready for M35-D1: `true`

Status counts:

```text
manual_review_required: 363
missing_data: 907
mixed: 2045
synergy: 604
```

Label counts:

```text
conflict: 2604
manual_review_required: 363
missing_data: 980
synergy: 3919
```

## Guardrails

- Advisory selected-slice compatibility output only.
- D1 candidates are inputs for package selection, not final deck choices.
- Manual-review edges are not promoted.
- Missing-data and conflict edges are not D1 clean candidates.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_selected_compatibility_output.py
python -m unittest tests.test_first_slice_selected_compatibility_output
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 169 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-D1`: Candidate package selection for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

# M35-D3 First Slice Combo Line Explainer Closeout

## Summary

Implemented combo line explanations for:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The explainer consumes M35-D1 packages, M35-D2 skeletons, and M35-C5 clean
compatibility edges. It writes human-reviewable combo line explanations for D4.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_COMBO_LINE_EXPLAINER_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_combo_line_explainer.py
```

Tests:

```text
tests/test_first_slice_combo_line_explainer.py
```

Outputs:

```text
outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json
outputs/target_slice/m35_d3_first_slice_combo_line_explainer.md
```

## Result

- source skeletons: `25`
- combo lines: `25`
- ready for M35-D4: `true`

## Guardrails

- Explanation only.
- Uses clean M35-C5 compatibility edges.
- No per-card quantities.
- No final play sequence legality claim.
- No bot/playbook export.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_combo_line_explainer.py
python -m unittest tests.test_first_slice_combo_line_explainer
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 190 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-D4`: Reviewed playbook seed export for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

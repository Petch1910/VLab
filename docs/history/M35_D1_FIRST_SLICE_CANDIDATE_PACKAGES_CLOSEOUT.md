# M35-D1 First Slice Candidate Packages Closeout

## Summary

Implemented candidate package selection for:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The selector consumes clean M35-C5 synergy edges and emits top anchor-based
candidate packages for D2 deck skeleton ratio planning.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_CANDIDATE_PACKAGES_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_candidate_packages.py
```

Tests:

```text
tests/test_first_slice_candidate_packages.py
```

Outputs:

```text
outputs/target_slice/m35_d1_first_slice_candidate_packages.json
outputs/target_slice/m35_d1_first_slice_candidate_packages.md
```

## Result

- clean synergy edges: `604`
- candidate packages: `25`
- ready for M35-D2: `true`

Top packages are intentionally package candidates only. They do not include
deck quantities or final deck slot choices.

## Guardrails

- Uses only clean M35-C5 synergy edges.
- Excludes manual-review, missing-data, and conflict edges.
- No deck skeleton.
- No deck quantities.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_candidate_packages.py
python -m unittest tests.test_first_slice_candidate_packages
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 176 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-D2`: Deck skeleton ratio planner for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

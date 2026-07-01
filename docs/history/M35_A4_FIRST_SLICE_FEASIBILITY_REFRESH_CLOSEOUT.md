# M35-A4 First Slice Feasibility Refresh Closeout

## Summary

Closed Phase A for the first selected Hybrid Vertical-Slice target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The refresh report combines M35-A2 target selection with M35-A3 minimal deck
legality fixtures.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_FEASIBILITY_REFRESH_SPEC.md
```

Tool:

```text
tools/deck/refresh_first_slice_feasibility.py
```

Tests:

```text
tests/test_first_slice_feasibility_refresh.py
```

Outputs:

```text
outputs/target_slice/m35_a4_first_slice_feasibility_refresh.json
outputs/target_slice/m35_a4_first_slice_feasibility_refresh.md
```

## Result

- Capacity ready: `true`
- Taxonomy ready: `true`
- Legality fixture ready: `true`
- Blocking gaps for Phase B: `none`
- Recommended next: `M35-B1`

Deferred before full official legality claim:

- official heal trigger maximum source fixture
- format-wide copy-limit exceptions beyond runtime `deck_limit`
- full official deck construction source citations

## Guardrails

- Offline Python tooling only.
- No Unity runtime change.
- No `GameState` mutation.
- No semantic ability tagging yet.
- No combo compatibility graph yet.
- No bot/playbook promotion.

## Verification

Passed:

```powershell
python tools\deck\refresh_first_slice_feasibility.py
python -m unittest tests.test_first_slice_feasibility_refresh
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 103 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-B1`: Semantic vocabulary for the selected Classic Core / โนว่า เกรปเปอร์
slice.

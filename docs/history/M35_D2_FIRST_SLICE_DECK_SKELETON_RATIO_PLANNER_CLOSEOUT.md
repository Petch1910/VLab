# M35-D2 First Slice Deck Skeleton Ratio Planner Closeout

## Summary

Implemented deck skeleton ratio planning for:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The planner consumes M35-D1 candidate packages and runtime SQLite card fields
to produce advisory skeleton ratio plans and package-local guard/shield
profiles.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_DECK_SKELETON_RATIO_PLANNER_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_deck_skeleton_ratio_planner.py
```

Tests:

```text
tests/test_first_slice_deck_skeleton_ratio_planner.py
```

Outputs:

```text
outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json
outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.md
```

## Result

- source packages: `25`
- skeletons: `25`
- cards loaded from SQLite: `44`
- ready for M35-D3: `true`

Ratio target:

```text
main deck: 50
trigger slots: 16
normal unit slots: 34
grade 0: 17
grade 1: 14
grade 2: 11
grade 3: 8
```

## Guardrails

- Ratio skeleton planner only.
- No per-card quantities.
- No final deck list.
- Guard/shield profile is package-local, not full-deck.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_deck_skeleton_ratio_planner.py
python -m unittest tests.test_first_slice_deck_skeleton_ratio_planner
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 183 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-D3`: Combo line explainer for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

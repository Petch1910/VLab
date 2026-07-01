# M35-A3 First Slice Deck Legality Fixtures Closeout

## Summary

Implemented minimal deck legality fixtures for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

This closes Phase A3 of the Hybrid Vertical-Slice Strategy.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_DECK_LEGALITY_FIXTURES_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_deck_fixtures.py
```

Tests:

```text
tests/test_first_slice_deck_fixtures.py
```

Outputs:

```text
outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.json
outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.md
```

## Fixture Result

Generated fixtures:

- `classic_core_selected_group_valid_minimal`: accepted
- `classic_core_selected_group_short_main`: rejected with `main_count`
- `classic_core_selected_group_bad_trigger_count`: rejected with `trigger_count`
- `classic_core_selected_group_missing_grade_3_setup`: rejected with
  `missing_setup_grade:3`
- `classic_core_selected_group_copy_limit_exceeded`: rejected with
  `copy_limit_exceeded`
- `classic_core_selected_group_identity_mismatch`: rejected with
  `identity_mismatch`

All fixture expectations passed.

## Guardrails

- Offline Python tooling only.
- No Unity runtime change.
- No `GameState` mutation.
- No semantic ability parsing.
- No bot/playbook promotion.
- No full official legality claim yet.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_deck_fixtures.py
python -m unittest tests.test_first_slice_deck_fixtures
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 98 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-A4`: First-slice feasibility report refresh with capacity,
legality-readiness, and missing-rule gates.

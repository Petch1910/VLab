# M36-03 Deck Recipe Validator Closeout

## Summary

`M36-03` validated the M36-02 recipe drafts against runtime SQLite card
metadata and M36 review blockers.

The key result is deliberately conservative: no recipe is runtime-ready yet.
This is expected because the single accepted-seed draft still has slot/trigger
gaps, and the other 24 drafts remain blocked by review.

## Results

- Recipes validated: `25`
- Runtime-ready recipes: `0`
- Validator passed: `0`
- Passed pending human acceptance: `0`
- Invalid drafts: `1`
- Blocked by review: `24`
- Slot-gap recipes: `16`
- Trigger-count mismatch recipes: `12`
- Missing-card recipes: `0`
- Copy-limit violation recipes: `0`
- Ready for M36-04: `true`

## Files

- Spec: `docs/specs/cards_and_decks/DECK_RECIPE_VALIDATOR_SPEC.md`
- Tool: `tools/deck/validate_deck_recipe_drafts.py`
- Tests: `tests/test_deck_recipe_validator.py`
- Output: `outputs/target_slice/m36_03_deck_recipe_validation_report.json`
- Output: `outputs/target_slice/m36_03_deck_recipe_validation_report.md`

## Preserved Boundaries

- No runtime deck creation.
- No bot integration.
- No live card text parsing.
- No direct `GameState` mutation.
- No automatic deck injection.

## Verification

```powershell
python tools\deck\validate_deck_recipe_drafts.py
python -m unittest tests.test_deck_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 257 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M36-04`: Combo-line to recipe consistency check.


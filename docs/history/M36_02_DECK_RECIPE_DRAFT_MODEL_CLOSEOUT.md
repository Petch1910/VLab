# M36-02 Deck Recipe Draft Model Closeout

## Summary

`M36-02` converted the first-slice advisory skeletons into reviewable deck
recipe drafts. Each draft now has explicit card quantities, review status,
blockers, slot summary, and validation metadata for the next validator step.

The result is still offline and advisory. It is not a runtime deck list.

## Results

- Recipe drafts: `25`
- Accepted seed recipes: `1`
- Rejected-line recipes: `24`
- Recipes with manual card overlap: `0`
- Recipes with slot gaps: `16`
- Ready for M36-03: `true`

Notable result: the accepted seed recipe from `line_003` / `skel_003` remains
blocked pending human acceptance and M36-03 validation, and currently exposes
`12` unfilled trigger slots.

## Files

- Spec: `docs/specs/cards_and_decks/DECK_RECIPE_DRAFT_MODEL_SPEC.md`
- Tool: `tools/deck/build_deck_recipe_draft_model.py`
- Tests: `tests/test_deck_recipe_draft_model.py`
- Output: `outputs/target_slice/m36_02_deck_recipe_draft_model.json`
- Output: `outputs/target_slice/m36_02_deck_recipe_draft_model.md`

## Preserved Boundaries

- No runtime deck creation.
- No bot integration.
- No deck validator claim yet.
- No live card text parsing.
- No direct `GameState` mutation.
- No automatic deck injection.

## Verification

```powershell
python tools\deck\build_deck_recipe_draft_model.py
python -m unittest tests.test_deck_recipe_draft_model
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 250 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M36-03`: Deck recipe validator.


# M36 Deck Recipe Validation Closeout

## Summary

`M36-closeout` closed the human-review-assisted deck recipe validation queue.
The result is intentionally conservative: M36 produced reviewable recipe
artifacts, but no recipe or combo line is ready for runtime promotion yet.

The next queue is `M37`, focused on repairing first-slice blockers before any
broader scale-out, runtime deck, or bot/playbook integration.

## Results

- Review items: `31`
- Accepted seed item: `1`
- Rejected combo-line items: `24`
- Manual card review items: `6`
- Recipe drafts: `25`
- Runtime-ready recipes: `0`
- Invalid drafts: `1`
- Blocked-by-review recipes: `24`
- Missing-card recipes: `0`
- Copy-limit violations: `0`
- Slot-gap recipes: `16`
- Trigger-count mismatch recipes: `12`
- Combo cards present: `25`
- Promotable combo lines: `0`
- Second slice future recipe-ready: `true`
- Runtime recipe promotion allowed: `false`

## Decision

```text
repair_first_slice_recipe_blockers_before_runtime_or_broader_scaleout
```

## Files

- Spec: `docs/specs/cards_and_decks/DECK_RECIPE_VALIDATION_CLOSEOUT_SPEC.md`
- Tool: `tools/deck/build_deck_recipe_validation_closeout.py`
- Tests: `tests/test_deck_recipe_validation_closeout.py`
- Output: `outputs/target_slice/m36_closeout_deck_recipe_validation.json`
- Output: `outputs/target_slice/m36_closeout_deck_recipe_validation.md`

## Verification

```powershell
python tools\deck\build_deck_recipe_validation_closeout.py
python -m unittest tests.test_deck_recipe_validation_closeout
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 281 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M37-01`: Accepted seed slot-gap completion candidates.


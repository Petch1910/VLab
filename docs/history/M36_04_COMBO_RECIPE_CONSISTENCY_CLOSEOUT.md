# M36-04 Combo-Line To Recipe Consistency Closeout

## Summary

`M36-04` checked every first-slice combo line against the M36-02 recipe drafts
and M36-03 validation statuses.

The important result is that all combo cards are present in their draft
recipes, but no line is promotable yet because review and validation blockers
remain.

## Results

- Combo lines checked: `25`
- Combo cards present: `25`
- Missing combo-card checks: `0`
- Manual-review dependency checks: `0`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Status counts: `24` blocked by review, `1` invalid recipe
- Ready for M36-05: `true`

## Files

- Spec: `docs/specs/cards_and_decks/COMBO_RECIPE_CONSISTENCY_SPEC.md`
- Tool: `tools/deck/check_combo_recipe_consistency.py`
- Tests: `tests/test_combo_recipe_consistency.py`
- Output: `outputs/target_slice/m36_04_combo_recipe_consistency_report.json`
- Output: `outputs/target_slice/m36_04_combo_recipe_consistency_report.md`

## Preserved Boundaries

- No runtime deck creation.
- No bot integration.
- No playbook promotion.
- No direct `GameState` mutation.
- No automatic deck injection.

## Verification

```powershell
python tools\deck\check_combo_recipe_consistency.py
python -m unittest tests.test_combo_recipe_consistency
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 264 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M36-05`: Second-slice readiness comparison.


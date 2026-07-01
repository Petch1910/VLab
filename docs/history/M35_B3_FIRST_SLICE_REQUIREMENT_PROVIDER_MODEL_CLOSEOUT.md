# M35-B3 First Slice Requirement / Provider Model Closeout

## Summary

Implemented the requirement/provider model for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

This converts M35-B2 advisory semantic tags into card-level requirement and
provider indexes for later compatibility graph work.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_REQUIREMENT_PROVIDER_MODEL_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_requirement_provider_model.py
```

Tests:

```text
tests/test_first_slice_requirement_provider_model.py
```

Outputs:

```text
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.md
```

## Result

- selected-slice cards: `112`
- cards with requirements: `94`
- cards with providers: `93`
- provider types: `17`
- requirement types: `16`
- manual review count: `6`
- ready for `M35-B4`: `true`

## Guardrails

- Advisory model only.
- No compatibility graph yet.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.
- No Unity runtime change.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_requirement_provider_model.py
python -m unittest tests.test_first_slice_requirement_provider_model
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 120 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-B4`: Manual review queue for unknown/low-confidence selected-slice cards.

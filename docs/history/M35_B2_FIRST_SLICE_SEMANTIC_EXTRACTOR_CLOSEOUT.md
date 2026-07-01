# M35-B2 First Slice Semantic Extractor Closeout

## Summary

Implemented the offline semantic extractor for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The extractor maps local runtime card text into the bounded `M35-B1`
vocabulary. Output is advisory only and does not execute effects.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_SEMANTIC_EXTRACTOR_SPEC.md
```

Tool:

```text
tools/deck/extract_first_slice_semantics.py
```

Tests:

```text
tests/test_first_slice_semantic_extractor.py
```

Outputs:

```text
outputs/target_slice/m35_b2_first_slice_semantic_tags.json
outputs/target_slice/m35_b2_first_slice_semantic_tags.md
```

## Result

- selected-slice cards: `112`
- cards with semantic tags: `112`
- manual review count: `6`
- excluded first-slice tags used: `none`
- ready for `M35-B3`: `true`

## Guardrails

- Offline advisory extraction only.
- No Unity runtime change.
- No `GameState` mutation.
- No runtime effect execution.
- No live card text parser.
- No structured card script generation.
- No bot/playbook promotion.

## Verification

Passed:

```powershell
python tools\deck\extract_first_slice_semantics.py
python -m unittest tests.test_first_slice_semantic_extractor
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 114 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-B3`: Requirement/provider model for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

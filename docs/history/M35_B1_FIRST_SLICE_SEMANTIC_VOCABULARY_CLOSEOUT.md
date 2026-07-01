# M35-B1 First Slice Semantic Vocabulary Closeout

## Summary

Implemented the Phase B1 semantic vocabulary for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The vocabulary is an allowed tag set for future offline extraction. It is not a
runtime effect schema.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_SEMANTIC_VOCABULARY_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_semantic_vocabulary.py
```

Tests:

```text
tests/test_first_slice_semantic_vocabulary.py
```

Outputs:

```text
outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.json
outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.md
```

## Result

- ability types: `3`
- zones: `10`
- timing tags: `18`
- condition tags: `12`
- cost tags: `7`
- effect tags: `13`
- duration tags: `5`
- mechanic groups: `6`
- trigger icons: `4`
- missing source terms: `none`
- ready for `M35-B2`: `true`

## Guardrails

- Offline advisory vocabulary only.
- No Unity runtime change.
- No `GameState` mutation.
- No runtime effect execution.
- No live card text parser.
- No bot/playbook promotion.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_semantic_vocabulary.py
python -m unittest tests.test_first_slice_semantic_vocabulary
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 108 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-B2`: Offline semantic extractor for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

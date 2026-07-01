# M36-05 Second-Slice Readiness Comparison Closeout

## Summary

`M36-05` compared the second selected slice (`Classic Core / Oracle Think Tank`)
against the first-slice M36 recipe validation pipeline.

The second slice is ready for future offline recipe work, but broader runtime
or bot scale-out remains disabled because the first slice still has no
runtime-ready recipe and no promotable combo line.

## Results

- First-slice runtime-ready recipes: `0`
- First-slice promotable combo lines: `0`
- Second-slice fixture ready: `true`
- Second-slice semantic probe ready: `true`
- Second-slice probe candidate edges: `259`
- Candidate edge ratio vs first slice: `0.429`
- Broader scale-out runtime allowed: `false`
- Ready for M36-closeout: `true`

Recommendation:

```text
second_slice_semantic_ready_but_hold_recipe_drafting_until_first_slice_review_blockers_clear
```

## Files

- Spec: `docs/specs/cards_and_decks/SECOND_SLICE_READINESS_COMPARISON_SPEC.md`
- Tool: `tools/deck/build_second_slice_readiness_comparison.py`
- Tests: `tests/test_second_slice_readiness_comparison.py`
- Output: `outputs/target_slice/m36_05_second_slice_readiness_comparison.json`
- Output: `outputs/target_slice/m36_05_second_slice_readiness_comparison.md`

## Verification

```powershell
python tools\deck\build_second_slice_readiness_comparison.py
python -m unittest tests.test_second_slice_readiness_comparison
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 271 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M36-closeout`: Deck recipe validation closeout.


# M35-C1 First Slice Pair Compatibility Graph Closeout

## Summary

Implemented the pair compatibility graph for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The graph connects provider cards to consumer cards using the M35-B3
requirement/provider model and carries M35-B4 manual-review gating forward.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_PAIR_COMPATIBILITY_GRAPH_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_pair_compatibility_graph.py
```

Tests:

```text
tests/test_first_slice_pair_compatibility_graph.py
```

Outputs:

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.md
```

## Result

- nodes: `112`
- edges: `3919`
- advisory edges: `3556`
- manual-review edges: `363`
- categories: `5`
- rules applied: `15`
- ready for M35-C2: `true`

Category counts:

```text
battle_pressure_support: 2227
board_support: 376
consistency_support: 541
resource_support: 369
trigger_support: 898
```

## Guardrails

- Advisory pair graph only.
- Manual-review cards are blocked from high-confidence compatibility.
- No resource conflict verdict yet.
- No timing compatibility verdict yet.
- No zone/target compatibility verdict yet.
- No deck skeleton.
- No bot/playbook promotion.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_pair_compatibility_graph.py
python -m unittest tests.test_first_slice_pair_compatibility_graph
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 134 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-C2`: Resource conflict detector for the selected Classic Core / โนว่า
เกรปเปอร์ slice.

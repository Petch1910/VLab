# M35-D4 First Slice Reviewed Playbook Seed Closeout

## Summary

Implemented static reviewed advisory playbook seed export for:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The exporter consumes M35-D3 combo line explanations, promotes only no-gap
lines into advisory seed entries, and preserves rejected lines with reasons.

## Added

Spec:

```text
docs/specs/cards_and_decks/FIRST_SLICE_REVIEWED_PLAYBOOK_SEED_SPEC.md
```

Tool:

```text
tools/deck/build_first_slice_reviewed_playbook_seed.py
```

Tests:

```text
tests/test_first_slice_reviewed_playbook_seed.py
```

Outputs:

```text
outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json
outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.md
```

## Result

- source combo lines: `25`
- seed entries: `1`
- rejected lines: `24`
- ready for M35-E1: `true`

Seed entry:

```text
seed_001 <- line_003 <- BT04-078TH
```

Rejected lines are retained with `unresolved_support_gap` reasons.

## Guardrails

- Static AI review only.
- Human acceptance required before runtime use.
- Not a runtime playbook.
- Not published to bot.
- Not auto-injected into player decks.
- Future bot use requires legal action mask and masked state.
- No runtime effect execution.

## Verification

Passed:

```powershell
python tools\deck\build_first_slice_reviewed_playbook_seed.py
python -m unittest tests.test_first_slice_reviewed_playbook_seed
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 198 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-E1`: Second slice selection.

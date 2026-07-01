# M35-E4 Bot Integration Gate Closeout

## Summary

`M35-E4` added an offline bot integration gate that allows only reviewed
`M35-D4` playbook seed entries to become future bot hint candidates. It also
explicitly blocks the generalized `M35-E3` semantic/compatibility probe edges
from runtime bot use.

No Unity runtime bot wiring was enabled.

## Results

- Reviewed hint candidates: `1`
- Blocked sources: `1`
- Gate passed: `true`
- Runtime bot integration enabled: `false`
- Allowed future hint candidate: `seed_001`, anchor `BT04-078TH`
- Blocked source: `M35-E3 semantic/compatibility probe`, `2660` edges, `259`
  candidate edges

## Files

- Spec: `docs/specs/cards_and_decks/BOT_INTEGRATION_GATE_SPEC.md`
- Tool: `tools/deck/build_bot_integration_gate.py`
- Tests: `tests/test_bot_integration_gate.py`
- Output: `outputs/target_slice/m35_e4_bot_integration_gate.json`
- Output: `outputs/target_slice/m35_e4_bot_integration_gate.md`

## Guardrails Preserved

- Bot may consume only reviewed hint candidates.
- Legal action mask is required before runtime use.
- Masked state view is required before runtime use.
- RulesCore command validation is required before runtime use.
- Direct `GameState` mutation is forbidden.
- True hidden state access is forbidden.
- Live card text parsing is forbidden.
- Unreviewed `M35-E3` probe edges are forbidden from runtime bot use.
- Automatic runtime/bot publication is forbidden.

## Verification

```powershell
python tools\deck\build_bot_integration_gate.py
python -m unittest tests.test_bot_integration_gate
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 228 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M35-closeout`: close the Hybrid Vertical-Slice Strategy status and select the
next implementation queue. Runtime bot wiring remains disabled until a separate
Unity/C# milestone opens with legal-action and masked-state tests.


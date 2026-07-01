# M35-A2 First Target Slice Closeout

## Summary

Implemented the Phase A2 report for the Hybrid Vertical-Slice Strategy.

The first target slice is:

```text
Slice: Classic Core
Era preset: classic_part1
Set scope: TD01-TD06 / BT01-BT09 / EB01-EB05
Selected group: โนว่า เกรปเปอร์
M34-03 rank: 5
Mechanic tier: OG Keywords
```

The tool deliberately does not choose Standard/DZ by default. It honors the
current user/team priority for early clan-era deck/combo work, then selects the
highest-ranked feasible group inside that slice.

## Added

Tool:

```text
tools/deck/select_first_target_slice.py
```

Tests:

```text
tests/test_first_target_slice_selection.py
```

Outputs:

```text
outputs/target_slice/m35_a2_first_target_slice_report.json
outputs/target_slice/m35_a2_first_target_slice_report.md
```

## Result

- Missing set codes: `none`
- Missing required taxonomy terms: `none`
- First-slice unsupported modules stay explicit: Legion, Stride, G Guardian,
  Imaginary Gift, Front/Over Trigger, Ride Deck, orders, gauge, crest, Energy,
  overDress/XoverDress, and Divine Skill.

## Guardrails

- Offline planning/tooling only.
- No Unity runtime change.
- No `GameState` mutation.
- No live card text parser.
- No bot/playbook promotion.
- No official full legality claim yet.

## Verification

Passed:

```powershell
python tools\deck\select_first_target_slice.py
python -m unittest tests.test_first_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```

Full Python suite result:

```text
Ran 93 tests
OK
```

Unity compile/EditMode were not run because this slice changes offline Python
tooling, outputs, and docs only.

## Next Target

`M35-A3`: Minimal deck legality fixtures for the selected Classic Core / โนว่า
เกรปเปอร์ slice.
